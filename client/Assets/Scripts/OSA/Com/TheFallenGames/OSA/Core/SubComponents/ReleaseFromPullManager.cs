using frame8.Logic.Misc.Other.Extensions;

namespace Com.TheFallenGames.OSA.Core.SubComponents
{
	internal class ReleaseFromPullManager<TParams, TItemViewsHolder> where TParams : BaseParams where TItemViewsHolder : BaseItemViewsHolder
	{
		public bool inProgress;

		public double targetCTInsetFromVPS;

		private OSA<TParams, TItemViewsHolder> _Adapter;

		private ComputeVisibilityParams _ComputeVisibilityParams_Reusable = new ComputeVisibilityParams();

		public ReleaseFromPullManager(OSA<TParams, TItemViewsHolder> adapter)
		{
			_Adapter = adapter;
		}

		public double CalculateFirstItemTargetInsetFromVPS()
		{
			return targetCTInsetFromVPS + _Adapter._InternalState.paddingContentStart;
		}

		public double CalculateFirstItemInsetFromVPS()
		{
			TItemViewsHolder val = _Adapter._VisibleItems[0];
			double num = val.root.GetInsetFromParentEdge(_Adapter.Parameters.Content, _Adapter._InternalState.startEdge);
			if (val.itemIndexInView > 0)
			{
				num -= _Adapter._InternalState.GetItemVirtualInsetFromParentStartUsingItemIndexInView(val.itemIndexInView) - _Adapter._InternalState.paddingContentStart;
			}
			return num;
		}

		public void FinishNowByDraggingItems(bool computeVisibility)
		{
			if (inProgress)
			{
				double num = CalculateFirstItemTargetInsetFromVPS() - CalculateFirstItemInsetFromVPS();
				if (num != 0.0)
				{
					// delta truyen qua thanh ghi float (Ghidra khong hien): lay num theo phep tru vua tinh o tren
					_Adapter.DragVisibleItemsRangeUnchecked(0, _Adapter.VisibleItemsCount, num, true, computeVisibility, true, true);
				}
				inProgress = false;
			}
		}

		public void FinishNowBySettingContentInset(bool computeVisibility)
		{
			_ComputeVisibilityParams_Reusable.overrideDelta = targetCTInsetFromVPS - _Adapter._InternalState.ctVirtualInsetFromVPS_Cached;
			ContentSizeOrPositionChangeParams p = default(ContentSizeOrPositionChangeParams);
			p.cancelSnappingIfAny = true;
			p.fireScrollPositionChangedEvent = true;
			p.keepVelocity = true;
			p.computeVisibilityParams = (computeVisibility ? _ComputeVisibilityParams_Reusable : null);
			bool looped;
			_Adapter.SetContentVirtualInsetFromViewportStart(targetCTInsetFromVPS, ref p, out looped);
			inProgress = false;
		}
	}
}
