using Com.TheFallenGames.OSA.Core;
using UnityEngine;

namespace Com.TheFallenGames.OSA.CustomAdapters.GridView
{
	public class CellGroupViewsHolder<TCellVH> : BaseItemViewsHolder where TCellVH : CellViewsHolder, new()
	{
		protected int _Capacity = -1;

		protected int _NumActiveCells;

		private RectTransform[] _ContainingCellInstances;

		public override int ItemIndex
		{
			get
			{
				return base.ItemIndex;
			}
			set
			{
				base.ItemIndex = value;
				if (_Capacity > 0)
				{
					OnGroupIndexChanged();
				}
			}
		}

		public int NumActiveCells
		{
			get
			{
				return _NumActiveCells;
			}
			set
			{
				if (_NumActiveCells == value)
				{
					return;
				}
				_NumActiveCells = value;
				for (int i = 0; i < _Capacity; i++)
				{
					TCellVH val = ContainingCellViewsHolders[i];
					GameObject gameObject = val.views.gameObject;
					bool flag = i < _NumActiveCells;
					if (gameObject.activeSelf != flag)
					{
						gameObject.SetActive(flag);
					}
					if (flag == val.rootLayoutElement.ignoreLayout)
					{
						val.rootLayoutElement.ignoreLayout = !flag;
					}
				}
			}
		}

		public TCellVH[] ContainingCellViewsHolders { get; private set; }

		public override void CollectViews()
		{
			base.CollectViews();
			ContainingCellViewsHolders = new TCellVH[_Capacity];
			for (int i = 0; i < _Capacity; i++)
			{
				ContainingCellViewsHolders[i] = new TCellVH();
				ContainingCellViewsHolders[i].InitWithExistingRootPrefab(_ContainingCellInstances[i]);
				ContainingCellViewsHolders[i].views.gameObject.SetActive(false);
			}
			if (ItemIndex != -1 && _Capacity > 0)
			{
				UpdateIndicesOfContainingCells();
			}
		}

		internal void Init(GameObject groupPrefab, RectTransform parent, int itemIndex, RectTransform cellPrefab, int numCellsPerGroup)
		{
			Init(groupPrefab, parent, itemIndex, true, false);
			_Capacity = numCellsPerGroup;
			Transform transform = groupPrefab.transform;
			_ContainingCellInstances = new RectTransform[_Capacity];
			for (int i = 0; i < _Capacity; i++)
			{
				RectTransform rectTransform = Object.Instantiate(cellPrefab.gameObject, transform, false).transform as RectTransform;
				rectTransform.gameObject.SetActive(true);
				rectTransform.SetParent(root);
				_ContainingCellInstances[i] = rectTransform;
			}
			CollectViews();
		}

		public override void OnBeforeDestroy()
		{
			if (ContainingCellViewsHolders != null)
			{
				for (int i = 0; i < ContainingCellViewsHolders.Length; i++)
				{
					if (ContainingCellViewsHolders[i] != null)
					{
						ContainingCellViewsHolders[i].OnBeforeDestroy();
					}
				}
			}
		}

		protected virtual void OnGroupIndexChanged()
		{
			if (_Capacity > 0)
			{
				UpdateIndicesOfContainingCells();
			}
		}

		protected virtual void UpdateIndicesOfContainingCells()
		{
			for (int i = 0; i < _Capacity; i++)
			{
				ContainingCellViewsHolders[i].ItemIndex = i + _Capacity * ItemIndex;
			}
		}
	}
}
