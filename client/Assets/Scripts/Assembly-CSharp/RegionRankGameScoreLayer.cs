using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameScene_Element;
using GameScene_Level;
using UnityEngine;
using UnityEngine.UI;

public class RegionRankGameScoreLayer : MonoBehaviour
{
	[SerializeField]
	private Text scoreTxt;

	[SerializeField]
	private Transform crownIcon;

	private List<FlyItem> _stars;

	private const string StartIconPath = "Effect/crown_icon";

	private void Awake()
	{
		_stars = new List<FlyItem>();
	}

	public void Init()
	{
		if (LevelMgr.Data.IsRegionRankLevel)
		{
			base.gameObject.SetActive(true);
			SetScore();
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}

	public void AddScore(List<Element> elements, Vector3 startPosition)
	{
		if (!LevelMgr.Data.IsRegionRankLevel)
		{
			return;
		}
		bool flag = startPosition == Vector3.zero;
		int num = 0;
		for (int i = 0; i < elements.Count; i++)
		{
			if (elements[i].Type == ElementType.RegionRankCollect)
			{
				if (flag)
				{
					LevelMgr.Data.RegionRankScore++;
				}
				num++;
			}
		}
		if (flag)
		{
			SetScore();
		}
		else
		{
			StartCoroutine(IEStar(num, startPosition));
		}
	}

	private void SetScore()
	{
		scoreTxt.text = "x" + LevelMgr.Data.RegionRankScore;
	}

	private IEnumerator IEStar(int number, Vector3 startPosition)
	{
		while (number > 0)
		{
			FlyItem flyItem = _stars.FirstOrDefault((FlyItem fd) => fd.isFree);
			if (!flyItem)
			{
				flyItem = LoadMgr.LoadRes<GameObject>("Effect/crown_icon").GetComponent<FlyItem>();
				_stars.Add(flyItem);
			}
			flyItem.Fly(ElementsMgr.I.starNode, startPosition, crownIcon.position, delegate
			{
				LevelMgr.Data.RegionRankScore++;
				SetScore();
			});
			number--;
			yield return new WaitForSeconds(0.05f);
		}
	}
}
