using System;
using System.Collections.Generic;
using Festival;
using UnityEngine;
using UnityEngine.UI;

namespace TreasureHunt
{
	public class TreasureHuntActivityDialog : DialogBase
	{
		[SerializeField]
		private TreasureHuntActivityRewardItem[] rewardItems;

		[SerializeField]
		private Text timeTex;

		[SerializeField]
		private GameObject finishValueObj;

		[SerializeField]
		private GameObject receiveBtnObj;

		[SerializeField]
		private GameObject closeBtnObj;

		[SerializeField]
		private TreasureHuntActivityItem ownItem;

		[SerializeField]
		private TreasureHuntActivityScroll scroll;

		[SerializeField]
		private Text barValueTex;

		[SerializeField]
		private Image barImg;

		[SerializeField]
		private GameObject bottomUIObj;

		[SerializeField]
		private Text receiveBtnWordTex;

		[SerializeField]
		private MButton closeBtn;

		[SerializeField]
		private MButton helpBtn;

		[SerializeField]
		private MButton receiveBtn;

		public Action<int> closeCallback;

		private int membersNum;

		protected override void Awake()
		{
			base.Awake();
			closeBtn.onClick.AddListener(_ClickClose);
			helpBtn.onClick.AddListener(_ClickHelp);
			receiveBtn.onClick.AddListener(_ClickReceive);
			DispatcherMgr.On<int>("TreasureHuntActivity_TimeChange", _RefreshTime);
			DispatcherMgr.On("TreasureHuntHttp_GetActivity_Succ", _RefreshBottomUI);
			DispatcherMgr.On("TreasureHuntHttp_GetTeamMembers_Succ", _RefreshUI);
		}

		public void Show(string openType, int surTime)
		{
			TreasureHuntHttpMgr.I.GetTeamMembers();
			_RefreshUI();
			_RefreshTime(surTime);
			_Open();
			LogEvent.I.HomeFrameOpen("TreasureHunt", openType);
		}

		private void _ClickClose(MButton btn)
		{
			btn.interactable = false;
			_Close(delegate
			{
				closeCallback?.Invoke(2);
			});
		}

		private void _ClickHelp(MButton btn)
		{
			Singleton<FestivalMgr>.I.AddTopNode<DialogShow>("Prefabs/Activity/TreasureHunt/{0}/TreasureHuntActivityHelpDialog");
		}

		private void _ClickReceive(MButton btn)
		{
			btn.interactable = false;
			int[,] rewards;
			if (!Singleton<TreasureHuntActivityMgr>.I.End(out rewards))
			{
				_Close(delegate
				{
					closeCallback?.Invoke(2);
				});
				return;
			}
			if (rewards.GetLength(0) == 0)
			{
				_Close(delegate
				{
					closeCallback?.Invoke(1);
				});
				return;
			}
			_Close();
			string text = ownItem.skinName;
			RewardType rType;
			if (string.IsNullOrEmpty(text))
			{
				TreasureHuntActivityRewardItem treasureHuntActivityRewardItem = Array.FindLast(rewardItems, fl => fl.state == 1);
				if (treasureHuntActivityRewardItem != null)
				{
					text = treasureHuntActivityRewardItem.skinName;
					rType = RewardType.ActivityOne;
				}
				else
				{
					rType = RewardType.ActivityTwo;
				}
			}
			else
			{
				rType = RewardType.ActivityTwo;
			}
			Global.CreateReward(rewards, rType, REWARD_PLACE.None, "PersonalReward").SetChestSkin(text).SetComplete(delegate
			{
				closeCallback?.Invoke(1);
			});
			int teamModel = 0;
			int myModel = 0;
			Singleton<TreasureHuntActivityMgr>.I.GetMembers().ForEach(delegate(MemberInfo fe)
			{
				int num = ((cf_const.treasureHuntActivityUnitScore != 0) ? (fe.score / cf_const.treasureHuntActivityUnitScore) : 0);
				teamModel += num;
				if (fe.isOwn)
				{
					myModel = num;
				}
			});
			Array.ForEach(rewardItems, delegate(TreasureHuntActivityRewardItem fe)
			{
				if (fe.state == 1)
				{
					LogEvent.I.Track("Team_Treasure_Box_Reward").Add("ActivityTime", DB.TreasureHuntActivity.Issue).Add("Team_ID", DB.TreasureHuntActivity.TeamId).Add("TeamBox", fe.cfg.id).Add("TeamModel", teamModel).Add("MyModel", myModel).CommonAttr(true).Send(false);
				}
			});
			LogEvent.I.Track("Team_Treasure_End").Add("ActivityTime", DB.TreasureHuntActivity.Issue).Add("Team_ID", DB.TreasureHuntActivity.TeamId).Add("MyRank", ownItem.ranking).Add("TeamModel", teamModel).Add("MyModel", myModel).CommonAttr(true).Send(false);
		}

		private void _RefreshUI()
		{
			List<MemberInfo> members = Singleton<TreasureHuntActivityMgr>.I.GetMembers();
			membersNum = members.Count;
			int totalScore = 0;
			members.ForEach(delegate(MemberInfo fe)
			{
				totalScore += fe.score;
			});
			List<cfgTreasureHuntActivityReward> cfgs;
			DataCenter.DBList<bool> activityRewardsReceivedState = Singleton<TreasureHuntActivityMgr>.I.GetActivityRewardsReceivedState(out cfgs);
			int maxScore = 0;
			int minScore = 0;
			int nextScore = 0;
			if (cfgs.Count > 0)
			{
				for (int i = 0; i < rewardItems.Length; i++)
				{
					cfgTreasureHuntActivityReward cfgTreasureHuntActivityReward = cfgs[i];
					int state = (activityRewardsReceivedState[i] ? 2 : ((totalScore >= cfgTreasureHuntActivityReward.condition) ? 1 : 0));
					rewardItems[i].Init(state, cfgTreasureHuntActivityReward);
					if (cfgTreasureHuntActivityReward.condition > maxScore)
					{
						maxScore = cfgTreasureHuntActivityReward.condition;
					}
					if (minScore == 0)
					{
						minScore = cfgTreasureHuntActivityReward.condition;
					}
					if (nextScore <= 0 && totalScore < cfgTreasureHuntActivityReward.condition)
					{
						nextScore = cfgTreasureHuntActivityReward.condition;
					}
				}
			}
			barValueTex.text = string.Format("{0}/{1}", totalScore, (nextScore == 0) ? maxScore : nextScore);
			barImg.fillAmount = Mathf.Min(1f, (float)totalScore / (float)maxScore);
			List<cfgTreasureHuntActivityRankingReward> activityRankingRewards = Singleton<TreasureHuntActivityMgr>.I.GetActivityRankingRewards();
			int num = members.FindIndex((MemberInfo m) => m.isOwn);
			if (num != -1)
			{
				bool flag = totalScore >= minScore;
				cfgTreasureHuntActivityRankingReward cfg = ((flag && num < activityRankingRewards.Count) ? activityRankingRewards[num] : null);
				ownItem.Init(cfg, num + 1, members[num]);
				scroll.RefreshItems(flag, activityRankingRewards, members);
			}
			_RefreshBottomUI();
		}

		private void _RefreshBottomUI()
		{
			bool flag = Singleton<TreasureHuntActivityMgr>.I.JudgeHasState(ActivityState.WaitingEnd);
			receiveBtnObj.SetActive(flag);
			closeBtnObj.SetActive(!flag);
			ownItem.gameObject.SetActive(membersNum > 8 && !flag);
			bottomUIObj.SetActive(receiveBtnObj.activeSelf || ownItem.gameObject.activeSelf);
			if (flag)
			{
				bool flag2 = Array.Exists(rewardItems, (TreasureHuntActivityRewardItem fl) => fl.state == 1);
				string lKey = "Language28";
				if (!flag2 && string.IsNullOrEmpty(ownItem.skinName))
				{
					lKey = "Language41";
				}
				receiveBtnWordTex.SetLKey(lKey);
			}
		}

		private void _RefreshTime(int surTime)
		{
			timeTex.text = Util.Helper.FormatTimer(surTime, "hh:mm:ss");
			if (finishValueObj.activeSelf != (surTime == 0))
			{
				finishValueObj.SetActive(surTime == 0);
			}
			if (timeTex.gameObject.activeSelf != (surTime != 0))
			{
				timeTex.gameObject.SetActive(surTime != 0);
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			DispatcherMgr.Off<int>("TreasureHuntActivity_TimeChange", _RefreshTime);
			DispatcherMgr.Off("TreasureHuntHttp_GetActivity_Succ", _RefreshBottomUI);
			DispatcherMgr.Off("TreasureHuntHttp_GetTeamMembers_Succ", _RefreshUI);
		}
	}
}
