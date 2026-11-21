using System;
using UnityEngine;
using UnityEngine.UI;

public class StarRankingActivityDialog : DialogBase
{
	[SerializeField]
	private StarRankingActivityScroll scroll;

	[SerializeField]
	private Text timeTex;

	[SerializeField]
	private StarRankingActivityItem ownItem;

	[SerializeField]
	private MButton closeBtn;

	[SerializeField]
	private MButton helpBtn;

	[SerializeField]
	private MButton bgBtn;

	public Action<bool> closeCallback;

	private StarRankingActivityHelpDialog helpDialog;

	protected override void Awake()
	{
		base.Awake();
		scroll.showOwnCallback = delegate(StarRankingData own)
		{
			if (own != null)
			{
				ownItem.Init(own, false, false);
			}
		};
		bgBtn.onClick.AddListener(CloseBtn);
		closeBtn.onClick.AddListener(CloseBtn);
		helpBtn.onClick.AddListener(_HelpBtn);
		DispatcherMgr.On<int>("StarRankingActivity_TimeChange", _RefreshTime);
	}

	public void Show(int surTime)
	{
		DB.StarRankingActivity.IsAutoEjected = true;
		_RefreshTime(surTime);
		_Open();
		LogEvent.I.Track("Activity_Ranking_Open").Add("MyStep", DB.StarRankingActivity.Level + 1).CommonAttr(true)
			.Send();
	}

	private void _RefreshTime(int surTime)
	{
		timeTex.text = Util.Helper.FormatTimer(surTime, "hh:mm:ss");
	}

	public void CloseBtn(MButton btn)
	{
		if (!closeBtn.interactable)
		{
			return;
		}
		closeBtn.interactable = false;
		if ((bool)helpDialog)
		{
			helpDialog.Close();
		}
		bool isForce = btn == null;
		_Close(delegate
		{
			closeCallback?.Invoke(isForce);
		});
	}

	private void _HelpBtn(MButton btn)
	{
		helpDialog = Singleton<Festival.FestivalMgr>.I.AddTopNode<StarRankingActivityHelpDialog>("Prefabs/Activity/StarRanking/{0}_StarRankingActivityHelpDialog");
		helpDialog.closeCallback = delegate
		{
			helpDialog = null;
		};
		helpDialog.Show();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DispatcherMgr.Off<int>("StarRankingActivity_TimeChange", _RefreshTime);
	}
}
