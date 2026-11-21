using System;
using UnityEngine;
using UnityEngine.UI;

public class FreeGold : MonoBehaviour
{
	[SerializeField]
	private Text numTex;

	[SerializeField]
	private Text rewardTex;

	[SerializeField]
	private CanvasGroup group;

	public Action cback;

	public Action<bool> lockCloseBtn;

	private Transform self;

	private void Awake()
	{
		self = base.transform;
		base.gameObject.SetActive(value: false);
	}

	public void Init()
	{
		int surFreeGoldNum = DB.User.SurFreeGoldNum;
		if (surFreeGoldNum >= 1)
		{
			Hide(isHide: false);
			numTex.text = surFreeGoldNum.ToString();
			rewardTex.text = string.Format("+{0}", cf_const.freeGoldReward);
		}
	}

	public void ClickAd(MButton btn)
	{
		Global.ShowRewardedVideo(delegate(string info)
		{
			if (info == "succ" && numTex != null)
			{
				int num = DB.User.SurFreeGoldNum - 1;
				DB.User.SurFreeGoldNum = num;
				numTex.text = num.ToString();
				if (num == 0)
				{
					Hide(isHide: true);
				}
				string rewards = string.Format("0|{0}", cf_const.freeGoldReward);
				RewardUnifyDialog rewardUnifyDialog = Global.CreateReward(rewards, RewardType.None, REWARD_PLACE.None, "FreeGold", false);
				if (rewardUnifyDialog != null)
				{
					rewardUnifyDialog.SetComplete(delegate
					{
						cback?.Invoke();
					});
				}
			}
		}, REWARD_PLACE.AdGiveCoin);
	}

	public void Hide(bool isHide)
	{
		base.gameObject.SetActive(!isHide);
	}
}
