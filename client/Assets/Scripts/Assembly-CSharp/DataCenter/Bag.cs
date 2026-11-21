using System;
using Newtonsoft.Json;
using UnityEngine;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Bag : IBag
	{
		public int GetProp(PropID id)
		{
			int value = 0;
			Prop.TryGetValue(id.ToString(), out value);
			return value;
		}

		public int GetUseNum(PropID id)
		{
			int value = 0;
			useNum.TryGetValue(id.ToString(), out value);
			return value;
		}

		public int GetTotalUseNum(PropID id)
		{
			int value = 0;
			useTotalNum.TryGetValue(id.ToString(), out value);
			return value;
		}

		public override void ReadLocalData()
		{
			int value = 0;
			foreach (PropID item in default(PropID)._types())
			{
				string text = item.ToString();
				if (StorageMgr.IsHave("Bag.prop." + text))
				{
					prop.TryGetValue(text, out value);
					prop[text] = StorageMgr.GetInt("Bag.prop." + text, value);
				}
			}
		}

		public override void DelLocalData()
		{
			foreach (string key in prop.Keys)
			{
				StorageMgr.DelKey("Bag.prop." + key);
			}
		}

		public override void WriteLocalData()
		{
			foreach (string key in prop.Keys)
			{
				StorageMgr.SetInt("Bag.prop." + key, prop[key]);
			}
		}

		public void SetProp(string key, int value)
		{
			if (!prop.ContainsKey(key))
			{
				prop.Add(key, value);
			}
			else
			{
				prop[key] = value;
			}
			Asse.SaveForced("prop." + key, value);
		}

		private void MdoityItemUseNum(PropID id, int count)
		{
			if (count >= 0)
			{
				return;
			}
			string key = id.ToString();
			if (!UseNum.ContainsKey(key))
			{
				UseNum[key] = -count;
			}
			else
			{
				UseNum[key] = UseNum[key] - count;
			}
			if (!UseTotalNum.ContainsKey(key))
			{
				UseTotalNum[key] = -count;
			}
			else
			{
				UseTotalNum[key] = UseTotalNum[key] - count;
			}
		}

		private void _DispatcherUpdateProp(PropID id, int old_count, int new_count)
		{
			int num = new_count;
			if (id == PropID.Energy)
			{
				num = Mathf.Min(new_count, cf_const.maxEnergyUpperLimit);
			}
			DispatcherMgr.Emit("Update" + id.ToString(), old_count, num);
		}

		private void _Modify(PropID id, int old_count, int new_count, bool b_update, string s_place)
		{
			string key = id.ToString();
			if (b_update)
			{
				_DispatcherUpdateProp(id, old_count, new_count);
			}
			if (!string.IsNullOrEmpty(s_place))
			{
				DispatcherMgr.Emit("TrackUpdateItem", id, old_count, new_count, s_place);
			}
			SetProp(key, new_count);
		}

		public int Modify(PropID id, int n_count, string s_place)
		{
			return Modify(id, n_count, true, s_place);
		}

		public int Modify(PropID id, int n_count, bool b_update = true, string s_place = "")
		{
			if (id == PropID.EnergyRecoverUpLimit)
			{
				return 0;
			}
			if (id == PropID.Medal)
			{
				ModifyMedal(n_count);
				return 0;
			}
			if (id != PropID.Energy)
			{
				MdoityItemUseNum(id, n_count);
				int prop = GetProp(id);
				_Modify(id, prop, prop + n_count, b_update, s_place);
				return prop;
			}
			int prop2 = GetProp(PropID.Energy);
			int num = prop2 + n_count;
			if (num >= 0)
			{
				bool isDeduct;
				if (n_count < 0 && JudgeUnlimitedEnergy())
				{
					isDeduct = true;
					num = 1000;
				}
				else
				{
					num = Mathf.Min(num, cf_const.maxEnergyUpperLimit);
					_Modify(PropID.Energy, prop2, num, b_update, s_place);
					isDeduct = n_count < 0;
				}
				_energyEvent(isDeduct, num);
			}
			return prop2;
		}

		public void ModifyShow(PropID id, int n_raw, int n_count)
		{
			_DispatcherUpdateProp(id, n_raw, n_count + n_raw);
		}

		public void ModifyShow(PropID id, ref int n_raw, int n_count)
		{
			_DispatcherUpdateProp(id, n_raw, n_raw + n_count);
			n_raw += n_count;
		}

		public void ModifyShow(PropID id, int original)
		{
			_DispatcherUpdateProp(id, original, GetProp(id));
		}

		public void ModifyShow(PropID id)
		{
			int prop = GetProp(id);
			_DispatcherUpdateProp(id, prop, prop);
		}

		public int ModifyEnergy(int n_count, bool b_update = true, string s_place = "")
		{
			int prop = GetProp(PropID.Energy);
			int num = prop + n_count;
			if (num < 0)
			{
				return prop;
			}
			bool isDeduct;
			if (n_count < 0 && JudgeUnlimitedEnergy())
			{
				isDeduct = true;
				num = 1000;
			}
			else
			{
				num = Mathf.Min(num, cf_const.maxEnergyUpperLimit);
				_Modify(PropID.Energy, prop, num, b_update, s_place);
				isDeduct = n_count < 0;
			}
			_energyEvent(isDeduct, num);
			return prop;
		}

		public bool JudgeUnlimitedEnergy()
		{
			if (cf_const.unlimitedEnergyLv != -1 && cf_const.unlimitedEnergyLv <= DB.User.Maxlv)
			{
				return GetEnergyUnlimitedTime() > 0;
			}
			return true;
		}

		public bool JudgeForceUnlimitedEnergy()
		{
			if (cf_const.unlimitedEnergyLv == -1)
			{
				return true;
			}
			return DB.User.Maxlv < cf_const.unlimitedEnergyLv;
		}

		public bool JudgeHasEnergy()
		{
			if (JudgeUnlimitedEnergy())
			{
				return true;
			}
			return GetProp(PropID.Energy) > 0;
		}

		private void _energyEvent(bool isDeduct, int value)
		{
			if (isDeduct)
			{
				LogEvent.I.Track("Deduct_Energy").Add("Remaining_energy", value).CommonAttr()
					.Send();
			}
		}

		public void AddEnergyUnlimitedTime(float second)
		{
			DateTime now = DateTime.Now;
			DateTime dateTime = now;
			if (!string.IsNullOrEmpty(EnergyUnlimitedTime))
			{
				dateTime = MDateTime.Parse(EnergyUnlimitedTime);
			}
			DateTime dateTime2 = dateTime;
			if (dateTime < now)
			{
				dateTime2 = now;
			}
			EnergyUnlimitedTime = dateTime2.AddSeconds(second).ToString();
			int prop = GetProp(PropID.Energy);
			int maxEnergyRecover = cf_const.maxEnergyRecover;
			if (prop < maxEnergyRecover)
			{
				_Modify(PropID.Energy, prop, maxEnergyRecover, false, "Stores");
			}
			DispatcherMgr.Emit(string.Format("ForceUpdate{0}", PropID.Energy));
		}

		public int GetEnergyUnlimitedTime()
		{
			if (!string.IsNullOrEmpty(EnergyUnlimitedTime))
			{
				DateTime now = DateTime.Now;
				DateTime dateTime = MDateTime.Parse(EnergyUnlimitedTime);
				if (!(dateTime <= now))
				{
					return (int)(dateTime - now).TotalSeconds;
				}
			}
			return 0;
		}

		public int GetMaxEnergyRecover()
		{
			return Mathf.Max(GetProp(PropID.EnergyRecoverUpLimit), cf_const.maxEnergyRecover);
		}

		public void RestoreEnergyRecoverUpLimit()
		{
			int maxEnergyRecover = cf_const.maxEnergyRecover;
			DispatcherMgr.Emit("Update_EnergyRecoverUpLimit_Change");
			SetProp(PropID.EnergyRecoverUpLimit.ToString(), maxEnergyRecover);
		}

		public int ModifyEnergyRecoverUpLimit(int va)
		{
			int prop = GetProp(PropID.EnergyRecoverUpLimit);
			if (prop < va)
			{
				DispatcherMgr.Emit("Update_EnergyRecoverUpLimit_Change");
				SetProp(PropID.EnergyRecoverUpLimit.ToString(), va);
				prop = va;
			}
			return prop;
		}

		public int ModifyMedal(int medalId)
		{
			if (Medal.Contains(medalId))
			{
				return 0;
			}
			Medal.Add(medalId);
			NewGetMedals.Add(medalId);
			DispatcherMgr.Emit("Update_User_MedalRed");
			return 0;
		}

		public void ReissueMedal(int medalId)
		{
			if (!Medal.Contains(medalId))
			{
				Medal.Add(medalId);
			}
		}
	}
}
