using System.Collections.Generic;
using UnityEngine;

public class TurntableView : MonoBehaviour
{
	[SerializeField]
	private Transform itemPrefab;

	[SerializeField]
	private Transform itemParent;

	[SerializeField]
	private List<GameObject> _brightLampObjs;

	private List<TurntableContentItem> _items;

	private TurntableContentItem _lastLightItem;

	public bool isRotating;

	private float _delayTime = 0.5f;

	private int switch_light = 1;

	public void Init(bool isCenter, int areaNumber)
	{
		_items = new List<TurntableContentItem>();
		if (areaNumber > 0)
		{
			float num = isCenter ? 0f : (360f / (float)areaNumber * 0.5f);
			for (int i = 0; i < areaNumber; i++)
			{
				Transform transform = Object.Instantiate(itemPrefab, itemParent);
				float angle = 360f / (float)areaNumber * (float)i - num;
				transform.localEulerAngles = Vector3.back * angle;
				_items.Add(transform.GetComponent<TurntableContentItem>());
			}
		}
		itemPrefab.gameObject.SetActive(false);
		SetLamp(-1);
	}

	public void Refresh(List<TurntableItemInfo> configs)
	{
		if (configs != null)
		{
			for (int i = 0; i < configs.Count; i++)
			{
				_items[i].Init(configs[i]);
			}
		}
	}

	public Vector3 GetPosition(int areaId)
	{
		int index = (areaId <= _items.Count) ? areaId : _items.Count;
		index--;
		if (areaId < 1)
		{
			index = 0;
		}
		return _items[index].iconImg.transform.position;
	}

	private void Update()
	{
		if (_delayTime > 0f)
		{
			_delayTime -= Time.deltaTime;
			return;
		}
		_delayTime = (isRotating ? 0.1f : 0.5f);
		switch_light = -switch_light;
		if (_brightLampObjs != null)
		{
			for (int i = 0; i < _brightLampObjs.Count; i++)
			{
				_brightLampObjs[i].SetActive((i % 3 == 1) != (switch_light != 1));
			}
		}
	}

	public void SetLamp(int areaId)
	{
		switch_light *= areaId;
		if (_brightLampObjs != null)
		{
			for (int i = 0; i < _brightLampObjs.Count; i++)
			{
				_brightLampObjs[i].SetActive((i % 3 == 1) != (switch_light != 1));
			}
		}
	}

	public void SetSelect(int areaId)
	{
		_ = _items.Count;
	}
}
