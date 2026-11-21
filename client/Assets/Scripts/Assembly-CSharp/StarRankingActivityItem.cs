using UnityEngine;
using UnityEngine.UI;

public class StarRankingActivityItem : StarRankingActivityItemBase
{
	[SerializeField]
	private Text nameTex;

	[SerializeField]
	private Text rankTex;

	[SerializeField]
	private Text scoreTex;

	[SerializeField]
	private Image rankImg;

	[SerializeField]
	private Image bgImg;

	[SerializeField]
	private Image scoreImg;

	[SerializeField]
	public Sprite[] rankSp;

	[SerializeField]
	public Sprite[] bgSp;

	[SerializeField]
	public Sprite[] scoreSp;

	[SerializeField]
	private StarRankingActivityDanGradingItem danGradingItem;

	public Image RankImage => rankImg;

	public override void Init(StarRankingData data, bool bUpDown, bool bShowUpDownIcon)
	{
		nameTex.text = data.name;
		if ((bool)rankTex)
		{
			rankTex.text = data.ranking.ToString();
		}
		scoreTex.text = data.score.ToString();
		if ((bool)rankTex)
		{
			rankTex.gameObject.SetActive(data.ranking > 3);
		}
		rankImg.gameObject.SetActive(data.ranking < 4);
		if (data.ranking < 4)
		{
			rankImg.sprite = rankSp[data.ranking - 1];
		}
		int num = (!data.isOwn) ? 1 : 0;
		bgImg.sprite = bgSp[num];
		scoreImg.sprite = scoreSp[num];
		int num2 = DB.StarRankingActivity.DanGradingConditionBy(data.ranking);
		if (bShowUpDownIcon)
		{
			danGradingItem.SetUpDownIcon(num2);
		}
		int lv = data.danGrading;
		if (bUpDown)
		{
			if (num2 == -1)
			{
				lv = data.danGrading - 1;
			}
			else if (num2 == 1)
			{
				lv = data.danGrading + 1;
			}
		}
		danGradingItem.SetDanGrading(lv);
	}
}
