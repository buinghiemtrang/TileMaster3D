using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util;

namespace GameMail
{
	public class MailsDialog : DialogBase
	{
		[SerializeField]
		private MailScroll scroll;

		[SerializeField]
		private MButton closeBtn;

		[SerializeField]
		private MButton oneKeyReceiveBtn;

		[SerializeField]
		private GameObject noMailDescObj;

		[SerializeField]
		private GameObject noMailBgObj;

		public const string CloseDialogEventKey = "MailsDialog_Close";

		private List<MailData> _mails;

		private int[,] _mailRewards;

		protected override void Awake()
		{
			base.Awake();
			closeBtn.onClick.AddListener(_ClickClose);
			oneKeyReceiveBtn.onClick.AddListener(_ClickOneKeyReceive);
			DispatcherMgr.On<bool, bool>("GameMail_Receive_Complete", _ReceiveMailComplete);
			DispatcherMgr.On("GameMail_Get_Success", _RefreshUI);
			DispatcherMgr.On<bool>("GameMail_Delete_Complete", _DeleteMailComplete);
		}

		public void Show(string openType)
		{
			_RefreshUI();
			_Open();
			LogEvent.I.Track("Mail_Open").Add("OpenType", openType).CommonAttr(true).Send();
		}

		private void _ClickClose(MButton btn)
		{
			_Close(delegate
			{
				DispatcherMgr.Emit("MailsDialog_Close");
			});
		}

		private void _ClickOneKeyReceive(MButton btn)
		{
			LogEvent.I.Track("Mail_Receive").Add("Type", "All").CommonAttr(true).Send();
			_mailRewards = new int[0, 0];
			int canReceiveEnergyMaxNum = MailMgr.I.EnergyMailMgr.GetCanReceiveEnergyMaxNum();
			List<MailData> list = new List<MailData>();
			List<MailData> list2 = _mails.Where(x => x.Type == 0).ToList();
			for (int i = 0; i < list2.Count; i++)
			{
				MailData mailData = _mails[i];
				if (_JudgeCanReceive(mailData))
				{
					int[,] array = mailData.Rewards.ToInt(2, ',', '|');
					_mailRewards = Helper.MergerReward(_mailRewards, array);
					if (canReceiveEnergyMaxNum > 0 && array != null && array.GetLength(0) > 0)
					{
						int num = array[i, 0];
						if (num == 2)
						{
							canReceiveEnergyMaxNum -= array[i, 1];
						}
						else if (num == 21)
						{
							canReceiveEnergyMaxNum = 0;
						}
					}
					list.Add(mailData);
				}
			}
			List<MailData> list3 = new List<MailData>();
			List<MailData> list4 = _mails.Where(x => x.Type == 1).ToList();
			for (int j = 0; j < list4.Count; j++)
			{
				MailData mailData2 = _mails[j];
				if (_JudgeCanReceive(mailData2))
				{
					if (canReceiveEnergyMaxNum < 1)
					{
						list3.Add(mailData2);
					}
					else
					{
						int[,] array2 = mailData2.Rewards.ToInt(2, ',', '|');
						_mailRewards = Helper.MergerReward(_mailRewards, array2);
						list.Add(mailData2);
						canReceiveEnergyMaxNum--;
					}
				}
			}
			if (list.Count < 1)
			{
				if (list3.Count > 0)
				{
					Global.CreateMsgBoxDialog("Language238", null, false, "");
				}
			}
			else
			{
				SceneMgr.OpenWaiting(15f, 1f);
				MailMgr.I.Receive(list, false);
			}
		}

		private bool _JudgeCanReceive(MailData mail)
		{
			return mail.Status != 2 && !string.IsNullOrEmpty(mail.Rewards.Trim());
		}

		private void _ReceiveMailComplete(bool isSingleReceive, bool isSuccess)
		{
			if (isSingleReceive)
			{
				return;
			}
			SceneMgr.CloseWaiting();
			if (isSuccess)
			{
				_RefreshUI();
				Global.CreateReward(_mailRewards, RewardType.None, REWARD_PLACE.None, "Mail", false, false);
			}
			else
			{
				Global.CreateMsgBoxDialog("Language55", null, false, "");
			}
		}

		private void _DeleteMailComplete(bool isSuccess)
		{
			if (isSuccess)
			{
				_RefreshUI();
			}
		}

		private void _RefreshUI()
		{
			_mails = MailMgr.I.GetMails();
			bool flag = _mails.Count == 0;
			noMailDescObj.SetActive(flag);
			noMailBgObj.SetActive(flag);
			bool active = false;
			if (_mails.Count >= 1)
			{
				for (int i = 0; i < _mails.Count; i++)
				{
					MailData mailData = _mails[i];
					if (mailData.Status != 2 && !string.IsNullOrEmpty(mailData.Rewards.Trim()))
					{
						if (mailData.Type == 2)
						{
							active = false;
							break;
						}
						active = true;
					}
				}
			}
			oneKeyReceiveBtn.gameObject.SetActive(active);
			scroll.RefreshItems(_mails);
		}

		protected override void OnDestroy()
		{
			DispatcherMgr.Off<bool, bool>("GameMail_Receive_Complete", _ReceiveMailComplete);
			DispatcherMgr.Off("GameMail_Get_Success", _RefreshUI);
			DispatcherMgr.Off<bool>("GameMail_Delete_Complete", _DeleteMailComplete);
			base.OnDestroy();
		}
	}
}
