using Com.TheFallenGames.OSA.Core;
using UnityEngine;

namespace Com.TheFallenGames.OSA.AdditionalComponents
{
	public class OSAPercentageContentPaddingSetter : MonoBehaviour
	{
		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("0 = none, .5f = half of the viewport, 1f = the entire viewport's size will be used for padding")]
		private float _PaddingStartPercent = 0.5f;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("Same rules as for PaddingStartPercent")]
		private float _PaddingEndPercent = 0.5f;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("After setting the padding, how much will this item approach the viewport's edge based on its size?. 0=none, i.e. full padding. 1=a distance equal to its size. \nFor example, a 0.5 value could be used along with PaddingStartPercent and PaddingEndPercent also set to 0.5, resulting in first/last items arriving exactly in the middle when you scroll in the extremities.\nItemSizeSource must also be set for this to be accurate. Otherwise, OSA.Parameters.DefaultItemSize will be used")]
		private float _FirstLastItemsInsidePercent = 0.5f;

		[SerializeField]
		[Tooltip("This object's width or height will be used to calculate the most accurate position to satisfy the FirstLastItemsInsidePercent property")]
		private RectTransform _ItemSizeCustomSource;

		private IOSA _IOSA;

		private bool _UseDefaultItemSize;

		private bool _IsHorizontal;

		private void Awake()
		{
			base.enabled = false;
			_IOSA = GetComponent(typeof(IOSA)) as IOSA;
			if (_IOSA == null)
			{
				Debug.Log(typeof(OSAPercentageContentPaddingSetter).Name + " needs to be attached to a game object containing an OSA component");
				return;
			}
			if (_IOSA.IsInitialized)
			{
				Debug.Log(typeof(OSAPercentageContentPaddingSetter).Name + " needs the OSA component to not be initialized before it");
				return;
			}
			BaseParams baseParameters = _IOSA.BaseParameters;
			_UseDefaultItemSize = _ItemSizeCustomSource == null;
			_IsHorizontal = _IOSA.BaseParameters.Orientation == BaseParams.OrientationEnum.HORIZONTAL;
			_IOSA.ScrollViewSizeChanged += OnScrollViewSizeChanged;
			baseParameters.PrepareForInit(firstTime: true);
			baseParameters.InitIfNeeded(_IOSA);
			UpdatePadding();
		}

		private void OnScrollViewSizeChanged()
		{
			if (!_IOSA.IsInitialized)
			{
				Debug.LogError(typeof(OSAPercentageContentPaddingSetter).Name + ".OnScrollViewSizeChanged() called, but OSA not initialized. This shouldn't happen if implemented correctly");
				return;
			}
			_IOSA.BaseParameters.PrepareForInit(firstTime: false);
			_IOSA.BaseParameters.InitIfNeeded(_IOSA);
			UpdatePadding();
		}

		private void UpdatePadding()
		{
			if (_UseDefaultItemSize)
			{
				SetPaddingFromDefaultItemSize();
			}
			else
			{
				SetPaddingFromCustomItemSource();
			}
		}

		private void SetPaddingFromCustomItemSource()
		{
			float vPSize = GetVPSize();
			float sourceItemSize = GetSourceItemSize();
			SetPaddingWith(vPSize, sourceItemSize);
		}

		private void SetPaddingFromDefaultItemSize()
		{
			float vPSize = GetVPSize();
			float defaultItemSize = _IOSA.BaseParameters.DefaultItemSize;
			SetPaddingWith(vPSize, defaultItemSize);
		}

		private void SetPaddingWith(float vpSize, float itemSizeToUse)
		{
			BaseParams baseParameters = _IOSA.BaseParameters;
			RectOffset contentPadding = baseParameters.ContentPadding;
			float num = _FirstLastItemsInsidePercent * itemSizeToUse;
			int num2 = (int)(_PaddingStartPercent * vpSize - num + 0.5f);
			int num3 = (int)(_PaddingEndPercent * vpSize - num + 0.5f);
			if (baseParameters.Orientation == BaseParams.OrientationEnum.HORIZONTAL)
			{
				contentPadding.left = num2;
				contentPadding.right = num3;
			}
			else
			{
				contentPadding.top = num2;
				contentPadding.bottom = num3;
			}
		}

		private float GetSourceItemSize()
		{
			Rect rect = _ItemSizeCustomSource.rect;
			if (!_IsHorizontal)
			{
				return rect.height;
			}
			return rect.width;
		}

		private float GetVPSize()
		{
			Rect rect = _IOSA.BaseParameters.Viewport.rect;
			if (!_IsHorizontal)
			{
				return rect.height;
			}
			return rect.width;
		}
	}
}
