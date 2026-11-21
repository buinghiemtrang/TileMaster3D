using System;
using System.Collections.Generic;
using Festival;
using RoyalPass;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace MonthRanking
{
	public class MonthRankingActivityDialog : DialogBase
	{
		[SerializeField]
		private Text timeTex;

		[SerializeField]
		private GameObject finishValueObj;

		[SerializeField]
		private GameObject receiveBtnObj;

		[SerializeField]
		private GameObject closeBtnObj;

		[SerializeField]
		private MonthRankingActivityItem ownItem;

		[SerializeField]
		private MonthRankingActivityScrollview scroll;

		[SerializeField]
		private GameObject bottomUIObj;

		[SerializeField]
		private Text receiveBtnWordTex;

		[SerializeField]
		private Sprite[] topBgs;

		[SerializeField]
		private MButton btnChest;

		[SerializeField]
		private SkeletonGraphic chestAnim;

		[SerializeField]
		private Text textOwnRank;

		[SerializeField]
		private GameObject rewardRootObj;

		[SerializeField]
		private Image imageTopBg;

		[SerializeField]
		private Image imageNoReward;

		public Action closeCallback;

		protected override void Awake()
		{
			base.Awake();
			DispatcherMgr.On<int>("MonthRankingActivity_TimeChange", _RefreshTime);
			DispatcherMgr.On("MonthRanking_GetActivityInfo_Succ", _RefreshUI);
			DispatcherMgr.On("MonthRanking_GetPersonalRankingData_Succ", _RefreshUI);
			if (rewardRootObj != null)
			{
				rewardRootObj.SetActive(false);
			}
		}

		public void Show(string openType, int leftTime)
		{
			MonthRankingActivityMgr.I.RequestRankingData();
			_RefreshUI();
			_RefreshTime(leftTime);
			_Open();
			LogEvent.I.Track("Month_Rank_Open").Add("ActivityTime", DB.MonthRankingActivity.Issue).CommonAttr(true).Send();
			LogEvent.I.HomeFrameOpen("MonthRanking", openType);
		}

		public void ClickClose(MButton btn)
		{
			_Close(closeCallback);
			if (DB.MonthRankingActivity.IsCanReceive)
			{
				RankingData ownRankingData = MonthRankingActivityMgr.I.GetOwnRankingData();
				if (MonthRankingActivityMgr.I.GetRewardsCfg(ownRankingData.ranking) == null)
				{
					MonthRankingActivityMgr.I.SetReceivedState();
				}
			}
		}

		public void ClickShowChest(MButton button)
		{
			RankingData ownRankingData = MonthRankingActivityMgr.I.GetOwnRankingData();
			if (ownRankingData == null)
			{
				return;
			}
			cfgMonthRankingActivityReward rewardsCfg = MonthRankingActivityMgr.I.GetRewardsCfg(ownRankingData.ranking);
			if (chestAnim == null || rewardsCfg == null || rewardsCfg.reward == null)
			{
				return;
			}
			RoyalPassActivityRewardTip tip = SceneMgr.AddTopNode<RoyalPassActivityRewardTip>("Prefabs/Activity/RoyalPass/RoyalPassActivityRewardTip");
			tip.yOffset = -50f;
			tip.xOffset = 0f;
			tip.ShowReward(chestAnim.transform, rewardsCfg.reward);
		}

		public void ClickHelp(MButton btn)
		{
			SceneMgr.AddTopNode<DialogShow>("Prefabs/Activity/MonthRanking/MonthRankingActivityHelpDialog");
		}

		public void ClickReceive(MButton btn)
		{
			RankingData ownRankingData = MonthRankingActivityMgr.I.GetOwnRankingData();
			cfgMonthRankingActivityReward rewardsCfg = MonthRankingActivityMgr.I.GetRewardsCfg(ownRankingData.ranking);
			if (!DB.MonthRankingActivity.IsCanReceive)
			{
				MonthRankingActivityMgr.I.SetReceivedState();
			}
			if (rewardsCfg != null && rewardsCfg.reward != null && rewardsCfg.reward.GetLength(0) > 0)
			{
				string monthRankingIssue = MonthRankingActivityMgr.I.GetMonthRankingIssue(DB.MonthRankingActivity.StartTime);
				LogEvent.I.Track("Month_Rank_Reward")
					.Add("ActivityTime", DB.MonthRankingActivity.Issue)
					.Add("MyRank", ownRankingData.ranking + 1)
					.Add("MyMonthStar", DB.User.GetMonthIssueSumStar(monthRankingIssue))
					.Add("MyMonthLevel", DB.User.GetMonthIssueLevel(monthRankingIssue))
					.Add("MyStar", DB.User.SumStar)
					.CommonAttr(true)
					.Send();
				RewardUnifyDialog rewardUnifyDialog = Global.CreateReward(rewardsCfg.reward, RewardType.ActivityTwo, REWARD_PLACE.None, "MonthRankReward");
				rewardUnifyDialog.SetChestSkin(rewardsCfg.skinName);
				rewardUnifyDialog.SetComplete(delegate
				{
					_Close(closeCallback);
				});
			}
			else
			{
				_Close(closeCallback);
			}
		}

		private void _RefreshUI()
		{
			bool isCanReceive = DB.MonthRankingActivity.IsCanReceive;
			RankingData ownRankingData = MonthRankingActivityMgr.I.GetOwnRankingData();
			string text = string.Empty;
			if (ownRankingData != null)
			{
				cfgMonthRankingActivityReward rewardsCfg = MonthRankingActivityMgr.I.GetRewardsCfg(ownRankingData.ranking);
				if (rewardsCfg != null)
				{
					text = rewardsCfg.skinName;
				}
			}
			bool flag = !isCanReceive && !string.IsNullOrEmpty(text);
			List<RankingData> list = MonthRankingActivityMgr.I.GetAllRankingData();
			if (ownRankingData != null && list == null)
			{
				ownRankingData.ranking = 0;
				list = new List<RankingData> { ownRankingData };
			}
			if (scroll != null)
			{
				scroll.RefreshItems(list);
			}
			if (rewardRootObj != null)
			{
				rewardRootObj.SetActive(isCanReceive);
			}
			if (imageTopBg != null && topBgs != null && topBgs.Length >= 1)
			{
				if (isCanReceive)
				{
					imageTopBg.sprite = topBgs[0];
				}
				else
				{
					imageTopBg.sprite = FestivalMgr.I.LoadAsset<Sprite>("Festival/MonthRanking/{0}/MonthlyRankingOpen");
				}
			}
			if (isCanReceive)
			{
				string text2 = flag ? string.Format("{0}", ownRankingData.ranking + 1) : string.Format("{0}+", cf_const.monthRankingActivityLimit);
				if (textOwnRank != null)
				{
					textOwnRank.text = text2;
				}
				if (chestAnim != null)
				{
					chestAnim.gameObject.SetActive(flag);
				}
				if (imageNoReward != null)
				{
					imageNoReward.gameObject.SetActive(!flag);
				}
				if (flag && chestAnim != null)
				{
					chestAnim.Skeleton.SetSkin(text);
					chestAnim.Skeleton.SetSlotsToSetupPose();
					chestAnim.AnimationState.SetAnimation(0, "idle", true);
				}
			}
			if (receiveBtnObj != null)
			{
				receiveBtnObj.SetActive(isCanReceive);
			}
			if (closeBtnObj != null)
			{
				closeBtnObj.SetActive(!isCanReceive);
			}
			bool flag2 = ownRankingData != null && !isCanReceive;
			if (ownItem != null)
			{
				ownItem.gameObject.SetActive(flag2);
				if (flag2)
				{
					ownItem.Init(ownRankingData);
				}
			}
			if (bottomUIObj != null)
			{
				bottomUIObj.SetActive(isCanReceive || ownRankingData != null);
			}
			if (receiveBtnWordTex != null)
			{
				receiveBtnWordTex.SetLKey(flag ? "Language28" : "Language41");
			}
		}

		private void _RefreshTime(int leftTime)
		{
			timeTex.text = Helper.FormatTimer(leftTime, "hh:mm:ss");
			bool flag = leftTime == 0 || DB.MonthRankingActivity.State == 4;
			if (finishValueObj.activeSelf != flag)
			{
				finishValueObj.SetActive(flag);
			}
			if (timeTex.gameObject.activeSelf == flag)
			{
				timeTex.gameObject.SetActive(!flag);
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			DispatcherMgr.Off<int>("MonthRankingActivity_TimeChange", _RefreshTime);
			DispatcherMgr.Off("MonthRanking_GetActivityInfo_Succ", _RefreshUI);
			DispatcherMgr.Off("MonthRanking_GetPersonalRankingData_Succ", _RefreshUI);
		}
	}
}
