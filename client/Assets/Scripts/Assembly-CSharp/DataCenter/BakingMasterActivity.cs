using System.Collections.Generic;
using Game.Activity.BakingMaster;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class BakingMasterActivity : IBakingMasterActivity
	{
		private int[] _layerIndex = new int[10] { 1, 5, 6, 9, 10, 12, 13, 14, 15, 15 };

		public void SaveUnlockLayerStatus()
		{
			Asse.Save("unlockLayerStatus", unlockLayerStatus);
		}

		private void _setMatchDataOne(RequestMatchData d)
		{
			d.ParseExtension();
			if (d.levelId == 0)
			{
				loser.Add(d);
			}
			else
			{
				List<RequestMatchData> levelData = GetLevelData(d.levelId - 1);
				if (levelData != null)
				{
					levelData.Add(d);
				}
			}
		}

		public void SetMatchData(List<RequestMatchData> datas)
		{
			for (int i = 0; i < levels.Length; i++)
			{
				levels[i].data.Clear();
			}
			loser.Clear();
			if (datas != null)
			{
				foreach (RequestMatchData data in datas)
				{
					_setMatchDataOne(data);
				}
				for (int j = 0; j < levels.Length; j++)
				{
					levels[j].data.Sort((RequestMatchData a, RequestMatchData b) => a.grade - b.grade);
				}
				Loser.Save();
				SaveLevels();
			}
		}

		public void UpdateOwnGrade(RequestMatchData d)
		{
			if (d != null)
			{
				if (d.levelId != 0)
				{
					List<RequestMatchData> levelData = GetLevelData(d.levelId - 1);
					if (levelData != null)
					{
						int num = levelData.FindIndex((RequestMatchData a) => a.userId == d.userId);
						if (num == -1)
						{
							levelData.Add(d);
						}
						else
						{
							levelData[num] = d;
						}
						levelData.Sort((RequestMatchData a, RequestMatchData b) => a.grade - b.grade);
						SaveLevels();
					}
				}
				else
				{
					if (!loser.Exists((RequestMatchData a) => a.userId == d.userId))
					{
						Loser.Add(d);
					}
					ClearFailureData();
				}
			}
		}

		public List<RequestMatchData> GetLevelData(int level)
		{
			if (level >= 0 && level < levels.Length)
			{
				return levels[level].data;
			}
			return null;
		}

		public void SetFailureData()
		{
			gameFailure.Issue = info.Issue;
			gameFailure.LevelId = 0;
			gameFailure.Grade = 0;
			SaveGameFailure();
		}

		public void ClearFailureData()
		{
			DB.BakingMasterActivity.Data.reviveCount++;
			DB.BakingMasterActivity.SaveData();
			gameFailure.Issue = string.Empty;
			SaveGameFailure();
		}

		public bool IsUnlockLayer(int id)
		{
			if (id == 1)
			{
				return true;
			}
			int num = (id - 1) * 2;
			int num2 = _layerIndex[num - 2];
			int num3 = _layerIndex[num - 1];
			if (num2 - 1 < num3)
			{
				List<RequestMatchData> levelData = GetLevelData(num2 - 1);
				bool flag = false;
				while (levelData != null && levelData.Count != 0)
				{
					flag = num3 <= num2;
					if (num3 == num2)
					{
						break;
					}
					levelData = GetLevelData(num2);
					num2++;
				}
				return flag;
			}
			return true;
		}

		public bool IsCompleteLayer(int id)
		{
			int num = id * 2 - 2;
			int num2 = _layerIndex[num];
			int num3 = _layerIndex[num + 1];
			if (num2 - 1 < num3)
			{
				List<RequestMatchData> levelData = GetLevelData(num2 - 1);
				bool flag = false;
				while (levelData != null && levelData.Count != 0)
				{
					flag = num3 <= num2;
					if (num3 == num2)
					{
						break;
					}
					levelData = GetLevelData(num2);
					num2++;
				}
				return flag;
			}
			return true;
		}

		public bool IsLoser(string userId)
		{
			return loser.Exists((RequestMatchData a) => a.userId == userId);
		}

		public bool IsTeamUser()
		{
			return info.myTeam != null;
		}
	}
}
