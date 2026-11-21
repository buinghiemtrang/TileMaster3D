using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Achieve : IAchieve
	{
		public int GetReachData(AchieveType type)
		{
			string key = type.ToString();
			if (!DB.Achieve.ReachDatas.ContainsKey(key))
			{
				return 0;
			}
			return DB.Achieve.ReachDatas[key];
		}

		public void SetReachData(AchieveType type, int value)
		{
			SetReachDatas(type.ToString(), value);
		}

		public int GetAchiveData(int cfgId)
		{
			string key = cfgId.ToString();
			if (!DB.Achieve.AchieveDatas.ContainsKey(key))
			{
				return 0;
			}
			return DB.Achieve.AchieveDatas[key];
		}

		public void SetAchieveData(int cfgId, int level)
		{
			SetAchieveDatas(cfgId.ToString(), level);
		}

		public bool GetTipsData(int cfgId)
		{
			string key = cfgId.ToString();
			if (!DB.Achieve.TipsDatas.ContainsKey(key))
			{
				return false;
			}
			return DB.Achieve.TipsDatas[key];
		}

		public void SetTipsData(int cfgId, bool bIsNotshow)
		{
			SetTipsDatas(cfgId.ToString(), bIsNotshow);
		}
	}
}
