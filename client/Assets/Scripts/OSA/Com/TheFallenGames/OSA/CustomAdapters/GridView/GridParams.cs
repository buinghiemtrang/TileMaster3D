using System;
using Com.TheFallenGames.OSA.Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Com.TheFallenGames.OSA.CustomAdapters.GridView
{
	[Serializable]
	public class GridParams : BaseParams
	{
		[Serializable]
		public class GridConfig
		{
			[SerializeField]
			[FormerlySerializedAs("numCellsPerGroup")]
			[Tooltip("The max. number of cells in a row group (for vertical ScrollView) or column group (for horizontal ScrollView).\nSet to -1 to fill with cells when there's space - note that it only works when cell's flexibleWidth is not used (flexibleHeight for horizontal scroll views).\n\nSet further to negative values to force more items than the space allows. For example, -2 sets 2x more items than the space allows. This may be useful for advanced scenarios where you use a custom LayoutGroup for the cell group")]
			private int _MaxCellsPerGroup;

			[SerializeField]
			[Tooltip("If true, the DefaultItemSize property will be used for the height of a row, if using a vertical scroll view (the width of a column, otherwise). Leave to false to automatically infer based on the cellPrefab's LayoutElement's values")]
			[FormerlySerializedAs("_UseDefaulfItemSizeForCellGroupSize")]
			private bool _UseDefaultItemSizeForCellGroupSize;

			[FormerlySerializedAs("cellPrefab")]
			[SerializeField]
			private RectTransform _CellPrefab;

			[Tooltip("The alignment of cells inside their parent LayoutGroup (Vertical or Horizontal, depending on ScrollView's orientation)")]
			[FormerlySerializedAs("alignmentOfCellsInGroup")]
			[SerializeField]
			private TextAnchor _AlignmentOfCellsInGroup;

			[Tooltip("The spacing between the cells of a group. Leave it to -1 to use the same value as contentSpacing (i.e. the spacing between the groups), so vertical and horizontal spacing will be the same")]
			[FormerlySerializedAs("spacingInGroup")]
			[SerializeField]
			private float _SpacingInGroup;

			[FormerlySerializedAs("groupPadding")]
			[SerializeField]
			private RectOffset _GroupPadding;

			[FormerlySerializedAs("cellWidthForceExpandInGroup")]
			[SerializeField]
			private bool _CellWidthForceExpandInGroup;

			[FormerlySerializedAs("cellHeightForceExpandInGroup")]
			[SerializeField]
			private bool _CellHeightForceExpandInGroup;

			public int MaxCellsPerGroup
			{
				get
				{
					return _MaxCellsPerGroup;
				}
				set
				{
					_MaxCellsPerGroup = value;
				}
			}

			public bool UseDefaultItemSizeForCellGroupSize
			{
				get
				{
					return _UseDefaultItemSizeForCellGroupSize;
				}
				protected set
				{
					_UseDefaultItemSizeForCellGroupSize = value;
				}
			}

			[Obsolete("Use CellPrefab instead", true)]
			public RectTransform cellPrefab
			{
				get
				{
					return _CellPrefab;
				}
				set
				{
					_CellPrefab = value;
				}
			}

			public RectTransform CellPrefab
			{
				get
				{
					return _CellPrefab;
				}
				set
				{
					_CellPrefab = value;
				}
			}

			[Obsolete("Use AlignmentOfCellsInGroup instead", true)]
			public TextAnchor alignmentOfCellsInGroup
			{
				get
				{
					return _AlignmentOfCellsInGroup;
				}
				set
				{
					_AlignmentOfCellsInGroup = value;
				}
			}

			public TextAnchor AlignmentOfCellsInGroup
			{
				get
				{
					return _AlignmentOfCellsInGroup;
				}
				set
				{
					_AlignmentOfCellsInGroup = value;
				}
			}

			[Obsolete("Use SpacingInGroup instead", true)]
			public float spacingInGroup
			{
				get
				{
					return _SpacingInGroup;
				}
				set
				{
					_SpacingInGroup = value;
				}
			}

			public float SpacingInGroup
			{
				get
				{
					return _SpacingInGroup;
				}
				set
				{
					_SpacingInGroup = value;
				}
			}

			[Obsolete("Use GroupPadding instead", true)]
			public RectOffset groupPadding
			{
				get
				{
					return _GroupPadding;
				}
				set
				{
					_GroupPadding = value;
				}
			}

			public RectOffset GroupPadding
			{
				get
				{
					return _GroupPadding;
				}
				set
				{
					_GroupPadding = value;
				}
			}

			[Obsolete("Use CellWidthForceExpandInGroup instead", true)]
			public bool cellWidthForceExpandInGroup
			{
				get
				{
					return _CellWidthForceExpandInGroup;
				}
				set
				{
					_CellWidthForceExpandInGroup = value;
				}
			}

			public bool CellWidthForceExpandInGroup
			{
				get
				{
					return _CellWidthForceExpandInGroup;
				}
				set
				{
					_CellWidthForceExpandInGroup = value;
				}
			}

			[Obsolete("Use CellHeightForceExpandInGroup instead", true)]
			public bool cellHeightForceExpandInGroup
			{
				get
				{
					return _CellHeightForceExpandInGroup;
				}
				set
				{
					_CellHeightForceExpandInGroup = value;
				}
			}

			public bool CellHeightForceExpandInGroup
			{
				get
				{
					return _CellHeightForceExpandInGroup;
				}
				set
				{
					_CellHeightForceExpandInGroup = value;
				}
			}
		}

		[FormerlySerializedAs("grid")]
		[SerializeField]
		private GridConfig _Grid;

		private LayoutGroup _TheOnlyGroupPrefab;

		private HorizontalOrVerticalLayoutGroup _TheOnlyGroupPrefabAsHorizontalOrVertical;

		private int _CurrentUsedNumCellsPerGroup;

		private LayoutElement _CellPrefabLayoutElement;

		[Obsolete("Use Grid instead", true)]
		public GridConfig grid
		{
			get
			{
				return _Grid;
			}
			set
			{
				_Grid = value;
			}
		}

		public GridConfig Grid
		{
			get
			{
				return _Grid;
			}
			set
			{
				_Grid = value;
			}
		}

		public int CurrentUsedNumCellsPerGroup => 0;

		public LayoutElement CellPrefabLayoutElement => null;

		public override void InitIfNeeded(IOSA iAdapter)
		{
		}

		public virtual int CalculateCurrentNumCellsPerGroup()
		{
			return 0;
		}

		public virtual LayoutGroup GetGroupPrefab(int forGroupAtThisIndex)
		{
			return null;
		}

		public virtual int GetGroupIndex(int cellIndex)
		{
			return 0;
		}

		public virtual int GetNumberOfRequiredGroups(int numberOfCells)
		{
			return 0;
		}

		protected void CreateOrReinitCellGroupPrefab()
		{
		}

		protected virtual GameObject CreateCellGroupPrefabGameObject()
		{
			return null;
		}

		protected virtual LayoutGroup AddLayoutGroupToCellGroupPrefab(GameObject cellGroupGameObject)
		{
			return null;
		}

		protected virtual void InitOrReinitCellGroupPrefabLayoutGroup(LayoutGroup cellGroupGameObject)
		{
		}
	}
}
