using System;
using System.Collections.Generic;
using System.Linq;

namespace GameMail
{
	public class MailMgr
	{
		public static readonly MailMgr I = new MailMgr();

		public const string GetMailSuccessEventKey = "GameMail_Get_Success";

		public const string ReceiveMailCompleteEventKey = "GameMail_Receive_Complete";

		public const string DeleteMailCompleteEventKey = "GameMail_Delete_Complete";

		public EnergyMail EnergyMailMgr;

		public SystemMail SystemMailMgr;

		public void Init()
		{
			EnergyMailMgr = new EnergyMail();
			SystemMailMgr = new SystemMail();
			SystemMailMgr.AnalysisConfigure();
		}

		public void RequestMailsFormServer()
		{
			EnergyMailMgr.GetEnergy(delegate
			{
				DispatcherMgr.Emit("GameMail_Get_Success");
			});
			SingletonMono<EmailManager>.Instance.GetEmails();
		}

		public bool JudgeRed()
		{
			if (DB.Mail.EnergyMails.Values.Any(_JudgeNewMail))
			{
				return true;
			}
			if (DB.Mail.Mails.Values.Any(_JudgeNewMail))
			{
				return true;
			}
			return SingletonMono<EmailManager>.Instance.CacheEmails.Count > 0;
		}

		public bool JudgeNewSysTemMail()
		{
			return DB.Mail.Mails.Values.Any(_JudgeNewMail);
		}

		public List<MailData> GetMails()
		{
			List<MailData> list = DB.Mail.EnergyMails.Values.ToList();
			list.AddRange(DB.Mail.Mails.Values);
			list.AddRange(SingletonMono<EmailManager>.Instance.CacheEmails);
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (!_JudgeCanShowMail(list[i]))
				{
					list.RemoveAt(i);
				}
			}
			list.Sort(delegate(MailData a, MailData b)
			{
				bool flagA = a.Status != 0 && (a.Status != 1 || string.IsNullOrEmpty(a.Rewards.Trim()));
				bool flagB = b.Status != 0 && (b.Status != 1 || string.IsNullOrEmpty(b.Rewards.Trim()));
				if (flagA != flagB)
				{
					return flagA ? 1 : -1;
				}
				return a.GetStartTime().CompareTo(b.GetStartTime());
			});
			return list;
		}

		public void Delete(MailData mail)
		{
			if (mail == null || mail.Status == 3 || mail.Status == 4)
			{
				DispatcherMgr.Emit("GameMail_Delete_Complete", false);
				return;
			}
			bool hasReward = !string.IsNullOrEmpty(mail.Rewards.Trim());
			if ((hasReward && mail.Status != 2) || (!hasReward && mail.Status != 1))
			{
				DispatcherMgr.Emit("GameMail_Delete_Complete", false);
				return;
			}
			mail.Status = 3;
			DB.Mail.Save(mail.Id, mail);
			DispatcherMgr.Emit("GameMail_Delete_Complete", true);
		}

		public bool Read(MailData mail)
		{
			if (mail == null)
			{
				return false;
			}
			if (mail.Type == 2)
			{
				SingletonMono<EmailManager>.Instance.EmailTagRead(mail);
				return true;
			}
			if (mail.Status == 3 || mail.Status == 4)
			{
				return false;
			}
			if (mail.Status == 0)
			{
				mail.Status = 1;
				DB.Mail.Save(mail.Id, mail);
			}
			return true;
		}

		public void Receive(List<MailData> mails, bool isSingleReceive)
		{
			if (mails == null)
			{
				_SendReceiveCompleteEvent(isSingleReceive, false);
				return;
			}
			if (mails[0].Type == 2)
			{
				SingletonMono<EmailManager>.Instance.EmailClaimReward(mails[0], delegate(bool succ)
				{
					_SendReceiveCompleteEvent(isSingleReceive, succ);
				});
				return;
			}
			List<MailData> energyMails = mails.Where(w => w.Type == 1).ToList();
			List<MailData> systemMails = mails.Where(w => w.Type == 0).ToList();
			if (energyMails.Count > 0)
			{
				EnergyMailMgr.ReceiveEnergy(energyMails, delegate(bool isSuccess)
				{
					if (systemMails.Count > 0)
					{
						_ReceiveSystemMails(isSingleReceive, systemMails);
					}
					else
					{
						_SendReceiveCompleteEvent(isSingleReceive, isSuccess);
					}
				});
			}
			else
			{
				_ReceiveSystemMails(isSingleReceive, systemMails);
			}
		}

		private void _ReceiveSystemMails(bool isSingleReceive, List<MailData> systemMails)
		{
			SystemMailMgr.Receive(systemMails);
			_SendReceiveCompleteEvent(isSingleReceive, true);
		}

		private void _SendReceiveCompleteEvent(bool isSingleReceive, bool isSuccess)
		{
			DispatcherMgr.Emit("GameMail_Receive_Complete", isSingleReceive, isSuccess);
		}

		private bool _JudgeInvalid(MailData mail)
		{
			if (mail.Type == 1)
			{
				return false;
			}
			DateTime now = DateTime.Now;
			if (now < mail.GetStartTime())
			{
				return true;
			}
			if (now <= mail.GetEndTime())
			{
				return false;
			}
			mail.Status = 4;
			DB.Mail.Save(mail.Id, mail);
			return true;
		}

		private bool _JudgeNewMail(MailData mail)
		{
			if (mail == null || mail.Status != 0)
			{
				return false;
			}
			return !_JudgeInvalid(mail);
		}

		private bool _JudgeCanShowMail(MailData mail)
		{
			if (mail != null && (mail.Status < 2 || mail.Status > 4))
			{
				if (mail.Status == 1 && string.IsNullOrEmpty(mail.Rewards.Trim()))
				{
					return false;
				}
				return !_JudgeInvalid(mail);
			}
			return false;
		}
	}
}
