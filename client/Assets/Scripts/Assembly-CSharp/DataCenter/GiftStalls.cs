using System;
using Newtonsoft.Json;
using UnityEngine;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class GiftStalls : IGiftStalls
	{
		public int GetStalls()
		{
			if (Stalls == 0)
			{
				SetStalls(cf_const.giftStallsDefault);
			}
			else if (!CheckDownStalls())
			{
				int num = Mathf.Clamp(Stalls, cf_const.giftStallsMin, cf_const.giftStallsMax);
				if (num != Stalls)
				{
					Stalls = num;
				}
			}
			return Stalls;
		}

		public void SetStalls(int value)
		{
			if (cf_const.giftStallsMin != 0 && cf_const.giftStallsMax != 0)
			{
				int num = Mathf.Clamp(value, cf_const.giftStallsMin, cf_const.giftStallsMax);
				if (Stalls != num)
				{
					Debug.Log(string.Format("重置礼包档位、刷新时间、购买次数 stalls:{0}", value));
					Stalls = num;
					BuyCount = 0;
					SetRefreshTime(Stalls);
					GiftTypeIndexData.Clear();
				}
			}
			else
			{
				Debug.LogError("礼包最小或最大档位配置不正确，请检查cf_const配置");
			}
		}

		private void SetRefreshTime(int stall)
		{
			if (cf_const.giftStallsDownTime != null)
			{
				int[] array = cf_const.giftStallsDownTime.ToInt('|');
				if (array.Length < stall)
				{
					Debug.LogError(string.Format("当前礼包档位为{0}，未找到超时配置，请检查cf_const表的giftStallsDownTime字段", stall));
				}
				else
				{
					RefreshTime = DateTime.Now.AddHours(array[stall - 1]);
				}
			}
			else
			{
				Debug.LogError("giftStallsDownTime字段为空，请检查cf_const配置");
			}
		}

		public int GetShowGiftButtonDataByIndex(int index)
		{
			string key = index.ToString();
			if (!ShowGiftButtonData.ContainsKey(key))
			{
				return 0;
			}
			return ShowGiftButtonData[key];
		}

		public void SetShowGiftButtonDataByIndex(int index, int value)
		{
			SetShowGiftButtonData(index.ToString(), value);
		}

		public int GetShowButtonTypeDataByIndex(int index)
		{
			string key = index.ToString();
			if (!ShowGiftButtonTypeData.ContainsKey(key))
			{
				return 0;
			}
			return ShowGiftButtonTypeData[key];
		}

		public void SetShowGiftButtonTypeDataByIndex(int index, int value)
		{
			SetShowGiftButtonTypeData(index.ToString(), value);
		}

		public void AddBuyCount()
		{
			BuyCount++;
			(bool isUp, int stall) = CheckUpStalls();
			if (!isUp)
			{
				SetRefreshTime(stall);
			}
		}

		private (bool, int) CheckUpStalls()
		{
			int stalls = GetStalls();
			if (cf_const.giftStallsUpCondition == null)
			{
				Debug.LogError("giftStallsUpCondition字段为空，请检查cf_const配置");
				return (false, stalls);
			}
			int[] array = cf_const.giftStallsUpCondition.ToInt('|');
			if (array.Length < stalls)
			{
				Debug.LogError(string.Format("当前礼包档位为{0}，升档条件配置长度{1}", stalls, array.Length));
				return (false, stalls);
			}
			int condition = array[stalls - 1];
			bool flag = condition <= BuyCount;
			if (flag)
			{
				SetStalls(stalls + 1);
			}
			return (flag, stalls);
		}

		private bool CheckDownStalls()
		{
			bool flag = DateTime.Now > RefreshTime;
			if (flag)
			{
				SetStalls(Stalls - 1);
			}
			return flag;
		}

		public void AddRecordStoreIndex(int storeIndex)
		{
			if (!RecordStoreIndexData.Contains(storeIndex))
			{
				RecordStoreIndexData.Add(storeIndex);
			}
		}

		public bool IsRecordStoreIndex(int storeIndex)
		{
			return RecordStoreIndexData.Contains(storeIndex);
		}

		public void RemoveStoreIndex(int index)
		{
			RecordStoreIndexData.Remove(index);
		}

		public GiftData GetGiftDataById(int id)
		{
			string key = id.ToString();
			if (!RecordGiftData.ContainsKey(key))
			{
				return null;
			}
			return RecordGiftData[key];
		}

		private void SetGiftDataById(int id, GiftData data)
		{
			SetRecordGiftData(id.ToString(), data);
		}

		public GiftData AddGiftData(cfbt_Gift cfg)
		{
			GiftData giftData = new GiftData();
			giftData.Init(cfg);
			SetRecordGiftData(cfg.id.ToString(), giftData);
			return giftData;
		}

		public GiftData FindAndRefresh(cfbt_Gift cfg, bool bIsForce = false)
		{
			GiftData giftData = GetGiftDataById(cfg.id);
			if (giftData == null)
			{
				return null;
			}
			if (cfg.isLoop)
			{
				DateTime now = DateTime.Now;
				if (!bIsForce && cfg.continueTime != -1)
				{
					if (giftData.isBuy && cfg.isOnceBuy)
					{
						return giftData;
					}
					if (giftData.timer > now)
					{
						return giftData;
					}
					if (giftData.day == DateTime.Now.DayOfYear)
					{
						return giftData;
					}
				}
				giftData.Init(cfg);
				SetRecordGiftData(cfg.id.ToString(), giftData);
				return giftData;
			}
			return giftData;
		}

		public void SetGiftDataBuy(cfbt_Gift cfg)
		{
			GiftData giftDataById = GetGiftDataById(cfg.id);
			giftDataById.isBuy = true;
			SetRecordGiftData(cfg.id.ToString(), giftDataById);
		}

		public void SetGiftDataPop(cfbt_Gift cfg)
		{
			GiftData giftDataById = GetGiftDataById(cfg.id);
			if (giftDataById.nPop > 0)
			{
				giftDataById.nPop--;
			}
			SetRecordGiftData(cfg.id.ToString(), giftDataById);
		}

		public int GetGiftTypeIndexData(GiftTriggerType type)
		{
			string key = ((int)type).ToString();
			if (!GiftTypeIndexData.ContainsKey(key))
			{
				return -1;
			}
			return GiftTypeIndexData[key];
		}

		public int GetGiftUnlockData(int giftId)
		{
			string key = giftId.ToString();
			if (!GiftUnlockData.ContainsKey(key))
			{
				return 0;
			}
			return GiftUnlockData[key];
		}

		public void AddGiftBuyData(int giftId, int storeIndex)
		{
			string key = giftId.ToString();
			string text;
			if (!GiftBuyData.ContainsKey(key))
			{
				text = storeIndex.ToString();
			}
			else
			{
				text = GiftBuyData[key] + "," + storeIndex;
			}
			DB.GiftStalls.SetGiftBuyData(key, text);
		}

		public void RemoveGiftBuyData(int giftId)
		{
			string key = giftId.ToString();
			if (GiftUnlockData.ContainsKey(key))
			{
				GiftUnlockData.Remove(key);
			}
		}

		public void CheckOfflineTime()
		{
			int num = Convert.ToInt32((DateTime.Today - LastLoseFocusTime).TotalDays);
			int num2 = Math.Max(0, num - 1);
			if (OfflineDays < num2)
			{
				OfflineDays = num2;
			}
			LastLoseFocusTime = DateTime.Today;
		}
	}
}
