using System.Collections.Generic;
using GameMail;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class IMail : AbsDataModel
	{
		[DBValue(DBStoreType.None, DBPutType.Remote)]
		[JsonProperty]
		protected Dictionary<string, MailData> mails;

		[DBValue(DBStoreType.Save, DBPutType.Remote)]
		[JsonProperty]
		protected List<string> mailIds;

		public DBList<string> MailIds = new DBList<string>();

		[DBValue(DBStoreType.Save, DBPutType.Local)]
		[JsonProperty]
		protected Dictionary<string, MailData> energyMails;

		public IMail()
		{
			MailIds.Init(() => mailIds, delegate(List<string> value)
			{
				mailIds = value;
			}, delegate
			{
				Asse.Save("mailIds", mailIds);
			});
			InitData();
		}

		public virtual Dictionary<string, MailData> Mails
		{
			get
			{
				return mails;
			}
			set
			{
				mails = value;
			}
		}

		public virtual Dictionary<string, MailData> EnergyMails
		{
			get
			{
				return energyMails;
			}
			set
			{
				energyMails = value;
				Asse.Save("energyMails", value);
			}
		}

		public virtual void SetEnergyMails(string key, MailData value)
		{
			energyMails[key] = value;
			Asse.Save("energyMails." + key, value);
		}

		public virtual void SaveEnergyMails()
		{
			Asse.Save("energyMails", energyMails);
		}

		public sealed override void InitData()
		{
			mails = new Dictionary<string, MailData>();
			mailIds = new List<string>();
			energyMails = new Dictionary<string, MailData>();
		}
	}
}
