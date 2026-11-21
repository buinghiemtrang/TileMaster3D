using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIGridView : ScrollRect
{
	[Serializable]
	public class ScrollEndedEvent : UnityEvent
	{
	}

	private enum Axis
	{
		Horizontal = 0,
		Vertical = 1
	}

	public enum ItemAdpter
	{
		FixedSize = 0,
		FixedCount = 1
	}

	[SerializeField]
	private ScrollEndedEvent _onScrollToBottom;

	[SerializeField]
	private ItemAdpter _itemAdapter;

	[SerializeField]
	private Axis _startAxis;

	[SerializeField]
	private int _constraintCount;

	[SerializeField]
	private bool _flipY;

	[SerializeField]
	private Vector2 _itemSize = new Vector2(50f, 50f);

	[SerializeField]
	private Vector2 _spacing = Vector2.zero;

	[SerializeField]
	private RectOffset _padding;

	private UIGridViewController _controller;

	public float ContentWidth;

	public float ContentHeight;

	private Queue<RectTransform> _itemQueue = new Queue<RectTransform>();

	private RectTransform[] _visibleItems;

	private GameObject _prefabCache;

	private int _itemCountCache;

	private int _constraintCountCache;

	private Vector2 _itemSizeCache;

	private int _centerLineIndex = -1;

	private int _maxVisibleLineCount;

	private int _maxLineCount;

	private bool _checkToBottom;

	private Vector2 _lastMoveVec1;

	private Vector2 _lastMoveVec2;

	private float _diffDistanceToBottom = 10f;

	private float _diffDistanceToTop = 10f;

	private Vector2 _lastContentSize;

	private Vector2 _pivot;

	private float _signY;

	private int _startIndex = -1;

	private int _endIndex = -1;

	private Coroutine _loadCoroutine;

	private readonly Queue<int> _items2Load = new Queue<int>();

	public ScrollEndedEvent OnScrollToBottom => _onScrollToBottom;

	public Vector2 ItemSize => _itemSize;

	public RectOffset Padding => _padding;

	public UIGridViewController Controller
	{
		get
		{
			return _controller;
		}
		set
		{
			if (_controller != value)
			{
				_controller = value;
				_prefabCache = _controller.GetItemPrefab();
				StackAllVisibleItems();
				while (_itemQueue.Count > 0)
				{
					UnityEngine.Object.Destroy(_itemQueue.Dequeue());
				}
			}
		}
	}

	public Vector2 Spacing => _spacing;

	private void ParamCalculation()
	{
		Rect rect = base.viewport.rect;
		float num = rect.width - (float)(_padding.right + _padding.left);
		Rect rect2 = base.viewport.rect;
		float num2 = rect2.height - (float)(_padding.bottom + _padding.top);
		if (_itemAdapter == ItemAdpter.FixedSize)
		{
			float num3;
			float num4;
			if (_startAxis == Axis.Vertical)
			{
				float num5 = 0f - _spacing.y;
				num3 = num5 - num2;
				num4 = num5 - _itemSize.y;
			}
			else
			{
				if (_startAxis != Axis.Horizontal)
				{
					return;
				}
				float num6 = 0f - _spacing.x;
				num3 = num6 - num;
				num4 = num6 - _itemSize.x;
			}
			_constraintCount = (int)(num3 / num4);
		}
		else if (_itemAdapter == ItemAdpter.FixedCount)
		{
			float x;
			float y;
			if (_startAxis == Axis.Vertical)
			{
				y = (num2 - _spacing.y * (float)(_constraintCount - 1)) / (float)_constraintCount;
				x = _itemSize.x;
			}
			else
			{
				y = 0f;
				x = 0f;
				if (_startAxis == Axis.Horizontal)
				{
					x = (num - _spacing.x * (float)(_constraintCount - 1)) / (float)_constraintCount;
					y = _itemSize.y;
				}
			}
			_itemSize.x = x;
			_itemSize.y = y;
		}
	}

	public void ReloadContent(bool resetPosition = false)
	{
		if (_controller == null)
		{
			return;
		}
		ParamCalculation();
		if (resetPosition)
		{
			if (base.vertical)
			{
				base.verticalNormalizedPosition = 1f;
			}
			if (base.horizontal)
			{
				base.horizontalNormalizedPosition = 0f;
			}
		}
		if (_itemCountCache != _controller.GetItemCount() || _constraintCountCache != _constraintCount || _itemSizeCache != _itemSize)
		{
			_itemCountCache = _controller.GetItemCount();
			_constraintCountCache = _constraintCount;
			_itemSizeCache = _itemSize;
			_maxLineCount = (int)Math.Ceiling((float)_itemCountCache / (float)_constraintCount);
			UpdateContentSize();
			StackAllVisibleItems();
			_visibleItems = new RectTransform[_itemCountCache];
		}
		else
		{
			StackAllVisibleItems();
		}
		UpdateItems(forceUpdate: true);
		foreach (RectTransform item in _itemQueue)
		{
			item.gameObject.SetActive(value: false);
		}
	}

	private void DestroyAllItems()
	{
		StackAllVisibleItems();
		while (_itemQueue.Count > 0)
		{
			UnityEngine.Object.Destroy(_itemQueue.Dequeue());
		}
	}

	private void StackAllVisibleItems()
	{
		if (_visibleItems == null)
		{
			return;
		}
		for (int i = 0; i < _visibleItems.Length; i++)
		{
			if (_visibleItems[i] != null)
			{
				_itemQueue.Enqueue(_visibleItems[i]);
				_visibleItems[i] = null;
			}
		}
	}

	private void UpdateContentSize()
	{
		float num = _padding.left + _padding.right;
		float num2 = _padding.top + _padding.bottom;
		if (_startAxis == Axis.Vertical)
		{
			num = _itemSize.x * (float)_maxLineCount + _spacing.x * (float)(_maxLineCount - 1) + num;
			num2 = _itemSize.y * (float)_constraintCount + _spacing.y * (float)(_constraintCount - 1) + num2;
		}
		else if (_startAxis == Axis.Horizontal)
		{
			num = _itemSize.x * (float)_constraintCount + _spacing.x * (float)(_constraintCount - 1) + num;
			num2 = _itemSize.y * (float)_maxLineCount + _spacing.y * (float)(_maxLineCount - 1) + num2;
		}
		base.content.sizeDelta = new Vector2(num, num2);
		ContentWidth = num;
		ContentHeight = num2;
	}

	private IEnumerator LoadItem(int index)
	{
		RectTransform rectTransform;
		GameObject gameObject;
		if (_itemQueue.Count == 0)
		{
			gameObject = UnityEngine.Object.Instantiate(_prefabCache, base.content);
			rectTransform = gameObject.GetComponent<RectTransform>();
		}
		else
		{
			rectTransform = _itemQueue.Dequeue();
			gameObject = rectTransform.gameObject;
		}
		rectTransform.anchorMin = _pivot;
		rectTransform.anchorMax = _pivot;
		rectTransform.pivot = _pivot;
		rectTransform.sizeDelta = _itemSize;
		_visibleItems[index] = rectTransform;
		rectTransform.gameObject.SetActive(value: true);
		UpdateItemPosition(rectTransform, index);
		yield return _controller.OnItemShown(gameObject, index);
	}

	private IEnumerator LoadItems()
	{
		while (_items2Load.Count > 0)
		{
			int num = _items2Load.Dequeue();
			if (_startIndex <= num && num <= _endIndex && !(_visibleItems[num] != null))
			{
				yield return LoadItem(num);
			}
		}
	}

	private void UpdateItems(bool forceUpdate = false)
	{
		int num = GetCenterLineIndex();
		if (_centerLineIndex == num && !forceUpdate)
		{
			return;
		}
		int num2 = (int)((float)_maxVisibleLineCount * 0.5f);
		int num3 = Math.Max(0, _constraintCount * (num - num2));
		int num4 = Math.Min(_itemCountCache - 1, _constraintCount + _constraintCount * (num + num2) - 1);
		_startIndex = num3;
		_endIndex = num4;
		for (int i = 0; i < _itemCountCache; i++)
		{
			if (!(_visibleItems[i] == null) && (i < num3 || i > num4))
			{
				_itemQueue.Enqueue(_visibleItems[i]);
				_visibleItems[i] = null;
			}
		}
		for (int j = num3; j <= num4; j++)
		{
			_items2Load.Enqueue(j);
		}
		if (base.gameObject.activeSelf && _items2Load.Count > 0)
		{
			_loadCoroutine = StartCoroutine(LoadItems());
		}
	}

	public Vector2 GetItemPosition(int index)
	{
		int num;
		int num2;
		if (_startAxis == Axis.Vertical)
		{
			num = index / _constraintCount;
			num2 = index - num * _constraintCount;
		}
		else if (_startAxis == Axis.Horizontal)
		{
			num2 = index / _constraintCount;
			num = index - num2 * _constraintCount;
		}
		else
		{
			num = 0;
			num2 = 0;
		}
		Vector2 result = default(Vector2);
		result.x = (_itemSize.x + _spacing.x) * (float)num + (float)_padding.left;
		result.y = _signY * ((_spacing.y + _itemSize.y) * (float)num2 + (float)_padding.top);
		return result;
	}

	private void UpdateItemPosition(RectTransform rt, int index)
	{
		rt.anchoredPosition = GetItemPosition(index);
	}

	private int GetCenterLineIndex()
	{
		RectTransform viewport = base.viewport;
		Rect rect = viewport.rect;
		Vector3 position = viewport.TransformPoint(new Vector3(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f, 0f));
		Vector3 vector = base.content.InverseTransformPoint(new Vector3(position.x, position.y, 0f));
		float num;
		float num2;
		if (_startAxis == Axis.Vertical)
		{
			num2 = _itemSize.x + _spacing.x;
			num = vector.x - (float)_padding.left;
		}
		else
		{
			if (_startAxis != Axis.Horizontal)
			{
				return 0;
			}
			num2 = _itemSize.y + _spacing.y;
			num = _signY * (vector.y + (float)_padding.top);
		}
		return (int)(num / num2);
	}

	protected override void Awake()
	{
		base.Awake();
		_pivot = (_flipY ? Vector2.zero : Vector2.up);
		_signY = (_flipY ? 1f : (-1f));
		base.onValueChanged.AddListener(OnScrollChanged);
		base.content.anchorMax = _pivot;
		base.content.anchorMin = _pivot;
		base.content.pivot = _pivot;
		base.content.anchoredPosition = Vector2.zero;
	}

	protected override void Start()
	{
		if (_startAxis == Axis.Vertical)
		{
			float num = base.viewport.rect.width / (_itemSize.x + _spacing.x);
			base.horizontal = true;
			base.vertical = false;
			_maxVisibleLineCount = (int)num;
		}
		else if (_startAxis == Axis.Horizontal)
		{
			float num2 = base.viewport.rect.height / (_itemSize.y + _spacing.y);
			base.horizontal = false;
			base.vertical = true;
			_maxVisibleLineCount = (int)num2;
		}
		base.Start();
		ReloadContent();
		CheckToBottom(Vector2.zero);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_loadCoroutine != null)
		{
			StopCoroutine(_loadCoroutine);
			_items2Load.Clear();
		}
	}

	private void OnScrollChanged(Vector2 delta)
	{
		UpdateItems();
		CheckToBottom(delta);
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		_checkToBottom = true;
		base.OnBeginDrag(eventData);
	}

	private void CheckToBottom(Vector2 vector2)
	{
		if (_lastContentSize != base.content.rect.size)
		{
			_checkToBottom = true;
			_lastContentSize = base.content.rect.size;
		}
		float num;
		float num2;
		if (_startAxis == Axis.Vertical)
		{
			num2 = 1f / base.content.rect.width * _diffDistanceToBottom;
			num = base.horizontalNormalizedPosition;
		}
		else
		{
			num = 0f;
			num2 = 0f;
			if (_startAxis == Axis.Horizontal)
			{
				num2 = 1f / base.content.rect.height * _diffDistanceToBottom;
				num = base.verticalNormalizedPosition;
			}
		}
		if (_checkToBottom && vector2.y - _lastMoveVec1.y < 0f && num <= num2)
		{
			_onScrollToBottom.Invoke();
			_checkToBottom = false;
		}
	}

	public bool IsStickBottom()
	{
		if (base.verticalNormalizedPosition <= 0f)
		{
			return true;
		}
		return base.content.rect.height < base.viewport.rect.height;
	}

	public bool IsStickTop()
	{
		if (1f <= base.verticalNormalizedPosition)
		{
			return true;
		}
		return base.content.rect.height < base.viewport.rect.height;
	}
}
