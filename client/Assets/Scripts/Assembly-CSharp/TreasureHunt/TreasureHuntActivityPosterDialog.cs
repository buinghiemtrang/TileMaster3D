using System;
using Festival;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace TreasureHunt
{
	public class TreasureHuntActivityPosterDialog : DialogBase
	{
		[SerializeField]
		private Text timeTex;

		[SerializeField]
		private Text playValueTex;

		[SerializeField]
		private MButton closeBtn;

		[SerializeField]
		private MButton helpBtn;

		[SerializeField]
		private MButton playBtn;

		public Action<int> closeCallback;

		private DialogShow helpDialog;

		private bool isJoined;

		protected override void Awake()
		{
			base.Awake();
			closeBtn.onClick.AddListener(_ClickClose);
			helpBtn.onClick.AddListener(_ClickHelp);
			playBtn.onClick.AddListener(_ClickPlay);
			DispatcherMgr.On<int>("TreasureHuntActivity_TimeChange", _RefreshTime);
			DispatcherMgr.On("TreasureHuntHttp_GetActivity_Succ", _RefreshUI);
		}

		public void Show(string openType, int surTime)
		{
			// UNVERIFIED: machine code calls TreasureHuntActivityMgr._RemoveState(PosterDialog) (private); ShowDialog is a one-instruction branch to it
			Singleton<TreasureHuntActivityMgr>.I.ShowDialog(ActivityState.PosterDialog);
			_RefreshTime(surTime);
			_RefreshUI();
			_Open();
			LogEvent.I.Track("Team_Treasure_Open").Add("ActivityTime", DB.TreasureHuntActivity.Issue).CommonAttr(true)
				.Send(false);
			LogEvent.I.Track("Home_Frame_Open").Add("Name", "TreasureHuntIntroduce").Add("OpenType", openType)
				.CommonAttr(true)
				.Send(false);
		}

		private void _ClickClose(MButton btn)
		{
			btn.interactable = false;
			if (helpDialog != null)
			{
				helpDialog.ClickClose();
			}
			_Close(delegate
			{
				if (closeCallback != null)
				{
					closeCallback(1);
				}
			});
		}

		private void _ClickHelp(MButton btn)
		{
			helpDialog = Singleton<FestivalMgr>.I.AddTopNode<DialogShow>("Prefabs/Activity/TreasureHunt/{0}/TreasureHuntActivityHelpDialog");
			helpDialog.func_callback_close = delegate
			{
				helpDialog = null;
			};
		}

		private void _ClickPlay(MButton btn)
		{
			btn.interactable = false;
			Action action;
			if (!isJoined)
			{
				SceneMainBottom.instance.ClickBtnTeam(null);
				action = delegate
				{
					if (closeCallback != null)
					{
						closeCallback(3);
					}
				};
			}
			else
			{
				Action action2 = delegate
				{
					BlackCutDialog.EnterGame();
				};
				if (DB.Bag.JudgeHasEnergy())
				{
					action2();
				}
				else
				{
					EnergyDialog.Create(action2, delegate
					{
						SceneMainBottom.instance.btnShop.ClickSelf();
					});
				}
				action = delegate
				{
					if (closeCallback != null)
					{
						closeCallback(2);
					}
				};
			}
			_Close(action);
		}

		private void _RefreshUI()
		{
			isJoined = Singleton<TreasureHuntActivityMgr>.I.JudgeHasState(ActivityState.JoinedTeam);
			playValueTex.SetLKey(isJoined ? "Language312" : "Language163");
		}

		private void _RefreshTime(int surTime)
		{
			timeTex.text = Helper.FormatTimer(surTime, "hh:mm:ss");
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			DispatcherMgr.Off<int>("TreasureHuntActivity_TimeChange", _RefreshTime);
			DispatcherMgr.Off("TreasureHuntHttp_GetActivity_Succ", _RefreshUI);
		}
	}
}
