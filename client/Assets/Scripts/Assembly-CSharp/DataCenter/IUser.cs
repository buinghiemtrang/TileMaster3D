using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IUser : AbsDataModel
	{
		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string languageid;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected bool bMusic;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected bool bSound;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected bool bVibrate;

		[DBValue(DBStoreType.None, DBPutType.Local)]
		[JsonProperty]
		protected int curlv;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int maxlv;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int newlv;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int lastlv;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected bool adFree;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected bool isPayingUsers;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int firstConsumeLv;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int sumStar;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int chestModelProbability;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int bombModelProbability;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int surReviveAdNum;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int surHintAdNum;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int surFreezeAdNum;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int surFreeGoldNum;

		[DBValue(DBStoreType.None, DBPutType.Local)]
		[JsonProperty]
		protected int sumStarLevel;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int surBackAdNum;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int surRefreshAdNum;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string adNumResetTime;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int lvRepeatDifficulty;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int lvDifficulty;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected bool curLvIsTimeoutFailed;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float currentLevelFirstFailedModelProportion;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int dynamicDifficultyPoints;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<int, int> dynamicDifficultyPointsChangeValue;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int dynamicDifficultyLevel;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected bool bShowBackGuide;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected float totalConsume;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<string, LvTime> lvTimeData;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int surMagnetAdNum;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected bool isUseMagnet;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int lvMedal;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<string, int> MonthIssueSumStar;

		[DBValue(DBStoreType.None, DBPutType.Local)]
		[JsonProperty]
		protected string monthIssue;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int medal;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected bool isFirstEnterMedal;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected bool isChangedMedal;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected bool isSkipPigAnim;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<string, int> monthIssueLevel;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int randomLevelWaveWeight;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int showElementWholeGuideStatus;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int useGoldReviveNumberTimes;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected bool isShowFestivalSkin;

		public IUser()
		{
			InitData();
		}

		public virtual string Languageid
		{
			get
			{
				return languageid;
			}
			set
			{
				if (string.Equals(languageid, value))
				{
					return;
				}
				languageid = value;
				Asse.Save("languageid", value);
			}
		}

		public virtual bool BMusic
		{
			get
			{
				return bMusic;
			}
			set
			{
				if (bMusic != value)
				{
					bMusic = value;
					Asse.Save("bMusic", value);
				}
			}
		}

		public virtual bool BSound
		{
			get
			{
				return bSound;
			}
			set
			{
				if (bSound != value)
				{
					bSound = value;
					Asse.Save("bSound", value);
				}
			}
		}

		public virtual bool BVibrate
		{
			get
			{
				return bVibrate;
			}
			set
			{
				if (bVibrate != value)
				{
					bVibrate = value;
					Asse.Save("bVibrate", value);
				}
			}
		}

		public virtual int Curlv
		{
			get
			{
				return curlv;
			}
			set
			{
				curlv = value;
			}
		}

		public virtual int Maxlv
		{
			get
			{
				return maxlv;
			}
			set
			{
				if (maxlv != value)
				{
					maxlv = value;
					Asse.Save("maxlv", value);
				}
			}
		}

		public virtual int Newlv
		{
			get
			{
				return newlv;
			}
			set
			{
				if (newlv != value)
				{
					newlv = value;
					Asse.Save("newlv", value);
				}
			}
		}

		public virtual int Lastlv
		{
			get
			{
				return lastlv;
			}
			set
			{
				if (lastlv != value)
				{
					lastlv = value;
					Asse.Save("lastlv", value);
				}
			}
		}

		public virtual bool AdFree
		{
			get
			{
				return adFree;
			}
			set
			{
				if (adFree != value)
				{
					adFree = value;
					Asse.Save("adFree", value);
				}
			}
		}

		public virtual bool IsPayingUsers
		{
			get
			{
				return isPayingUsers;
			}
			set
			{
				if (isPayingUsers != value)
				{
					isPayingUsers = value;
					Asse.Save("isPayingUsers", value);
				}
			}
		}

		public virtual int FirstConsumeLv
		{
			get
			{
				return firstConsumeLv;
			}
			set
			{
				if (firstConsumeLv != value)
				{
					firstConsumeLv = value;
					Asse.Save("firstConsumeLv", value);
				}
			}
		}

		public virtual int SumStar
		{
			get
			{
				return sumStar;
			}
			set
			{
				if (sumStar != value)
				{
					sumStar = value;
					Asse.Save("sumStar", value);
				}
			}
		}

		public virtual int ChestModelProbability
		{
			get
			{
				return chestModelProbability;
			}
			set
			{
				if (chestModelProbability != value)
				{
					chestModelProbability = value;
					Asse.Save("chestModelProbability", value);
				}
			}
		}

		public virtual int BombModelProbability
		{
			get
			{
				return bombModelProbability;
			}
			set
			{
				if (bombModelProbability != value)
				{
					bombModelProbability = value;
					Asse.Save("bombModelProbability", value);
				}
			}
		}

		public virtual int SurReviveAdNum
		{
			get
			{
				return surReviveAdNum;
			}
			set
			{
				if (surReviveAdNum != value)
				{
					surReviveAdNum = value;
					Asse.Save("surReviveAdNum", value);
				}
			}
		}

		public virtual int SurHintAdNum
		{
			get
			{
				return surHintAdNum;
			}
			set
			{
				if (surHintAdNum != value)
				{
					surHintAdNum = value;
					Asse.Save("surHintAdNum", value);
				}
			}
		}

		public virtual int SurFreezeAdNum
		{
			get
			{
				return surFreezeAdNum;
			}
			set
			{
				if (surFreezeAdNum != value)
				{
					surFreezeAdNum = value;
					Asse.Save("surFreezeAdNum", value);
				}
			}
		}

		public virtual int SurFreeGoldNum
		{
			get
			{
				return surFreeGoldNum;
			}
			set
			{
				if (surFreeGoldNum != value)
				{
					surFreeGoldNum = value;
					Asse.Save("surFreeGoldNum", value);
				}
			}
		}

		public virtual int SumStarLevel
		{
			get
			{
				return sumStarLevel;
			}
			set
			{
				sumStarLevel = value;
			}
		}

		public virtual int SurBackAdNum
		{
			get
			{
				return surBackAdNum;
			}
			set
			{
				if (surBackAdNum != value)
				{
					surBackAdNum = value;
					Asse.Save("surBackAdNum", value);
				}
			}
		}

		public virtual int SurRefreshAdNum
		{
			get
			{
				return surRefreshAdNum;
			}
			set
			{
				if (surRefreshAdNum != value)
				{
					surRefreshAdNum = value;
					Asse.Save("surRefreshAdNum", value);
				}
			}
		}

		public virtual string AdNumResetTime
		{
			get
			{
				return adNumResetTime;
			}
			set
			{
				if (string.Equals(adNumResetTime, value))
				{
					return;
				}
				adNumResetTime = value;
				Asse.Save("adNumResetTime", value);
			}
		}

		public virtual int LvRepeatDifficulty
		{
			get
			{
				return lvRepeatDifficulty;
			}
			set
			{
				if (lvRepeatDifficulty != value)
				{
					lvRepeatDifficulty = value;
					Asse.Save("lvRepeatDifficulty", value);
				}
			}
		}

		public virtual int LvDifficulty
		{
			get
			{
				return lvDifficulty;
			}
			set
			{
				if (lvDifficulty != value)
				{
					lvDifficulty = value;
					Asse.Save("lvDifficulty", value);
				}
			}
		}

		public virtual bool CurLvIsTimeoutFailed
		{
			get
			{
				return curLvIsTimeoutFailed;
			}
			set
			{
				if (curLvIsTimeoutFailed != value)
				{
					curLvIsTimeoutFailed = value;
					Asse.Save("curLvIsTimeoutFailed", value);
				}
			}
		}

		public virtual float CurrentLevelFirstFailedModelProportion
		{
			get
			{
				return currentLevelFirstFailedModelProportion;
			}
			set
			{
				if (currentLevelFirstFailedModelProportion != value)
				{
					currentLevelFirstFailedModelProportion = value;
					Asse.Save("currentLevelFirstFailedModelProportion", value);
				}
			}
		}

		public virtual int DynamicDifficultyPoints
		{
			get
			{
				return dynamicDifficultyPoints;
			}
			set
			{
				if (dynamicDifficultyPoints != value)
				{
					dynamicDifficultyPoints = value;
					Asse.Save("dynamicDifficultyPoints", value);
				}
			}
		}

		public virtual Dictionary<int, int> DynamicDifficultyPointsChangeValue
		{
			get
			{
				return dynamicDifficultyPointsChangeValue;
			}
			set
			{
				dynamicDifficultyPointsChangeValue = value;
				Asse.Save("dynamicDifficultyPointsChangeValue", value);
			}
		}

		public virtual int DynamicDifficultyLevel
		{
			get
			{
				return dynamicDifficultyLevel;
			}
			set
			{
				if (dynamicDifficultyLevel != value)
				{
					dynamicDifficultyLevel = value;
					Asse.Save("dynamicDifficultyLevel", value);
				}
			}
		}

		public virtual bool BShowBackGuide
		{
			get
			{
				return bShowBackGuide;
			}
			set
			{
				if (bShowBackGuide != value)
				{
					bShowBackGuide = value;
					Asse.Save("bShowBackGuide", value);
				}
			}
		}

		public virtual float TotalConsume
		{
			get
			{
				return totalConsume;
			}
			set
			{
				if (totalConsume != value)
				{
					totalConsume = value;
					Asse.Save("totalConsume", value);
				}
			}
		}

		public virtual Dictionary<string, LvTime> LvTimeData
		{
			get
			{
				return lvTimeData;
			}
			set
			{
				lvTimeData = value;
				Asse.Save("lvTimeData", value);
			}
		}

		public virtual int SurMagnetAdNum
		{
			get
			{
				return surMagnetAdNum;
			}
			set
			{
				if (surMagnetAdNum != value)
				{
					surMagnetAdNum = value;
					Asse.Save("surMagnetAdNum", value);
				}
			}
		}

		public virtual bool IsUseMagnet
		{
			get
			{
				return isUseMagnet;
			}
			set
			{
				if (isUseMagnet != value)
				{
					isUseMagnet = value;
					Asse.Save("isUseMagnet", value);
				}
			}
		}

		public virtual int LvMedal
		{
			get
			{
				return lvMedal;
			}
			set
			{
				if (lvMedal != value)
				{
					lvMedal = value;
					Asse.Save("lvMedal", value);
				}
			}
		}

		public virtual Dictionary<string, int> MonthIssueSumStar_
		{
			get
			{
				return MonthIssueSumStar;
			}
			set
			{
				MonthIssueSumStar = value;
				Asse.Save("MonthIssueSumStar", value);
			}
		}

		public virtual string MonthIssue
		{
			get
			{
				return monthIssue;
			}
			set
			{
				monthIssue = value;
			}
		}

		public virtual int Medal
		{
			get
			{
				return medal;
			}
			set
			{
				if (medal != value)
				{
					medal = value;
					Asse.Save("medal", value);
				}
			}
		}

		public virtual bool IsFirstEnterMedal
		{
			get
			{
				return isFirstEnterMedal;
			}
			set
			{
				if (isFirstEnterMedal != value)
				{
					isFirstEnterMedal = value;
					Asse.Save("isFirstEnterMedal", value);
				}
			}
		}

		public virtual bool IsChangedMedal
		{
			get
			{
				return isChangedMedal;
			}
			set
			{
				if (isChangedMedal != value)
				{
					isChangedMedal = value;
					Asse.Save("isChangedMedal", value);
				}
			}
		}

		public virtual bool IsSkipPigAnim
		{
			get
			{
				return isSkipPigAnim;
			}
			set
			{
				if (isSkipPigAnim != value)
				{
					isSkipPigAnim = value;
					Asse.Save("isSkipPigAnim", value);
				}
			}
		}

		public virtual Dictionary<string, int> MonthIssueLevel
		{
			get
			{
				return monthIssueLevel;
			}
			set
			{
				monthIssueLevel = value;
				Asse.Save("monthIssueLevel", value);
			}
		}

		public virtual int RandomLevelWaveWeight
		{
			get
			{
				return randomLevelWaveWeight;
			}
			set
			{
				if (randomLevelWaveWeight != value)
				{
					randomLevelWaveWeight = value;
					Asse.Save("randomLevelWaveWeight", value);
				}
			}
		}

		public virtual int ShowElementWholeGuideStatus
		{
			get
			{
				return showElementWholeGuideStatus;
			}
			set
			{
				if (showElementWholeGuideStatus != value)
				{
					showElementWholeGuideStatus = value;
					Asse.Save("showElementWholeGuideStatus", value);
				}
			}
		}

		public virtual int UseGoldReviveNumberTimes
		{
			get
			{
				return useGoldReviveNumberTimes;
			}
			set
			{
				if (useGoldReviveNumberTimes != value)
				{
					useGoldReviveNumberTimes = value;
					Asse.Save("useGoldReviveNumberTimes", value);
				}
			}
		}

		public virtual bool IsShowFestivalSkin
		{
			get
			{
				return isShowFestivalSkin;
			}
			set
			{
				if (isShowFestivalSkin != value)
				{
					isShowFestivalSkin = value;
					Asse.Save("isShowFestivalSkin", value);
				}
			}
		}

		public virtual void SetDynamicDifficultyPointsChangeValue(int key, int value)
		{
			dynamicDifficultyPointsChangeValue[key] = value;
			Asse.Save(string.Format("dynamicDifficultyPointsChangeValue.{0}", key), value);
		}

		public virtual void SaveDynamicDifficultyPointsChangeValue()
		{
			Asse.Save("dynamicDifficultyPointsChangeValue", dynamicDifficultyPointsChangeValue);
		}

		public virtual void SetLvTimeData(string key, LvTime value)
		{
			lvTimeData[key] = value;
			Asse.Save("lvTimeData." + key, value);
		}

		public virtual void SaveLvTimeData()
		{
			Asse.Save("lvTimeData", lvTimeData);
		}

		public virtual void SetMonthIssueSumStar_(string key, int value)
		{
			MonthIssueSumStar[key] = value;
			Asse.Save("MonthIssueSumStar." + key, value);
		}

		public virtual void SaveMonthIssueSumStar_()
		{
			Asse.Save("MonthIssueSumStar", MonthIssueSumStar);
		}

		public virtual void SetMonthIssueLevel(string key, int value)
		{
			monthIssueLevel[key] = value;
			Asse.Save("monthIssueLevel." + key, value);
		}

		public virtual void SaveMonthIssueLevel()
		{
			Asse.Save("monthIssueLevel", monthIssueLevel);
		}

		public sealed override void InitData()
		{
			languageid = "";
			adFree = false;
			isPayingUsers = false;
			bMusic = true;
			bSound = true;
			bVibrate = true;
			surHintAdNum = 0;
			surFreezeAdNum = 0;
			bombModelProbability = 0;
			surReviveAdNum = 0;
			surBackAdNum = 0;
			surRefreshAdNum = 0;
			surFreeGoldNum = 0;
			sumStarLevel = 0;
			newlv = -1;
			lastlv = 0;
			curlv = 0;
			maxlv = 0;
			firstConsumeLv = 0;
			sumStar = 0;
			chestModelProbability = 100;
			adNumResetTime = string.Empty;
			curLvIsTimeoutFailed = false;
			lvRepeatDifficulty = 0;
			lvDifficulty = 0;
			currentLevelFirstFailedModelProportion = -1f;
			dynamicDifficultyPoints = -1;
			dynamicDifficultyPointsChangeValue = new Dictionary<int, int>();
			bShowBackGuide = false;
			dynamicDifficultyLevel = -1;
			totalConsume = 0f;
			lvTimeData = new Dictionary<string, LvTime>();
			surMagnetAdNum = 0;
			isUseMagnet = false;
			lvMedal = 1;
			MonthIssueSumStar = new Dictionary<string, int>();
			monthIssue = string.Empty;
			medal = 0;
			isFirstEnterMedal = true;
			isChangedMedal = false;
			isSkipPigAnim = false;
			monthIssueLevel = new Dictionary<string, int>();
			useGoldReviveNumberTimes = 0;
			isShowFestivalSkin = true;
			randomLevelWaveWeight = 0;
			showElementWholeGuideStatus = 0;
		}
	}
}
