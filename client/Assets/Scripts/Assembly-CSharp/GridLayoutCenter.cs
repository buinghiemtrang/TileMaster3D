using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLayoutCenter : MonoBehaviour
{
	public Vector2 spacing;

	[HideInInspector]
	public List<Transform> items;

	private bool _bRefresh;

	private Transform _tfContent;

	private Action _completeCallback;

	public void SetCompleteCallBack(Action callback)
	{
		_completeCallback = callback;
	}

	private void Awake()
	{
		_tfContent = base.transform.Find("Content");
		if (_tfContent == null)
		{
			_tfContent = base.transform;
		}
		if (_tfContent.childCount < 1)
		{
			return;
		}
		foreach (Transform item in _tfContent)
		{
			items.Add(item);
		}
		if (items.Count > 0)
		{
			StartCoroutine(RefreshItems());
		}
	}

	private void OnDestroy()
	{
		StopCoroutine(RefreshItems());
	}

	public void Clear()
	{
		foreach (Transform item in items)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		items.Clear();
	}

	public void AddItem(Transform tf)
	{
		items.Add(tf);
		tf.SetParent(_tfContent, worldPositionStays: false);
		if (!_bRefresh)
		{
			_bRefresh = true;
			StartCoroutine(RefreshItems());
		}
	}

	public void SetWidth(int maxColumn)
	{
		base.transform.SetWidth(items[0].Rect().width * (float)maxColumn + spacing.x * (float)(maxColumn - 1));
	}

	private IEnumerator RefreshItems()
	{
		yield return 0;
		int count = items.Count;
		Rect rect = base.transform.Rect();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		int num4 = 0;
		for (int i = 0; i < count; i++)
		{
			Transform transform = items[i];
			float num5 = transform.Rect().width + spacing.x;
			float b = transform.Rect().height + spacing.y;
			float num6 = num + num5;
			num3 = Mathf.Max(num3, b);
			if (rect.width + spacing.x < num6)
			{
				if (num4 < i)
				{
					num = (num - spacing.x) * -0.5f;
					do
					{
						Transform transform2 = items[num4];
						transform2.SetAnchorPos(num, num2 + spacing.y * -0.5f);
						num += transform2.Rect().width + spacing.x;
						num4++;
					}
					while (i != num4);
				}
				num2 -= num3;
				num4 = i;
				num6 = num5;
			}
			num = num6;
		}
		num = ((count < 1) ? (-0f) : (num * -0.5f));
		if (num4 < count)
		{
			float num7 = 0f;
			do
			{
				Transform transform3 = items[num4];
				transform3.SetAnchorPos(num + spacing.x * 0.5f, num2 - spacing.y * 0.5f);
				num += transform3.Rect().width + spacing.x;
				num4++;
				num7 = Mathf.Max(num7, transform3.Rect().height + spacing.y);
			}
			while (count != num4);
			num2 -= num7;
		}
		base.transform.SetHeight(0f - num2);
		_bRefresh = false;
		_completeCallback.InvokeSafe();
	}
}
