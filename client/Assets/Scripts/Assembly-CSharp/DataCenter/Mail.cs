using System.Collections.Generic;
using GameMail;
using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Mail : IMail
	{
		public MailData GetMail(string key)
		{
			MailData value = null;
			if (!mails.TryGetValue(key, out value))
			{
				value = null;
			}
			return value;
		}

		public void Save(string key, MailData data)
		{
			if (data.Type == 1)
			{
				SetEnergyMails(key, data);
			}
			else if (data.Type == 0)
			{
				if (!MailIds.Contains(key))
				{
					MailIds.Add(key);
				}
				mails[key] = data;
				Asse.SaveForced("mails." + key, data);
			}
		}

		public override void ReadLocalData()
		{
			foreach (string mailId in mailIds)
			{
				Dictionary<string, MailData> dictionary = mails;
				dictionary[mailId] = StorageMgr.GetObject<MailData>("Mail.mails." + mailId, "{}");
			}
		}

		public override void DelLocalData()
		{
			foreach (string key in mails.Keys)
			{
				StorageMgr.DelKey("Mail.mails." + key);
			}
		}

		public override void WriteLocalData()
		{
			foreach (string key in mails.Keys)
			{
				StorageMgr.SetObject("Mail.mails." + key, mails[key]);
			}
		}
	}
}
