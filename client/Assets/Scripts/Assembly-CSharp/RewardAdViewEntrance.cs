using DG.Tweening;
using FunctionEject;
using RewardAdView;
using UnityEngine;
using UnityEngine.UI;

public class RewardAdViewEntrance : EjectItem
{
	[SerializeField]
	private GameObject _root;

	[SerializeField]
	private MButton _entranceButton;

	[SerializeField]
	private Text _timeTex;

	private void Awake()
	{
		_entranceButton.onClick.AddListener(OnClickEntrance);
	}

	private void OnDestroy()
	{
		_timeTex.DOKill(false);
	}

	public override void Init()
	{
		RewardAdViewManager.I.CheckSameDay();
		_root.SetActive(RewardAdViewManager.I.IsOpen);
		if (!_root.activeSelf)
		{
			return;
		}
		int leftTime = RewardAdViewManager.I.GetLeftTime();
		if (leftTime < 1)
		{
			_timeTex.gameObject.SetActive(false);
		}
		else
		{
			_timeTex.gameObject.SetActive(true);
			ShowCountdown(leftTime);
		}
	}

	public override bool Eject()
	{
		if (RewardAdViewManager.I.HasPop())
		{
			return false;
		}
		if (!RewardAdViewManager.I.IsOpen)
		{
			return false;
		}
		DB.RewardAdView.Data.HasDailyShowPop = true;
		DB.RewardAdView.Save();
		RewardAdViewManager.I.PopDialog(null);
		return true;
	}

	private void OnClickEntrance(MButton button)
	{
		RewardAdViewDialog.Create(null);
	}

	private void ShowCountdown(int seconds)
	{
		_timeTex.DOKill(false);
		if (seconds > 0)
		{
			_timeTex.DOCounter(seconds, 0, seconds, Util.Helper.FormatTimer).OnComplete(delegate
			{
				_root.SetActive(false);
			});
		}
		else
		{
			_timeTex.text = "00:00:00";
		}
	}
}
