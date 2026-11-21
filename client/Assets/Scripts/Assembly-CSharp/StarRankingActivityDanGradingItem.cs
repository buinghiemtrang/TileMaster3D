using UnityEngine;
using UnityEngine.UI;

public class StarRankingActivityDanGradingItem : MonoBehaviour
{
	[SerializeField]
	private Image imgBackground;

	[SerializeField]
	private Image[] imgStar;

	[SerializeField]
	private GameObject objUp;

	[SerializeField]
	private GameObject objDown;

	[SerializeField]
	private Image imgChange;

	public AnimationControl aniCtrl;

	private Vector3[][] _starPositions = new Vector3[5][]
	{
		new Vector3[1]
		{
			new Vector3(2f, 13f, 92f)
		},
		new Vector3[2]
		{
			new Vector3(-31f, 10f, 75f),
			new Vector3(35f, 10f, 75f)
		},
		new Vector3[3]
		{
			new Vector3(2f, 22.4f, 88f),
			new Vector3(48.9f, 3f, 50f),
			new Vector3(-45.5f, 3f, 50f)
		},
		new Vector3[4]
		{
			new Vector3(27f, 56f, 50f),
			new Vector3(37f, 2f, 70f),
			new Vector3(-34f, 2f, 70f),
			new Vector3(-22.7f, 56f, 50f)
		},
		new Vector3[5]
		{
			new Vector3(3f, 16.4f, 88f),
			new Vector3(49.9f, -3f, 50f),
			new Vector3(-44.5f, 3f, 50f),
			new Vector3(-29.7f, 56f, 50f),
			new Vector3(35f, 56f, 50f)
		}
	};

	private void Awake()
	{
		objUp.SetActive(false);
		objDown.SetActive(false);
	}

	public void SetDanGrading(int lv)
	{
		DB.StarRankingActivity.LevelToDanGrading(lv, out var dan, out var grading);
		SetDanGrading(dan, grading);
	}

	public void SetDanGrading(int dan, int grading)
	{
		string danGradingPath = DB.StarRankingActivity.GetDanGradingPath(dan);
		imgBackground.sprite = LoadMgr.LoadSprite(danGradingPath);
		Vector3[] array = _starPositions[grading - 1];
		for (int i = 0; i < imgStar.Length; i++)
		{
			imgStar[i].sprite = LoadMgr.LoadSprite(danGradingPath + "-star");
			if (i < grading)
			{
				imgStar[i].gameObject.SetActive(true);
				imgStar[i].transform.localPosition = array[i];
				imgStar[i].transform.SetSize(array[i].z, array[i].z);
			}
			else
			{
				imgStar[i].gameObject.SetActive(false);
			}
		}
	}

	public void SetUpDownIcon(int value)
	{
		objUp.SetActive(value == 1);
		objDown.SetActive(value == -1);
	}

	public void PlayChangeAni(int lastDan, int lastGrading, int curDan, int curGrading)
	{
		if (!aniCtrl)
		{
			return;
		}
		if (lastDan < curDan)
		{
			imgChange.sprite = LoadMgr.LoadSprite(DB.StarRankingActivity.GetDanGradingPath(curDan));
			aniCtrl.Play("LevelUp1", delegate
			{
				SetDanGrading(curDan, curGrading);
			});
		}
		else if (lastDan > curDan)
		{
			imgChange.sprite = LoadMgr.LoadSprite(DB.StarRankingActivity.GetDanGradingPath(curDan));
			aniCtrl.Play("LevelDown1", delegate
			{
				SetDanGrading(curDan, curGrading);
			});
		}
		else if (curGrading != lastGrading)
		{
			aniCtrl.Play(string.Format("Level{0}TO{1}", lastGrading, curGrading));
		}
	}
}
