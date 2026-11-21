using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DotControl : MonoBehaviour
{
	[SerializeField]
	private GameObject _dotPrefab;

	[SerializeField]
	private Transform _dotParent;

	private readonly List<DotItem> _dots = new List<DotItem>(2);

	private int _bannerCount;

	internal void Refresh(int bannerCount, bool isShowOne = false)
	{
		_bannerCount = bannerCount;
		if (bannerCount > 1 || isShowOne)
		{
			if (_dots != null)
			{
				if (bannerCount > 0)
				{
					for (int i = 0; i < bannerCount; i++)
					{
						if (i < _dots.Count)
						{
							if (!_dots[i].gameObject.activeSelf)
							{
								_dots[i].gameObject.SetActive(value: true);
							}
							_dots[i].SetShowState(isShow: false);
						}
						else
						{
							GameObject gameObject = Object.Instantiate(_dotPrefab, _dotParent);
							gameObject.SetActive(value: true);
							DotItem component = gameObject.GetComponent<DotItem>();
							component.SetShowState(isShow: false);
							_dots.Add(component);
						}
					}
					if (bannerCount == 1 && isShowOne)
					{
						_dots[0].gameObject.SetActive(value: true);
						_dots[0].SetShowState(isShow: true);
					}
				}
				for (int num = _dots.Count - 1; num >= bannerCount; num--)
				{
					_dots[num].gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			foreach (DotItem item in _dots.Where((DotItem item) => item.gameObject.activeSelf))
			{
				item.gameObject.SetActive(value: false);
			}
		}
	}

	internal void Show(int index)
	{
		if (_bannerCount > 1 && _dots != null)
		{
			int num = Mathf.Max(0, index);
			if (num < _dots.Count)
			{
				for (int i = 0; i < _bannerCount; i++)
				{
					_dots[i].SetShowState(num == i);
				}
			}
		}
	}
}
