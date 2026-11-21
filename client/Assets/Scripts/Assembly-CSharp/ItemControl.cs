using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemControl : MonoBehaviour
{
	[SerializeField]
	private bool _isSingle;

	[SerializeField]
	private bool _isNoShowIconItemCountLessThanTwo;

	[SerializeField]
	private GameObject _decorateRoot;

	[SerializeField]
	private GameObject _goldCoinRoot;

	[SerializeField]
	private Text _goldCoinValueTex;

	[SerializeField]
	private GameObject _item0Root;

	[SerializeField]
	private Transform _item0Parent;

	[SerializeField]
	private List<ItemPositionSize> _item0PositionSizes;

	[SerializeField]
	private GameObject _item1Root;

	[SerializeField]
	private Transform _item1Parent;

	[SerializeField]
	private List<ItemPositionSize> _item1PositionSizes;

	[SerializeField]
	private GameObject _item2Root;

	[SerializeField]
	private Transform _item2Parent;

	[SerializeField]
	private List<ItemPositionSize> _item2PositionSizes;

	[SerializeField]
	private GameObject _item3Root;

	[SerializeField]
	private Transform _item3Parent;

	[SerializeField]
	private List<ItemPositionSize> _item3PositionSizes;

	public void InitData(string item, string itemOther)
	{
		Dictionary<int, int> rewardsOne = item.ToDictionary();
		if (!_isSingle)
		{
			Dictionary<int, int> rewardsTwo = itemOther.ToDictionary();
			if (!_isNoShowIconItemCountLessThanTwo || (rewardsOne.Count > 2 && rewardsTwo.Count != 0))
			{
				SetGoldCoin(rewardsOne);
				foreach (int item2 in rewardsOne.Keys.ToList().Where((int propId) => rewardsTwo.ContainsKey(propId)))
				{
					rewardsOne.Remove(item2);
				}
				if (rewardsOne.Count != 0 && rewardsTwo.Count != 0)
				{
					if ((bool)_item0Root)
					{
						_item0Root.SetActive(value: false);
					}
					_item1Root.SetActive(value: false);
					AddItem(rewardsOne, _item2Root, _item2PositionSizes, _item2Parent);
					AddItem(rewardsTwo, _item3Root, _item3PositionSizes, _item3Parent);
					return;
				}
				if ((bool)_item0Root)
				{
					_item0Root.SetActive(value: false);
				}
				_item2Root.SetActive(value: false);
				_item3Root.SetActive(value: false);
				AddItem((rewardsTwo.Count != 0) ? rewardsTwo : rewardsOne, _item1Root, _item1PositionSizes, _item1Parent);
				return;
			}
			_decorateRoot.SetActive(value: false);
			_goldCoinRoot.SetActive(value: false);
			_item1Root.SetActive(value: false);
			_item2Root.SetActive(value: false);
			_item3Root.SetActive(value: false);
		}
		AddItem(rewardsOne, _item0Root, _item0PositionSizes, _item0Parent);
	}

	private void SetGoldCoin(IDictionary<int, int> rewardsOne)
	{
		int num = 0;
		bool flag = false;
		foreach (KeyValuePair<int, int> item in rewardsOne)
		{
			item.Deconstruct(out var key, out var value);
			if (key == 0)
			{
				flag = true;
				num = value;
				break;
			}
		}
		rewardsOne.Remove(0);
		_goldCoinRoot.SetActive(flag);
		if (_goldCoinRoot.activeSelf)
		{
			_goldCoinValueTex.text = num.ToString();
		}
		_decorateRoot.SetActive(!_goldCoinRoot.activeSelf);
	}

	private void AddItem(Dictionary<int, int> rewards, GameObject root, IEnumerable<ItemPositionSize> itemPositionSizes, Transform parent)
	{
		int count = rewards.Count;
		root.SetActive(count > 0);
		if (!root.activeSelf)
		{
			return;
		}
		ItemPositionSize itemPositionSize = itemPositionSizes.FirstOrDefault((ItemPositionSize fd) => fd.Count == count);
		if (itemPositionSize == null)
		{
			return;
		}
		List<GiftPopItem> list = parent.transform.Cast<Transform>().Select((Transform item) => item.GetComponent<GiftPopItem>()).ToList();
		foreach (GiftPopItem item2 in list)
		{
			Object.Destroy(item2.gameObject);
		}
		list.Clear();
		int num = 0;
		foreach (KeyValuePair<int, int> reward in rewards)
		{
			reward.Deconstruct(out var key, out var value);
			GiftPopItem giftPopItem = ((num < list.Count) ? list[num] : Object.Instantiate(itemPositionSize.Prefab, parent));
			giftPopItem.Root.SetActive(value: true);
			giftPopItem.Init(key, value);
			giftPopItem.Root.transform.localPosition = itemPositionSize.Positions[num];
			num++;
		}
		for (; num < list.Count; num++)
		{
			list[num].Root.SetActive(value: false);
		}
	}
}
