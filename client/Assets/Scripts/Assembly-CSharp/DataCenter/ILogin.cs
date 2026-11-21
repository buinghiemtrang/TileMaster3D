using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ILogin : AbsDataModel
	{
		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected bool isNotRemindAgainLogin;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string lastRefreshTime;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected int showNumber;

		[DBValue(DBStoreType.None, DBPutType.Local)]
		[JsonProperty]
		protected bool isCostInGameScene;

		public ILogin()
		{
			InitData();
		}

		public virtual bool IsNotRemindAgainLogin
		{
			get
			{
				return isNotRemindAgainLogin;
			}
			set
			{
				if (isNotRemindAgainLogin != value)
				{
					isNotRemindAgainLogin = value;
					Asse.Save("isNotRemindAgainLogin", value);
				}
			}
		}

		public virtual string LastRefreshTime
		{
			get
			{
				return lastRefreshTime;
			}
			set
			{
				if (string.Equals(lastRefreshTime, value))
				{
					return;
				}
				lastRefreshTime = value;
				Asse.Save("lastRefreshTime", value);
			}
		}

		public virtual int ShowNumber
		{
			get
			{
				return showNumber;
			}
			set
			{
				if (showNumber != value)
				{
					showNumber = value;
					Asse.Save("showNumber", value);
				}
			}
		}

		public virtual bool IsCostInGameScene
		{
			get
			{
				return isCostInGameScene;
			}
			set
			{
				isCostInGameScene = value;
			}
		}

		public sealed override void InitData()
		{
			lastRefreshTime = string.Empty;
			isNotRemindAgainLogin = false;
			showNumber = 0;
			isCostInGameScene = false;
		}
	}
}
