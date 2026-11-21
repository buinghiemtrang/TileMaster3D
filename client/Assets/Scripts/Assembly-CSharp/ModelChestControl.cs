using System.Linq;
using Festival;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class ModelChestControl : MonoBehaviour
{
	[SerializeField]
	private Transform chestParentTran;

	[SerializeField]
	private Text valueTex;

	[SerializeField]
	private Image imgProgress;

	[SerializeField]
	private bool isInGame;

	public Transform tfStar;

	private cfgModelChest cfg;

	private SkeletonGraphic _anim;

	public static bool NeedShowAnimation { get; set; }

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

	public void Add(int num)
	{
		SetSumStar(DB.User.SumStarLevel + num);
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
		if (isInGame)
		{
			valueTex.text = DB.User.SumStarLevel.ToString();
			return;
		}
		int num = DB.Chest.Num;
		if (num > cfg.completedNum)
		{
			num = cfg.completedNum;
		}
		valueTex.text = string.Format("{0}/{1}", num, cfg.completedNum);
		imgProgress.fillAmount = (float)num / (float)cfg.completedNum;
	}

	public void SetSumStar(int value)
	{
		DB.User.SumStarLevel = value;
		Debug.Log(string.Format(" DB.User.SumStarLevel {0}", DB.User.SumStarLevel));
		_setUI();
		_playAnim();
	}
}
