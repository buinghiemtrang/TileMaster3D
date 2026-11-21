using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Festival;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class ModelChestInMain : MonoBehaviour
{
	[SerializeField]
	private Transform chestParentTran;

	[SerializeField]
	private Text valueTex;

	[SerializeField]
	private Image imgProgress;

	private cfgModelChest cfg;

	private SkeletonGraphic _anim;

	[SerializeField]
	private RectTransform endPoint;

	[SerializeField]
	private GameObject animationObject;

	[SerializeField]
	private RectTransform startTransform;

	[SerializeField]
	private RectTransform endTransform;

	[SerializeField]
	private RectTransform bezierTransform;

	private List<GameObject> _starsPool = new List<GameObject>();

	private static int CacheStarCount = -1;

	private void Awake()
	{
		DispatcherMgr.On("Update_Account", Init);
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off("Update_Account", Init);
	}

	private void Start()
	{
		if ((bool)chestParentTran)
		{
			_anim = FestivalMgr.I.LoadAndInstantiate<SkeletonGraphic>("Chests/{0}_Chest");
			_anim.transform.SetParent(chestParentTran, false);
			_anim.Skeleton.SetSkin("Green");
			_anim.Skeleton.SetSlotsToSetupPose();
			Init();
		}
	}

	public void Init()
	{
		RefreshData();
		if (CacheStarCount == -1)
		{
			int num = DB.Chest.Num;
			if (num > cfg.completedNum)
			{
				num = cfg.completedNum;
			}
			CacheStarCount = num;
		}
		_setUI();
		_playAnim();
	}

	public void RefreshData()
	{
		DB.User.SumStarLevel = 0;
		if (DB.Chest.Id != 0)
		{
			cfg = DataCfg.cfgModelChests.FirstOrDefault((cfgModelChest s) => s.id == DB.Chest.Id);
		}
		if (cfg == null)
		{
			DB.Chest.AddStar(0, true);
			cfg = DataCfg.cfgModelChests.Random();
			DB.Chest.Id = cfg.id;
			DB.Chest.IsReceived = false;
		}
		else if (DB.Chest.IsReceived)
		{
			int star = DB.Chest.Num - cfg.completedNum;
			DB.Chest.AddStar(star, true);
			cfg = DataCfg.cfgModelChests.Random();
			DB.Chest.Id = cfg.id;
			DB.Chest.IsReceived = false;
		}
		DB.Chest.IsCompleted = DB.Chest.Num >= cfg.completedNum;
	}

	private void RefreshUI()
	{
		_setUI();
		_playAnim();
	}

	public void ClickBtnGet(MButton button)
	{
		if (!DB.Chest.IsCompleted || DB.Chest.IsReceived)
		{
			return;
		}
		DB.Chest.IsReceived = true;
		RefreshData();
		RewardUnifyDialog rewardUnifyDialog = Global.CreateReward(cfg.rewards, RewardType.Chest, REWARD_PLACE.AdModelBoxDouble, "Model");
		rewardUnifyDialog.SetChestSkin("Green");
		rewardUnifyDialog.SetComplete(delegate(bool isDouble)
		{
			LogEvent.I.ModelBox_Reward(isDouble ? "Free" : "Ads", cfg.completedNum, cfg.rewards);
			int num = DB.Chest.Num;
			if (num > cfg.completedNum)
			{
				num = cfg.completedNum;
			}
			CacheStarCount = num;
			_setUI();
			_playAnim();
		});
	}

	private void _playAnim()
	{
		if ((bool)_anim)
		{
			_anim.AnimationState.SetAnimation(0, DB.Chest.IsCompleted ? "close-idle" : "idle", true);
		}
	}

	private void _setUI()
	{
		valueTex.text = string.Format("{0}/{1}", CacheStarCount, cfg.completedNum);
		imgProgress.fillAmount = (float)CacheStarCount / (float)cfg.completedNum;
	}

	private GameObject GetStar()
	{
		GameObject gameObject = _starsPool.FirstOrDefault((GameObject s) => !s.activeSelf);
		if (gameObject == null)
		{
			gameObject = Object.Instantiate(animationObject, endPoint.parent.transform);
			_starsPool.Add(gameObject);
		}
		return gameObject;
	}

	public IEnumerator DoPlayProgressAnimation()
	{
		if (ModelChestControl.NeedShowAnimation)
		{
			ModelChestControl.NeedShowAnimation = false;
			yield return new WaitForSeconds(0.2f);
			animationObject.SetActive(true);
			animationObject.transform.localPosition = startTransform.localPosition;
			endTransform.position = endPoint.position;
			yield return new WaitForSeconds(0.5f);
			for (int i = 0; i < 5; i++)
			{
				GameObject star = GetStar();
				star.SetActive(true);
				star.transform.localPosition = startTransform.localPosition;
				star.transform.DOLocalBezier(0.5f, startTransform.localPosition, bezierTransform.localPosition, endTransform.localPosition).OnComplete(delegate
				{
					star.SetActive(false);
				});
				yield return new WaitForSeconds(0.05f);
			}
			animationObject.SetActive(false);
			yield return new WaitForSeconds(0.5f);
			ShowProgressAnimation();
		}
	}

	private void ShowProgressAnimation()
	{
		int targetValue = DB.Chest.Num;
		if (targetValue > cfg.completedNum)
		{
			targetValue = cfg.completedNum;
		}
		int tempValue = CacheStarCount;
		CacheStarCount = targetValue;
		DOTween.To(() => tempValue, delegate(int x)
		{
			tempValue = x;
		}, targetValue, 0.5f).OnUpdate(delegate
		{
			valueTex.text = string.Format("{0}/{1}", tempValue, cfg.completedNum);
			imgProgress.fillAmount = (float)tempValue / (float)cfg.completedNum;
		});
	}
}
