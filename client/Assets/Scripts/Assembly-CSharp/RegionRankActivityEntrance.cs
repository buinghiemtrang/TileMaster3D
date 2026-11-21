using DG.Tweening;
using FunctionEject;
using UnityEngine;
using UnityEngine.UI;

public class RegionRankActivityEntrance : EjectItem
{
	[SerializeField]
	private MButton button;

	[SerializeField]
	private Text timeDownTxt;

	[SerializeField]
	private Text rankingTxt;

	private void Start()
	{
		button.onClick.AddListener(OpenClick);
		DispatcherMgr.On("Update_Account", Init);
	}

	public override void Init()
	{
		int num = 0;
		ActivityStateBase activityStateBase = GSingleton<RegionRankActivityManager>.ins.GetActivityStateWithRefresh();
		base.gameObject.SetActive(activityStateBase != ActivityStateBase.NoStart);
		num = Singleton<RegionRankServerManager>.I.GetCachePersonalRanking();
		if (num < 0)
		{
			rankingTxt.text = "";
		}
		else if (num == 0)
		{
			rankingTxt.text = "1000+";
		}
		else
		{
			rankingTxt.text = num.ToString();
		}
		if (activityStateBase == ActivityStateBase.During)
		{
			_Countdown();
			return;
		}
		string text = (GSingleton<RegionRankActivityManager>.ins.IsPreview() ? "TM3DLanguage21" : "TM3DLanguage22");
		timeDownTxt.SetLBaseKey((GSingleton<RegionRankActivityManager>.ins.GetActivityState() == ActivityStateBase.Ending) ? "TM3DLanguage23" : text);
	}

	public override bool Eject()
	{
		if (!base.gameObject.activeSelf)
		{
			return false;
		}
		return GSingleton<RegionRankActivityManager>.ins.TryPop(false, delegate(bool isSuccessPop)
		{
			Init();
			if (isSuccessPop)
			{
				End(EjectType.Continue);
			}
		});
	}

	private void _Countdown()
	{
		if (base.gameObject.activeSelf)
		{
			timeDownTxt.DOKill();
			int activityEndSeconds = GSingleton<RegionRankActivityManager>.ins.GetActivityEndSeconds();
			if (activityEndSeconds != 0)
			{
				timeDownTxt.DOCounter(activityEndSeconds, 0, activityEndSeconds, delegate(int value)
				{
					timeDownTxt.text = Util.Helper.FormatTimerD(value);
				}).OnComplete(Init);
			}
		}
	}

	private void OpenClick(MButton _)
	{
		GSingleton<RegionRankActivityManager>.ins.TryPop(true, delegate
		{
			Init();
		});
	}

	private void OnDestroy()
	{
		timeDownTxt.DOKill();
		DispatcherMgr.Off("Update_Account", Init);
	}
}
