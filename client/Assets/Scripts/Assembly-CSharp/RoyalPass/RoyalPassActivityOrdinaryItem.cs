using System;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace RoyalPass
{
	public class RoyalPassActivityOrdinaryItem : RoyalPassActivityItem
	{
		[SerializeField]
		private RoyalPassActivityOrdinaryItemRewardUI[] rewardUI;

		[SerializeField]
		private GameObject starObj;

		[SerializeField]
		private Text lvTex;

		[SerializeField]
		private SkeletonGraphic lineAnim;

		[SerializeField]
		private SkeletonGraphic barAnim;

		[SerializeField]
		private Sprite[] gradeBgSps;

		[SerializeField]
		private Image gradeBgImg;

		[SerializeField]
		private Color[] lvValueOutlineColor;

		[SerializeField]
		private Color[] lvValueShadowColor;

		private void Awake()
		{
			DispatcherMgr.On<int>("RoyalPassActivity_UpgradeFinish", _OnUpgradeFinish);
			DispatcherMgr.On<int>("RoyalPassActivity_ScrollStartMove", _OnScrollStartMove);
			DispatcherMgr.On<bool, int>("RoyalPassActivity_ScrollMoveFinish", _OnScrollMoveFinish);
		}

		public override void Init(int ordinaryRewardsNum, RoyalPassActivityRewardInfo info)
		{
			base.info = info;
			base.ordinaryRewardsNum = ordinaryRewardsNum;
			int grade = info.cfg.grade;
			starObj.SetActive(grade == 0);
			lvTex.gameObject.SetActive(grade != 0);
			lvTex.text = grade.ToString();
			Array.ForEach(rewardUI, delegate(RoyalPassActivityOrdinaryItemRewardUI fe)
			{
				fe.Init(info);
			});
			int curGrade = DB.RoyalPassActivity.CurGrade;
			int num = ((curGrade < info.cfg.grade) ? 1 : 0);
			string animationName = ((curGrade < info.cfg.grade) ? "1" : "3");
			gradeBgImg.sprite = gradeBgSps[num];
			Shadow[] components = lvTex.GetComponents<Shadow>();
			components[0].effectColor = lvValueOutlineColor[num];
			components[1].effectColor = lvValueShadowColor[num];
			barAnim.AnimationState.SetAnimation(0, animationName, true);
			_OnScrollMoveFinish(false, curGrade);
		}

		private void _OnUpgradeFinish(int grade)
		{
			if (info == null || !lineAnim)
			{
				return;
			}
			if (info.cfg.grade == grade)
			{
				TrackEntry trackEntry = lineAnim.AnimationState.SetAnimation(0, "1-out", false);
				trackEntry.TimeScale = 0.7f;
				trackEntry.Complete += delegate
				{
					lineAnim.gameObject.SetActive(false);
				};
			}
			else if (grade + 1 == info.cfg.grade)
			{
				TrackEntry trackEntry2 = lineAnim.AnimationState.SetAnimation(0, "2-out", false);
				trackEntry2.TimeScale = 0.7f;
				trackEntry2.Complete += delegate
				{
					lineAnim.gameObject.SetActive(false);
				};
			}
		}

		private void _OnScrollStartMove(int grade)
		{
			if (info != null && (bool)barAnim && (bool)gradeBgImg && info.cfg.grade == grade)
			{
				TrackEntry trackEntry = barAnim.AnimationState.SetAnimation(0, "2", false);
				trackEntry.TimeScale = 0.7f;
				barAnim.AnimationState.AddAnimation(0, "3", false, 0f);
				if (gradeBgSps.Length != 0)
				{
					gradeBgImg.sprite = gradeBgSps[0];
				}
			}
		}

		private void _OnScrollMoveFinish(bool isPlayShowAnim, int grade)
		{
			if (info == null || !lineAnim)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			bool active = false;
			if (grade < ordinaryRewardsNum - 1)
			{
				int grade2 = info.cfg.grade;
				if (grade2 == grade)
				{
					active = true;
					flag = true;
				}
				else
				{
					active = grade2 == grade + 1;
					flag2 = active;
				}
			}
			lineAnim.gameObject.SetActive(active);
			if (flag)
			{
				if (!isPlayShowAnim)
				{
					lineAnim.AnimationState.SetAnimation(0, "1-idle", true);
					return;
				}
				lineAnim.AnimationState.SetAnimation(0, "1-show", false).TimeScale = 0.7f;
				lineAnim.AnimationState.AddAnimation(0, "1-idle", true, 0f);
			}
			else if (flag2)
			{
				if (!isPlayShowAnim)
				{
					lineAnim.AnimationState.SetAnimation(0, "2-idle", true);
					return;
				}
				lineAnim.AnimationState.SetAnimation(0, "2-show", false).TimeScale = 0.7f;
				lineAnim.AnimationState.AddAnimation(0, "2-idle", true, 0f);
			}
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off<int>("RoyalPassActivity_UpgradeFinish", _OnUpgradeFinish);
			DispatcherMgr.Off<int>("RoyalPassActivity_ScrollStartMove", _OnScrollStartMove);
			DispatcherMgr.Off<bool, int>("RoyalPassActivity_ScrollMoveFinish", _OnScrollMoveFinish);
		}
	}
}
