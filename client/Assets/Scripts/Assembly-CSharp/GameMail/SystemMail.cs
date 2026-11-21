using System;

namespace GameMail
{
	public class SystemMail : MailBase
	{
		public void AnalysisConfigure()
		{
			if (DataCfg.cfgMails == null)
			{
				return;
			}
			for (int i = 0; i < DataCfg.cfgMails.Count; i++)
			{
				cfgMail cfg = DataCfg.cfgMails[i];
				if (!DB.Mail.Mails.ContainsKey(cfg.id.ToString()))
				{
					MailData mailData = new MailData(cfg);
					GetTime(cfg.time, out DateTime startTime, out DateTime endTime);
					mailData.StartTime = startTime.ToString();
					mailData.EndTime = endTime.ToString();
					DB.Mail.Save(mailData.Id, mailData);
				}
			}
		}

		private void GetTime(string time, out DateTime startTime, out DateTime endTime)
		{
			startTime = DateTime.Now;
			endTime = DateTime.Now;
			int[,] array = time.ToInt(6, '|', ',');
			for (int i = 0; i < array.GetLength(0); i++)
			{
				DateTime dateTime = new DateTime(array[i, 0], array[i, 1], array[i, 2], array[i, 3], array[i, 4], array[i, 5]);
				if (i == 0)
				{
					startTime = dateTime;
				}
				else if (i == 1)
				{
					endTime = dateTime;
				}
			}
		}
	}
}
