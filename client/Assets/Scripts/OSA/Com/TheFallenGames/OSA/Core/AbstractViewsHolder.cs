using UnityEngine;
using UnityEngine.UI;

namespace Com.TheFallenGames.OSA.Core
{
	public abstract class AbstractViewsHolder
	{
		public RectTransform root;

		public virtual int ItemIndex { get; set; }

		public void Init(RectTransform rootPrefab, RectTransform parent, int itemIndex, bool activateRootGameObject = true, bool callCollectViews = true)
		{
			Init(rootPrefab.gameObject, parent, itemIndex, activateRootGameObject, callCollectViews);
		}

		public virtual void Init(GameObject rootPrefabGO, RectTransform parent, int itemIndex, bool activateRootGameObject = true, bool callCollectViews = true)
		{
			root = Object.Instantiate(rootPrefabGO, parent, false).transform as RectTransform;
			if (activateRootGameObject)
			{
				root.gameObject.SetActive(true);
			}
			ItemIndex = itemIndex;
			if (callCollectViews)
			{
				CollectViews();
			}
		}

		public virtual void CollectViews()
		{
		}

		public virtual void MarkForRebuild()
		{
			if ((bool)root)
			{
				LayoutRebuilder.MarkLayoutForRebuild(root);
			}
		}

		public virtual void UnmarkForRebuild()
		{
		}

		public virtual void ShiftIndex(int shift, int modulo)
		{
			ItemIndex = ShiftIntWithOverflowCheck(ItemIndex, shift, modulo);
		}

		public virtual void OnBeforeDestroy()
		{
		}

		protected int ShiftIntWithOverflowCheck(int value, int shift, int modulo)
		{
			return (int)(((long)value + (long)shift + (long)modulo) % (long)modulo);
		}
	}
}
