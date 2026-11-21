using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class StarRankingActivityRewardDialog : DialogBase
{
	[SerializeField]
	private StarRankingActivityHelpItem.RewardItem[] rewardItems;

	[SerializeField]
	private Text namtTex;

	[SerializeField]
	private Text scoreTex;

	[SerializeField]
	private Text rankingTex;

	[SerializeField]
	private StarRankingActivityItem[] rankingItems;

	[SerializeField]
	private MButton trueBtn;

	[SerializeField]
	private MButton receiveBtn;

	[SerializeField]
	private Image ownImgRank;

	[SerializeField]
	private StarRankingActivityDanGradingItem danGradingItem;

	public Action closeCallback;

	private int[,] reward;

	private StarRankingData pOwn;

	private List<StarRankingData> rankingDatas;

	private int lastLevel;

	private int curLevel;

	protected override void Awake()
	{
		base.Awake();
		trueBtn.onClick.AddListener(_ReceiveBtn);
		receiveBtn.onClick.AddListener(_ReceiveBtn);
	}

	public void Show()
	{
		rankingDatas = Singleton<StarRankingActivityMgr>.I.GetPlayerData(out pOwn);
		for (int i = 0; i < rankingItems.Length && i < rankingDatas.Count; i++)
		{
			rankingItems[i].Init(rankingDatas[i], false, true);
		}
		namtTex.text = pOwn.name;
		scoreTex.text = pOwn.score.ToString();
		rankingTex.text = pOwn.ranking.ToString();
		if (pOwn.ranking < 4)
		{
			ownImgRank.gameObject.SetActive(true);
			ownImgRank.sprite = rankingItems[pOwn.ranking - 1].RankImage.sprite;
		}
		DB.StarRankingActivity.LevelToDanGrading(pOwn.danGrading, out var dan, out var grading);
		int upDown = DB.StarRankingActivity.DanGradingConditionBy(pOwn.ranking);
		danGradingItem.SetDanGrading(dan, grading);
		danGradingItem.SetUpDownIcon(upDown);
		List<cfgStarRankingActivityReward> rewards = Singleton<StarRankingActivityMgr>.I.GetRewards(dan, grading);
		cfgStarRankingActivityReward cfgStarRankingActivityReward = null;
		for (int num = rewards.Count - 1; num >= 0; num--)
		{
			if (pOwn.ranking >= rewards[num].condition[0] && pOwn.ranking <= rewards[num].condition[1])
			{
				cfgStarRankingActivityReward = rewards[num];
				break;
			}
		}
		receiveBtn.gameObject.SetActive(cfgStarRankingActivityReward != null);
		trueBtn.gameObject.SetActive(cfgStarRankingActivityReward == null);
		reward = ((cfgStarRankingActivityReward == null) ? new int[0, 0] : cfgStarRankingActivityReward.reward);
		int length = reward.GetLength(0);
		if (rewardItems.Length < length)
		{
			return;
		}
		for (int j = 0; j < rewardItems.Length; j++)
		{
			rewardItems[j].obj.SetActive(j < length);
			if (j < length)
			{
				float y = 6f;
				if (rewardItems.Length <= length)
				{
					y = ((j != length - 1 && j != 0) ? 6f : (-39f));
				}
				rewardItems[j].obj.transform.SetLocPos((float)(j * 2 + 1) * 47f - ((float)length * 94f + (float)(length - 1) * 34f) * 0.5f + (float)(j * 34), y);
				if (reward[j, 0] == 23)
				{
					rewardItems[j].iconImg.sprite = LoadMgr.LoadSprite(string.Format("Props/medal{0}", reward[j, 1]));
					rewardItems[j].numTex.text = "X1";
				}
				else
				{
					rewardItems[j].iconImg.sprite = LoadMgr.LoadSprite(string.Format("Props/prop{0}", reward[j, 0]));
					if (reward[j, 0] == 21)
					{
						rewardItems[j].numTex.text = string.Format("{0}h", reward[j, 1]);
					}
					else
					{
						rewardItems[j].numTex.text = string.Format("X{0}", reward[j, 1]);
					}
				}
			}
		}
		_Open();
	}

	private void _ReceiveBtn(MButton btn)
	{
		btn.interactable = false;
		lastLevel = DB.StarRankingActivity.Level;
		if (cf_const.activityRankingStarUpDownCondition[0] < pOwn.ranking)
		{
			if (cf_const.activityRankingStarUpDownCondition[1] <= pOwn.ranking)
			{
				DB.StarRankingActivity.ModifyLevel(-1);
			}
		}
		else
		{
			DB.StarRankingActivity.ModifyLevel(1);
		}
		curLevel = DB.StarRankingActivity.Level;
		LogEvent.I.Track("").Add("MyStarStep", curLevel + 1).UserSet();
		Singleton<StarRankingActivityMgr>.I.Receive();
		int length = reward.GetLength(0);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < length; i++)
		{
			int num = reward[i, 0];
			if (num == 21)
			{
				DB.Bag.AddEnergyUnlimitedTime(reward[i, 1] * 3600);
			}
			else
			{
				DB.Bag.Modify((PropID)num, reward[i, 1], true, "ActivityRanking");
			}
			stringBuilder.Append(reward[i, 0]);
			stringBuilder.Append("|");
			stringBuilder.Append(reward[i, 1]);
			if (i < length - 1)
			{
				stringBuilder.Append(",");
			}
		}
		_Close(closeCallback);
		LogEvent.I.Track("Activity_Ranking_Reward").Add("RankNum", pOwn.ranking).Add("StarNum", pOwn.score)
			.Add("No1StarNum", rankingDatas[0].score)
			.Add("MyStep", DB.StarRankingActivity.Level + 1)
			.Add("TaskItem", stringBuilder.ToString())
			.CommonAttr(true)
			.Send();
		LogEvent.I.Track("Activity_Ranking_Change").Add("MyStep", DB.StarRankingActivity.Level + 1).CommonAttr(true)
			.Send();
		StarRankingActivityDanGradingDialog.Create().Show(lastLevel + 1, curLevel + 1);
	}
}
