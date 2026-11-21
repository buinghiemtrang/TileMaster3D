using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameMail
{
	public class SingleMailDialog : DialogBase
	{
		[SerializeField]
		private Text contentTex;

		[SerializeField]
		private OrdinaryRewardItem[] items;

		[SerializeField]
		private GameObject itemsObj;

		[SerializeField]
		private MButton closeBtn;

		[SerializeField]
		private MButton receiveBtn;

		[SerializeField]
		private MButton deleteBtn;

		public Action<int> CloseCallback;

		private MailData _mail;

		private int[,] _mailRewards;

		protected override void Awake()
		{
			base.Awake();
			closeBtn.onClick.AddListener(_ClickClose);
			receiveBtn.onClick.AddListener(_ClickReceive);
			deleteBtn.onClick.AddListener(_ClickDelete);
			DispatcherMgr.On<bool, bool>("GameMail_Receive_Complete", _ReceiveMailComplete);
			DispatcherMgr.On<bool>("GameMail_Delete_Complete", _DeleteMailComplete);
		}

		public void Show(MailData mail)
		{
			_mail = mail;
			Language.LanguageData languageDataByKey = Language.LanguageManager.Instance.GetLanguageDataByKey(mail.Content);
			contentTex.text = languageDataByKey != null ? languageDataByKey.value : mail.DefaultContent;
			bool flag;
			if (string.IsNullOrEmpty(mail.Rewards.Trim()))
			{
				flag = true;
			}
			else
			{
				flag = _mail.Status == 2;
			}
			receiveBtn.gameObject.SetActive(!flag);
			deleteBtn.gameObject.SetActive(flag);
			_mailRewards = null;
			bool active = !string.IsNullOrEmpty(mail.Rewards.Trim());
			itemsObj.SetActive(active);
			if (itemsObj.activeSelf)
			{
				_mailRewards = mail.Rewards.ToInt(2, ',', '|');
				int length = _mailRewards.GetLength(0);
				for (int i = 0; i < items.Length; i++)
				{
					items[i].gameObject.SetActive(i < length);
					if (i < length)
					{
						items[i].Init((PropID)_mailRewards[i, 0], _mailRewards[i, 1]);
					}
				}
			}
			_Open();
			LogEvent.I.Track("Mail_Click").Add("MailType", ((MailType)mail.Type).ToString()).CommonAttr(true).Send();
		}

		private void _ClickClose(MButton btn)
		{
			_Close(delegate
			{
				CloseCallback.InvokeSafe(1);
			});
		}

		private void _ClickReceive(MButton btn)
		{
			LogEvent.I.Track("Mail_Receive").Add("Type", "One").CommonAttr(true).Send();
			if (_mail.Type == 1 && MailMgr.I.EnergyMailMgr.GetCanReceiveEnergyMaxNum() <= 0)
			{
				Global.CreateMsgBoxDialog("Language238", null, false, "");
				return;
			}
			SceneMgr.OpenWaiting(-1f, 1f);
			List<MailData> list = new List<MailData> { _mail };
			MailMgr.I.Receive(list, true);
		}

		private void _ClickDelete(MButton btn)
		{
			MailMgr.I.Delete(_mail);
		}

		private void _DeleteMailComplete(bool isSuccess)
		{
			_Close(delegate
			{
				CloseCallback.InvokeSafe(3);
			});
		}

		private void _ReceiveMailComplete(bool isSingleReceive, bool isSuccess)
		{
			if (!isSingleReceive)
			{
				return;
			}
			SceneMgr.CloseWaiting();
			if (isSuccess)
			{
				if (_mailRewards != null)
				{
					RewardUnifyDialog rewardUnifyDialog = Global.CreateReward(_mailRewards, RewardType.None, REWARD_PLACE.None, "Mail", false, false);
					rewardUnifyDialog.SetComplete(delegate
					{
						_Close(delegate
						{
							CloseCallback.InvokeSafe(2);
						});
					});
				}
			}
			else
			{
				Global.CreateMsgBoxDialog("Language55", null, false, "");
			}
		}

		protected override void OnDestroy()
		{
			DispatcherMgr.Off<bool, bool>("GameMail_Receive_Complete", _ReceiveMailComplete);
			DispatcherMgr.Off<bool>("GameMail_Delete_Complete", _DeleteMailComplete);
			base.OnDestroy();
		}
	}
}
