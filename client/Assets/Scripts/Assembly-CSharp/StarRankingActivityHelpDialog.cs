using System;
using System.Collections.Generic;
using UnityEngine;

public class StarRankingActivityHelpDialog : DialogBase
{
	[SerializeField]
	private GameObject itemObj;

	[SerializeField]
	private Transform itemParent;

	public Action closeCallback;

	public void Show()
	{
		DB.StarRankingActivity.GetDanGrading(out var dan, out var grading);
		List<cfgStarRankingActivityReward> rewards = Singleton<StarRankingActivityMgr>.I.GetRewards(dan, grading);
		for (int i = 0; i < rewards.Count; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(itemObj, itemParent);
			gameObject.GetComponent<StarRankingActivityHelpItem>().Init(i, rewards[i]);
		}
		_Open();
	}

	public void CloseBtn(MButton btn)
	{
		btn.interactable = false;
		_Close(closeCallback);
	}

	public void Close()
	{
		_Close(closeCallback);
	}
}
