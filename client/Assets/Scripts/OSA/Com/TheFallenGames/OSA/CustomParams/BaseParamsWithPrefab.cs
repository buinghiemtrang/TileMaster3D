using System;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Com.TheFallenGames.OSA.CustomParams
{
	[Serializable]
	public class BaseParamsWithPrefab : BaseParams
	{
		[FormerlySerializedAs("itemPrefab")]
		[SerializeField]
		private RectTransform _ItemPrefab;

		[Tooltip("Whether to set the BaseParam's ItemTransversalSize to the transversal size of the prefab, like it's done with DefaultItemSize.\nSetting this to true naturally overrides any value you set to ItemTransversalSize.\nSetting it to false, leaves ItemTransversalSize unchanged.\n")]
		[SerializeField]
		private bool _AlsoControlItemTransversalSize;

		private float _ItemPrefabSize = -1f;

		[Obsolete("Use ItemPrefab instead", true)]
		public RectTransform itemPrefab
		{
			get
			{
				return _ItemPrefab;
			}
			set
			{
				_ItemPrefab = value;
			}
		}

		public RectTransform ItemPrefab
		{
			get
			{
				return _ItemPrefab;
			}
			set
			{
				_ItemPrefab = value;
			}
		}

		public bool AlsoControlItemTransversalSize
		{
			get
			{
				return _AlsoControlItemTransversalSize;
			}
			set
			{
				_AlsoControlItemTransversalSize = value;
			}
		}

		public float ItemPrefabSize
		{
			get
			{
				if (!_ItemPrefab)
				{
					throw new OSAException(GetType().ToString() + ": the prefab was not set. Please set it through inspector or in code");
				}
				if (_ItemPrefabSize == -1f)
				{
					if (base.Orientation == OrientationEnum.HORIZONTAL)
					{
						Rect rect = _ItemPrefab.rect;
						_ItemPrefabSize = rect.width;
						if (_AlsoControlItemTransversalSize)
						{
							ItemTransversalSize = rect.height;
							base.ContentPadding.bottom = -1;
							base.ContentPadding.top = -1;
						}
					}
					else
					{
						Rect rect2 = _ItemPrefab.rect;
						_ItemPrefabSize = rect2.height;
						if (_AlsoControlItemTransversalSize)
						{
							ItemTransversalSize = rect2.width;
							base.ContentPadding.right = -1;
							base.ContentPadding.left = -1;
						}
					}
				}
				return _ItemPrefabSize;
			}
		}

		public override void InitIfNeeded(IOSA iAdapter)
		{
			base.InitIfNeeded(iAdapter);
			InitItemPrefab();
		}

		protected void InitItemPrefab()
		{
			RectTransform rectTransform = _ItemPrefab;
			if (_ItemPrefab.parent != base.ScrollViewRT)
			{
				rectTransform = _ItemPrefab.parent as RectTransform;
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
			AssertValidWidthHeight(_ItemPrefab);
			_ItemPrefabSize = -1f;
			DefaultItemSize = ItemPrefabSize;
		}
	}
}
