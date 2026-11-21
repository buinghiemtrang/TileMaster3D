using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MHorizontalLayout : MonoBehaviour
{
	public float spacing;

	public Action func_refresh;

	private bool bRefresh;

	private List<Transform> items = new List<Transform>();

	private void Awake()
	{
		if (base.transform.childCount >= 1 && !bRefresh)
		{
			StartCoroutine(RefreshItems());
		}
	}

	public void Refresh()
	{
		if (!bRefresh)
		{
			StartCoroutine(RefreshItems());
		}
	}

	private IEnumerator RefreshItems()
	{
		bRefresh = true;
		yield return 0;
		bRefresh = false;
		items.Clear();
		foreach (Transform item in base.transform)
		{
			if (item.gameObject.activeSelf)
			{
				items.Add(item);
			}
		}
		float num = 0f;
		for (int i = 0; i < items.Count; i++)
		{
			num += items[i].Rect().width + spacing;
		}
		num = (num - spacing) * -0.5f;
		for (int j = 0; j < items.Count; j++)
		{
			float width = items[j].Rect().width;
			items[j].SetLocPosX(num + width * 0.5f);
			num += width + spacing;
		}
		func_refresh?.Invoke();
	}
}
