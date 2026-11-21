using System.Collections.Generic;
using GameMail;

public class MailBase
{
	public virtual void Receive(List<MailData> mails)
	{
		for (int i = 0; i < mails.Count; i++)
		{
			MailData mailData = mails[i];
			if (mailData != null && (uint)(mailData.Status - 2) > 2u)
			{
				mailData.Status = 2;
				DB.Mail.Save(mailData.Id, mailData);
			}
		}
	}
}
