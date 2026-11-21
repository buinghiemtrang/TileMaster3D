using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PigAnimDialog : MonoBehaviour
{
	public GameObject animObj;

	private List<FlyItem> stars = new List<FlyItem>();

	private int oldGoldNum;

	private int addGoldNum;

	private void Start()
	{
		ShowPigAnim();
	}

	public void Show(int oldNum, int addNum)
	{
		oldGoldNum = oldNum;
		addGoldNum = addNum;
	}

	private void ShowPigAnim()
	{
		if ((bool)animObj)
		{
			animObj.SetActive(true);
			Invoke("ShowFlyAnim", 1f);
		}
	}

	private void ShowFlyAnim()
	{
		StartCoroutine(IETimeStar(delegate
		{
			DB.Bag.ModifyShow(PropID.Money, oldGoldNum, addGoldNum);
			LoginMgr.I.Cost();
			UnityEngine.Object.Destroy(base.gameObject);
		}));
	}

	private IEnumerator IETimeStar(Action cback = null)
	{
		int animNum = MMath.Random(5, 8);
		int num = animNum;
		Vector2 sPos = base.transform.localPosition;
		Vector2 ePos = base.transform.InverseTransformPoint(SceneMain.ins.goldIcon.position);
		Action onComplete = delegate
		{
			if (animNum < 2)
			{
				if (cback != null)
				{
					cback();
				}
			}
			else
			{
				animNum--;
			}
		};
		while (num > 0)
		{
			FlyItem flyItem = stars.FirstOrDefault((FlyItem fd) => fd.isFree);
			if (!flyItem)
			{
				flyItem = LoadMgr.LoadRes<GameObject>("Effect/Prop").GetComponent<FlyItem>();
				stars.Add(flyItem);
			}
			flyItem.Fly2D(base.transform, new Vector3(sPos.x, sPos.y, 0f), ePos, onComplete);
			num--;
			yield return new WaitForSeconds(0.05f);
		}
	}
}
