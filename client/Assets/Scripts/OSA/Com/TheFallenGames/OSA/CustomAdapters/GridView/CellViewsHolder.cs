using Com.TheFallenGames.OSA.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Com.TheFallenGames.OSA.CustomAdapters.GridView
{
	public abstract class CellViewsHolder : AbstractViewsHolder
	{
		public RectTransform views;

		public LayoutElement rootLayoutElement;

		public override void Init(GameObject rootPrefabGO, RectTransform parent, int itemIndex, bool activateRootGameObject = true, bool callCollectViews = true)
		{
			throw new OSAException("A cell cannot be initialized this way. Use InitWithExistingRootPrefab(RectTransform) instead");
		}

		public virtual void InitWithExistingRootPrefab(RectTransform root)
		{
			this.root = root;
			ItemIndex = -1;
			CollectViews();
		}

		public override void CollectViews()
		{
			views = GetViews();
			if (views == root)
			{
				throw new OSAException("CellViewsHolder: views == root not allowed: you should have a child of root that holds all the views, as the root should always be enabled for layouting purposes");
			}
			rootLayoutElement = root.GetComponent<LayoutElement>();
			if (!rootLayoutElement)
			{
				throw new OSAException("CellViewsHolder: no LayoutElement found on the root: you should add one to configure how the cell's parent LayoutGroup should position/size it");
			}
		}

		public override void MarkForRebuild()
		{
			base.MarkForRebuild();
			if ((bool)views)
			{
				LayoutRebuilder.MarkLayoutForRebuild(views);
			}
		}

		protected virtual RectTransform GetViews()
		{
			RectTransform rectTransform = root.Find("Views") as RectTransform;
			if (!rectTransform)
			{
				throw new OSAException("Override " + GetType().ToString() + ".GetViews() and provide your own path to the child containing the views. For more info, check the Grid example scene");
			}
			return rectTransform;
		}
	}
}
