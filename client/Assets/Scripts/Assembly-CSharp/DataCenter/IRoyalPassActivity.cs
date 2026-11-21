using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IRoyalPassActivity : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string issue;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string startTime;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string endTime;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int activityState;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<int> rewardsState;

		public DBList<int> RewardsState = new DBList<int>();

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<string, int> scores;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int curGrade;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected int curExp;

		public IRoyalPassActivity()
		{
			RewardsState.Init(() => rewardsState, delegate(List<int> value)
			{
				rewardsState = value;
			}, delegate
			{
				Asse.Save("rewardsState", rewardsState);
			});
			InitData();
		}

		public virtual string Issue
		{
			get
			{
				return issue;
			}
			set
			{
				if (string.Equals(issue, value))
				{
					return;
				}
				issue = value;
				Asse.Save("issue", value);
			}
		}

		public virtual string StartTime
		{
			get
			{
				return startTime;
			}
			set
			{
				if (string.Equals(startTime, value))
				{
					return;
				}
				startTime = value;
				Asse.Save("startTime", value);
			}
		}

		public virtual string EndTime
		{
			get
			{
				return endTime;
			}
			set
			{
				if (string.Equals(endTime, value))
				{
					return;
				}
				endTime = value;
				Asse.Save("endTime", value);
			}
		}

		public virtual int ActivityState
		{
			get
			{
				return activityState;
			}
			set
			{
				if (activityState != value)
				{
					activityState = value;
					Asse.Save("activityState", value);
				}
			}
		}

		public virtual Dictionary<string, int> Scores
		{
			get
			{
				return scores;
			}
			set
			{
				scores = value;
				Asse.Save("scores", value);
			}
		}

		public virtual int CurGrade
		{
			get
			{
				return curGrade;
			}
			set
			{
				if (curGrade != value)
				{
					curGrade = value;
					Asse.Save("curGrade", value);
				}
			}
		}

		public virtual int CurExp
		{
			get
			{
				return curExp;
			}
			set
			{
				if (curExp != value)
				{
					curExp = value;
					Asse.Save("curExp", value);
				}
			}
		}

		public virtual void SetScores(string key, int value)
		{
			scores[key] = value;
			Asse.Save("scores." + key, value);
		}

		public virtual void SaveScores()
		{
			Asse.Save("scores", scores);
		}

		public sealed override void InitData()
		{
			issue = string.Empty;
			startTime = string.Empty;
			endTime = string.Empty;
			activityState = 0;
			rewardsState = new List<int>();
			scores = new Dictionary<string, int>();
			curGrade = 0;
			curExp = 0;
		}
	}
}
