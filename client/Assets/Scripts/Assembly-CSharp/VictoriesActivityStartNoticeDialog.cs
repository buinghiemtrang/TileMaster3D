using System;
using UnityEngine;
using UnityEngine.UI;

public class VictoriesActivityStartNoticeDialog : DialogBase
{
	[SerializeField]
	private Text timeTex;

	[SerializeField]
	private MButton closeBtn;

	public Action closedCallback;

	public VictoriesActivityStartNoticeDialog()
	{
		fMaskAlpha = 0.8f;
	}

	protected override void Awake()
	{
		base.Awake();
		closeBtn.onClick.AddListener(ClickClose);
		DispatcherMgr.On<int>("VictoriesActivity_TimeChange", _RefreshTime);
	}

	public void Show(string openType, int surTime)
	{
		DB.VictoriesActivity.IsAutoEjectedNoticeDialog = true;
		int[,] array = cf_const.victoriesActivityDate.ToInt(2, '|', ',');
		if ((array.GetLength(0) & 1) != 0)
		{
			Debug.LogError("victors activity date error. " + cf_const.victoriesActivityDate);
		}
		timeTex.text = string.Format("{0}:00-{1}:00", array[0, 1], array[1, 1]);
		_Open();
		LogEvent.I.HomeFrameOpen("WinningStreakNotice", openType);
	}

	public void ClickClose(MButton btn)
	{
		if (closeBtn.interactable)
		{
			closeBtn.interactable = false;
			_Close(closedCallback);
		}
	}

	private void _RefreshTime(int surTime)
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DispatcherMgr.Off<int>("VictoriesActivity_TimeChange", _RefreshTime);
	}
}
