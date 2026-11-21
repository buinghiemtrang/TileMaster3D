using RoyalPass;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class RegionRankPanelItem : MonoBehaviour
{
	[Header("排名")]
	[SerializeField]
	private Text rankingTex;

	[SerializeField]
	private Image rankingImg;

	[SerializeField]
	private Sprite[] rankingSprites;

	[SerializeField]
	private Image medalIcon;

	[SerializeField]
	private Image flagIcon;

	[SerializeField]
	private Text nameTex;

	[SerializeField]
	private Text scoreTex;

	[SerializeField]
	protected GameObject selfBg; // [rebuild] private→protected: RegionRankEndPanelItem.IsSelf đọc trực tiếp this+0x88

	[Header("奖励")]
	[SerializeField]
	private SkeletonGraphic personalChest;

	[SerializeField]
	private SkeletonGraphic nationalChest;

	[SerializeField]
	private MButton chestButton;

	private int _ranking;

	private RegionRankType _rankType;

	private RegionRankClientItemData _itemData;

	public cfgRegionRankReward RewardConfig;

	private void Awake()
	{
		chestButton.onClick.AddListener(ClickShowChest);
	}

	public virtual bool IsSelf()
	{
		if (_rankType == RegionRankType.Personanl)
		{
			return _itemData.Key == DB.Account.Userid;
		}
		return _itemData.Key == GSingleton<RegionRankActivityManager>.ins.GetRegionCode();
	}

	private void SetRanking()
	{
		rankingImg.gameObject.SetActive(_ranking < 4);
		rankingTex.gameObject.SetActive(_ranking > 3);
		rankingTex.text = ((_ranking < 1001) ? _ranking.ToString() : "1000+");
		if (_ranking >= 1 && _ranking <= 3)
		{
			rankingImg.sprite = rankingSprites[_ranking - 1];
		}
	}

	private void SetName()
	{
		if (_rankType == RegionRankType.Personanl)
		{
			nameTex.text = ((_itemData.Data != null) ? _itemData.Data.name : _itemData.Key);
			return;
		}
		cfgRegionRankRegions regionConfig = GSingleton<RegionRankActivityManager>.ins.GetRegionConfig(_itemData.Key);
		if (regionConfig != null)
		{
			nameTex.SetLKey(regionConfig.Desc);
		}
		else
		{
			nameTex.text = "";
		}
	}

	private void SetIcon()
	{
		Image image;
		string sFile;
		if (_rankType == RegionRankType.Personanl)
		{
			medalIcon.gameObject.SetActive(true);
			flagIcon.gameObject.SetActive(false);
			if (_itemData.Data == null)
			{
				medalIcon.gameObject.SetActive(false);
				return;
			}
			cfg_medal cfgById = MedalMgr.I.GetCfgById(_itemData.Data.medal);
			if (cfgById == null)
			{
				return;
			}
			image = medalIcon;
			sFile = "MedalIcon/" + cfgById.Icon;
		}
		else
		{
			medalIcon.gameObject.SetActive(false);
			flagIcon.gameObject.SetActive(true);
			cfgRegionRankRegions regionConfig = GSingleton<RegionRankActivityManager>.ins.GetRegionConfig(_itemData.Key);
			if (regionConfig == null)
			{
				return;
			}
			image = flagIcon;
			sFile = regionConfig.Res;
		}
		image.sprite = LoadMgr.LoadSprite(sFile);
	}

	public void Init(RegionRankClientItemData data, int ranking, RegionRankType rankType)
	{
		_itemData = data;
		_ranking = ((ranking != 0) ? ranking : 1001);
		_rankType = rankType;
		RewardConfig = GSingleton<RegionRankActivityManager>.ins.GetRankReward(_ranking, _rankType);
		SetName();
		SetRanking();
		SetIcon();
		selfBg.SetActive(IsSelf());
		scoreTex.text = data.Score.ToString();
		chestButton.gameObject.SetActive(RewardConfig != null);
		if (RewardConfig == null)
		{
			return;
		}
		SkeletonGraphic skeletonGraphic;
		if (rankType == RegionRankType.Personanl)
		{
			nationalChest.gameObject.SetActive(false);
			personalChest.gameObject.SetActive(true);
			skeletonGraphic = personalChest;
		}
		else
		{
			personalChest.gameObject.SetActive(false);
			nationalChest.gameObject.SetActive(true);
			skeletonGraphic = nationalChest;
		}
		skeletonGraphic.Skeleton.SetSkin(RewardConfig.ChestSkin);
	}

	private void ClickShowChest(MButton button)
	{
		if (RewardConfig != null)
		{
			RoyalPassActivityRewardTip royalPassActivityRewardTip = SceneMgr.AddTopNode<RoyalPassActivityRewardTip>("Prefabs/Activity/RoyalPass/RoyalPassActivityRewardTip");
			royalPassActivityRewardTip.yOffset = -50f;
			royalPassActivityRewardTip.xOffset = 0f;
			royalPassActivityRewardTip.ShowReward(chestButton.transform, RewardConfig.Reward);
		}
	}
}
