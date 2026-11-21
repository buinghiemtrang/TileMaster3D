using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IAccount : AbsDataModel
	{
		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string name;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string email;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string photoUrl;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string providerid;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string uid;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string userid;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected string deviceid;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string token;

		[DBValue(DBStoreType.Permanent, DBPutType.Local)]
		[JsonProperty]
		protected string loginPlatform;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected string defaultName;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected bool isCurrentVersionNewUser;

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected string newUserVersion;

		public IAccount()
		{
			InitData();
		}

		public virtual string Name
		{
			get
			{
				return name;
			}
			set
			{
				if (string.Equals(name, value))
				{
					return;
				}
				name = value;
				Asse.Save("name", value);
			}
		}

		public virtual string Email
		{
			get
			{
				return email;
			}
			set
			{
				if (string.Equals(email, value))
				{
					return;
				}
				email = value;
				Asse.Save("email", value);
			}
		}

		public virtual string PhotoUrl
		{
			get
			{
				return photoUrl;
			}
			set
			{
				if (string.Equals(photoUrl, value))
				{
					return;
				}
				photoUrl = value;
				Asse.Save("photoUrl", value);
			}
		}

		public virtual string Providerid
		{
			get
			{
				return providerid;
			}
			set
			{
				if (string.Equals(providerid, value))
				{
					return;
				}
				providerid = value;
				Asse.Save("providerid", value);
			}
		}

		public virtual string Uid
		{
			get
			{
				return uid;
			}
			set
			{
				if (string.Equals(uid, value))
				{
					return;
				}
				uid = value;
				Asse.Save("uid", value);
			}
		}

		public virtual string Userid
		{
			get
			{
				return userid;
			}
			set
			{
				if (string.Equals(userid, value))
				{
					return;
				}
				userid = value;
				Asse.Save("userid", value);
			}
		}

		public virtual string Deviceid
		{
			get
			{
				return deviceid;
			}
			set
			{
				if (string.Equals(deviceid, value))
				{
					return;
				}
				deviceid = value;
				Asse.Save("deviceid", value);
			}
		}

		public virtual string Token
		{
			get
			{
				return token;
			}
			set
			{
				if (string.Equals(token, value))
				{
					return;
				}
				token = value;
				Asse.Save("token", value);
			}
		}

		public virtual string LoginPlatform
		{
			get
			{
				return loginPlatform;
			}
			set
			{
				if (string.Equals(loginPlatform, value))
				{
					return;
				}
				loginPlatform = value;
				Asse.Save("loginPlatform", value);
			}
		}

		public virtual string DefaultName
		{
			get
			{
				return defaultName;
			}
			set
			{
				if (string.Equals(defaultName, value))
				{
					return;
				}
				defaultName = value;
				Asse.Save("defaultName", value);
			}
		}

		public virtual bool IsCurrentVersionNewUser
		{
			get
			{
				return isCurrentVersionNewUser;
			}
			set
			{
				if (isCurrentVersionNewUser != value)
				{
					isCurrentVersionNewUser = value;
					Asse.Save("isCurrentVersionNewUser", value);
				}
			}
		}

		public virtual string NewUserVersion
		{
			get
			{
				return newUserVersion;
			}
			set
			{
				if (string.Equals(newUserVersion, value))
				{
					return;
				}
				newUserVersion = value;
				Asse.Save("newUserVersion", value);
			}
		}

		public sealed override void InitData()
		{
			name = "";
			email = "";
			photoUrl = "";
			providerid = "";
			uid = "";
			userid = "";
			deviceid = "";
			token = "";
			loginPlatform = "";
			defaultName = "";
			newUserVersion = "";
			isCurrentVersionNewUser = false;
		}
	}
}
