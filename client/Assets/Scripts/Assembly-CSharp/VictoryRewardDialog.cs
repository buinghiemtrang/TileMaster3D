using System;
using UnityEngine;
using UnityEngine.UI;
using _FrameWork;

public class VictoryRewardDialog : DialogBase
{
	[SerializeField]
	private MButton _trueButton;

	[SerializeField]
	private Text _valueTex;

	[SerializeField]
	private ParticleSystemInfo _particle;

	private int _goldCount;

	private Action _onCompleted;

	private int _lv;

	public VictoryRewardDialog()
	{
		fMaskAlpha = 0.8f;
	}

	protected override void Awake()
	{
		base.Awake();
		_trueButton.onClick.AddListener(ClickTrue);
	}

	private void Start()
	{
		Refresh();
	}

	private void Refresh()
	{
		int minInclusive = 2;
		int maxExclusive = 6;
		if (cf_const.VictoryRewardRewardGold.Length >= 2)
		{
			minInclusive = cf_const.VictoryRewardRewardGold[0];
			maxExclusive = cf_const.VictoryRewardRewardGold[1] + 1;
		}
		_goldCount = UnityEngine.Random.Range(minInclusive, maxExclusive);
		_valueTex.text = string.Format("x{0}", _goldCount);
		_Open(delegate
		{
			_particle.PlayOnce();
		});
	}

	private void ClickTrue(MButton button)
	{
		Global.ShowNextInterstitial(delegate
		{
			_trueButton.gameObject.SetActive(false);
			DB.Bag.Modify(PropID.Money, _goldCount, true, "AdVictory");
			_Close(_onCompleted.InvokeSafe);
		}, _lv, INTER_PLACE.AdInterstitial);
	}

	public static bool TryShow(int lv)
	{
		if (!cf_const.IsOpenVictoryReward)
		{
			return false;
		}
		return cf_const.interstitialUnlockLv <= lv;
	}

	public static void Show(int lv, Action onCompleted)
	{
		VictoryRewardDialog victoryRewardDialog = SceneMgr.AddTopNode<VictoryRewardDialog>("Prefabs/VictoryRewardDialog");
		victoryRewardDialog._onCompleted = onCompleted;
		victoryRewardDialog._lv = lv;
	}
}
