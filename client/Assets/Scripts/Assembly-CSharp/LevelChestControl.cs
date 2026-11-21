using System.Collections;
using DG.Tweening;
using Festival;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class LevelChestControl : MonoBehaviour
{
	public bool isVictory;

	[SerializeField]
	private Transform chestParentTran;

	public Text ttNum;

	public Image imgProgress;

	private bool bReceived;

	private int nMax;

	private int nCur;

	private string sRewards;

	private SkeletonGraphic _anim;

	private Tweener tweener;

	private void Awake()
	{
		DispatcherMgr.On("Update_Account", _UpdateData);
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off("Update_Account", _UpdateData);
		if (tweener != null)
		{
			tweener.Kill(false);
		}
		DOTween.Kill("DoTween_Level_Chest_Delay_Victory_NextFunction");
	}

	private void Start()
	{
		if ((bool)chestParentTran)
		{
			_anim = Singleton<FestivalMgr>.I.LoadAndInstantiate<SkeletonGraphic>("Chests/{0}_Chest");
			_anim.transform.SetParent(chestParentTran, false);
			_anim.Skeleton.SetSkin("Red");
			_anim.Skeleton.SetSlotsToSetupPose();
		}
		StartCoroutine(ShowLevelGift());
	}

	private void _UpdateData()
	{
		StartCoroutine(ShowLevelGift());
	}

	private void SetChestData()
	{
		if (DB.Chest.IdLv < DataCfg.cfgLevelChests.Count)
		{
			cfgLevelChest cfgLevelChest = DataCfg.cfgLevelChests[DB.Chest.IdLv];
			int num = ((DB.Chest.IdLv >= 2) ? DataCfg.cfgLevelChests[DB.Chest.IdLv - 1].lvId : 0);
			nMax = cfgLevelChest.lvId - num;
			nCur = DB.User.Maxlv - num;
			sRewards = cfgLevelChest.rewards;
		}
		else
		{
			cfgLevelChest cfgLevelChest2 = DataCfg.cfgLevelChests[0];
			cfgLevelChest cfgLevelChest3 = DataCfg.cfgLevelChests.Last();
			int idLv = DB.Chest.IdLv;
			nMax = cfgLevelChest2.lvId;
			nCur = (DataCfg.cfgLevelChests.Count - idLv - 1) * cfgLevelChest2.lvId - cfgLevelChest3.lvId + DB.User.Maxlv + cfgLevelChest2.lvId;
			int[,] array = cfgLevelChest2.rewards.ToInt(2, ',', '|');
			sRewards = string.Format("0|{0},{1}|{2}", Random.Range(array[0, 0], array[0, 1] + 1) * 10, array[1, 0], array[1, 1]);
		}
		if (DB.Chest.IdLv != 1 && nCur < 0)
		{
			int num2 = DB.Chest.IdLv - 1;
			if (num2 < 2)
			{
				num2 = 1;
			}
			DB.Chest.IdLv = num2;
			SetChestData();
		}
	}

	private void UpdateProgress()
	{
		if (!ttNum)
		{
			return;
		}
		ttNum.text = string.Format("{0}/{1}", nCur, nMax);
		imgProgress.fillAmount = (float)nCur / (float)nMax;
		bReceived = nMax <= nCur;
		if (_anim != null)
		{
			if (!bReceived)
			{
				_anim.AnimationState.SetAnimation(0, "idle", loop: true);
			}
			else
			{
				_anim.AnimationState.SetAnimation(0, "close-idle", loop: true);
			}
		}
	}

	public void ClickBtnGet(MButton button)
	{
		if (bReceived)
		{
			RewardType rType = ((DB.Chest.IdLv == 1) ? RewardType.Novice : RewardType.Chest);
			DB.Chest.IdLv = DB.Chest.IdLv + 1;
			Global.CreateReward(sRewards, rType, REWARD_PLACE.AdLevelBoxDouble, "Level").SetChestSkin("Red").SetComplete(delegate(bool isAd)
			{
				if (VictoryControl.instance != null)
				{
					VictoryControl.instance.isPlayRewardAd = isAd;
					VictoryControl.instance.NextFunction();
				}
				LogEvent.I.LevelBox_Reward(isAd ? "Free" : "Ads", nMax, sRewards);
				SetChestData();
				UpdateProgress();
			});
		}
	}

	private IEnumerator ShowLevelGift()
	{
		SetChestData();
		if (!isVictory)
		{
			UpdateProgress();
		}
		else
		{
			bReceived = nMax <= nCur;
			if (VictoryControl.instance != null)
			{
				VictoryControl.instance.SetBgHeight(!bReceived);
			}
			int num = Mathf.Clamp(nCur - 1, 0, nMax);
			ttNum.text = string.Format("{0}/{1}", num, nMax);
			imgProgress.fillAmount = (float)num / (float)nMax;
			tweener = TweenMgr.NumberTo(1f, num, nCur, delegate(float var)
			{
				ttNum.text = string.Format("{0}/{1}", (int)var, nMax);
				imgProgress.fillAmount = var / (float)nMax;
			});
			tweener.onComplete = delegate
			{
				if (VictoryControl.instance != null)
				{
					VictoryControl.instance.CreateFunctionQueue(!bReceived);
				}
				if (bReceived)
				{
					ClickBtnGet(null);
				}
				else
				{
					TweenMgr.DelayCall(0.5f, delegate
					{
						if (VictoryControl.instance != null)
						{
							VictoryControl.instance.NextFunction();
						}
					}).SetId("DoTween_Level_Chest_Delay_Victory_NextFunction");
				}
			};
		}
		yield return 0;
	}
}
