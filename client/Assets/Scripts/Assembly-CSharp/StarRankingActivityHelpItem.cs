using System;
using UnityEngine;
using UnityEngine.UI;

public class StarRankingActivityHelpItem : MonoBehaviour
{
	[Serializable]
	public class RewardItem
	{
		public GameObject obj;

		public Image iconImg;

		public Text numTex;
	}

	[SerializeField]
	private Text gearTex;

	[SerializeField]
	private Image gearImg;

	[SerializeField]
	private Sprite[] grarSp;

	[SerializeField]
	private RewardItem[] items;

	public void Init(int index, cfgStarRankingActivityReward cfg)
	{
		base.gameObject.SetActive(true);
		gearTex.gameObject.SetActive(index >= 3);
		gearImg.gameObject.SetActive(index < 3);
		if (index < 3)
		{
			gearImg.sprite = grarSp[index];
		}
		else
		{
			gearTex.text = string.Format("{0}~{1}", cfg.condition[0], cfg.condition[1]);
		}
		int length = cfg.reward.GetLength(0);
		if (items.Length < length)
		{
			Debug.LogError("star ranking activity help page reward data error");
			return;
		}
		for (int i = 0; i < items.Length; i++)
		{
			items[i].obj.SetActive(i < length);
			if (i < length)
			{
				items[i].iconImg.sprite = LoadMgr.LoadSprite(string.Format("Props/prop{0}", cfg.reward[i, 0]));
				if (cfg.reward[i, 0] == 21)
				{
					items[i].numTex.text = string.Format("{0}h", cfg.reward[i, 1]);
				}
				else
				{
					items[i].numTex.text = string.Format("X{0}", cfg.reward[i, 1]);
				}
			}
		}
	}
}
