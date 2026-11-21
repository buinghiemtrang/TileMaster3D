using System;
using System.Text;
using RoyalPass;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class VictoriesActivityRewardItem : MonoBehaviour
{
	[SerializeField]
	private Text valueTex;

	[SerializeField]
	private Sprite[] bgSpr;

	[SerializeField]
	private Image bgImg;

	[SerializeField]
	private SkeletonGraphic anim;

	public Action onReceived;

	public VictoriesActivityRewardInfo info;

	public string skinName;

	public void Init(VictoriesActivityRewardInfo info)
	{
		this.info = info;
		gameObject.SetActive(true);
		RefreshUI();
	}

	public void RefreshUI()
	{
		valueTex.text = info.cfg.condition.ToString();
		if (info.rewardState == 0)
		{
			bgImg.sprite = bgSpr[0];
		}
		else
		{
			bgImg.sprite = bgSpr[1];
		}
		_PlayChestAnimation();
	}

	private void _PlayChestAnimation()
	{
		string animationName;
		if (info.rewardState == 0)
		{
			animationName = "idle";
		}
		else if (info.rewardState == 1)
		{
			animationName = "close_idle";
		}
		else
		{
			anim.transform.SetLocPosX(-4f);
			animationName = "open_idle";
		}
		anim.AnimationState.SetAnimation(0, animationName, true);
	}

	public void ClickReceive(MButton btn)
	{
		if (info == null)
		{
			return;
		}
		if (info.rewardState == 1)
		{
			int[,] array = VictoriesActivityMgr.I.Receive(info.cfg.id);
			if (array == null || array.GetLength(0) == 0)
			{
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int length = array.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				stringBuilder.Append(array[i, 0]);
				stringBuilder.Append("=");
				stringBuilder.Append(array[i, 1]);
				if (i < length - 1)
				{
					stringBuilder.Append(";");
				}
			}
			LogEvent.I.Track("Activity_Victories_Reward")
				.Add("VictoriesNum", DB.VictoriesActivity.HighestVictoryNum)
				.Add("TaskItem", stringBuilder.ToString())
				.CommonAttr()
				.Send();
			RewardUnifyDialog rewardUnifyDialog = Global.CreateReward(array, RewardType.ActivityOne, REWARD_PLACE.None, "Victories", false, false);
			if (rewardUnifyDialog != null)
			{
				rewardUnifyDialog.SetChestSkin(skinName);
				rewardUnifyDialog.SetComplete(delegate
				{
					onReceived?.Invoke();
					info.rewardState = 2;
					RefreshUI();
				});
			}
		}
		else if (info.rewardState == 0)
		{
			RoyalPassActivityRewardTip royalPassActivityRewardTip = RoyalPassActivityRewardTip.Create();
			if (royalPassActivityRewardTip != null)
			{
				royalPassActivityRewardTip.yOffset = -50f;
				royalPassActivityRewardTip.xOffset = 0f;
				royalPassActivityRewardTip.ShowReward(base.transform, info.cfg.reward);
			}
		}
	}
}
