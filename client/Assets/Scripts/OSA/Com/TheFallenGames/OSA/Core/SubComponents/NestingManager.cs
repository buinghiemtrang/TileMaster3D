using UnityEngine;
using UnityEngine.EventSystems;

namespace Com.TheFallenGames.OSA.Core.SubComponents
{
	internal class NestingManager<TParams, TItemViewsHolder> : IInitializePotentialDragHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler where TParams : BaseParams where TItemViewsHolder : BaseItemViewsHolder
	{
		private OSA<TParams, TItemViewsHolder> _Adapter;

		private InternalState<TItemViewsHolder> _InternalState;

		private IInitializePotentialDragHandler parentInitializePotentialDragHandler;

		private IBeginDragHandler parentBeginDragHandler;

		private IDragHandler parentDragHandler;

		private IEndDragHandler parentEndDragHandler;

		private IScrollHandler parentScrollHandler;

		private bool _SearchedAtLeastOnce;

		private bool _CurrentDragCapturedByParent;

		private bool _CurrentScrollConsumedByParent;

		public bool SearchedParentAtLeastOnce => _SearchedAtLeastOnce;

		public bool CurrentDragCapturedByParent => _CurrentDragCapturedByParent;

		public bool CurrentScrollConsumedByParent => _CurrentScrollConsumedByParent;

		public NestingManager(OSA<TParams, TItemViewsHolder> adapter)
		{
			_Adapter = adapter;
			_InternalState = _Adapter._InternalState;
		}

		public void FindAndStoreNestedParent()
		{
			parentInitializePotentialDragHandler = null;
			parentBeginDragHandler = null;
			parentDragHandler = null;
			parentEndDragHandler = null;
			parentScrollHandler = null;
			Transform parent = _Adapter.transform;
			while (true)
			{
				parent = parent.parent;
				bool hasParent = parent;
				if (parentInitializePotentialDragHandler != null || !hasParent)
				{
					break;
				}
				parentInitializePotentialDragHandler = parent.GetComponent(typeof(IInitializePotentialDragHandler)) as IInitializePotentialDragHandler;
				if (parentInitializePotentialDragHandler == null)
				{
					continue;
				}
				parentBeginDragHandler = parentInitializePotentialDragHandler as IBeginDragHandler;
				if (parentBeginDragHandler != null)
				{
					parentDragHandler = parentInitializePotentialDragHandler as IDragHandler;
					if (parentDragHandler != null)
					{
						parentEndDragHandler = parentInitializePotentialDragHandler as IEndDragHandler;
						if (parentEndDragHandler != null)
						{
							continue;
						}
						parentInitializePotentialDragHandler = null;
						parentBeginDragHandler = null;
						parentDragHandler = null;
					}
					else
					{
						parentInitializePotentialDragHandler = null;
						parentBeginDragHandler = null;
					}
				}
				else
				{
					parentInitializePotentialDragHandler = null;
				}
			}
			if (parentInitializePotentialDragHandler != null)
			{
				parentScrollHandler = parentInitializePotentialDragHandler as IScrollHandler;
				_SearchedAtLeastOnce = true;
				return;
			}
			Transform scrollParent = _Adapter.transform;
			while (true)
			{
				scrollParent = scrollParent.parent;
				if (!scrollParent || parentScrollHandler != null)
				{
					break;
				}
				parentScrollHandler = scrollParent.GetComponent(typeof(IScrollHandler)) as IScrollHandler;
			}
			_SearchedAtLeastOnce = true;
		}

		public void OnInitializePotentialDrag(PointerEventData eventData)
		{
			_CurrentDragCapturedByParent = false;
			if (!_SearchedAtLeastOnce)
			{
				FindAndStoreNestedParent();
			}
			if (parentInitializePotentialDragHandler != null)
			{
				parentInitializePotentialDragHandler.OnInitializePotentialDrag(eventData);
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (parentInitializePotentialDragHandler == null)
			{
				return;
			}
			if (!_Adapter.Parameters.DragEnabled)
			{
				_CurrentDragCapturedByParent = true;
			}
			else
			{
				Vector2 delta = eventData.delta;
				_CurrentDragCapturedByParent = 0f <= (Mathf.Abs(delta.y) - Mathf.Abs(delta.x)) * (float)_InternalState.hor1_vertMinus1;
				if (!_CurrentDragCapturedByParent)
				{
					_CurrentDragCapturedByParent = CheckForForwardingToParent(delta);
					if (!_CurrentDragCapturedByParent)
					{
						return;
					}
				}
			}
			parentBeginDragHandler.OnBeginDrag(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (parentInitializePotentialDragHandler != null)
			{
				parentDragHandler.OnDrag(eventData);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (parentInitializePotentialDragHandler != null)
			{
				parentEndDragHandler.OnEndDrag(eventData);
				_CurrentDragCapturedByParent = false;
			}
		}

		public void OnScroll(PointerEventData eventData)
		{
			Vector2 delta = default(Vector2);
			_CurrentScrollConsumedByParent = false;
			if (!_SearchedAtLeastOnce)
			{
				FindAndStoreNestedParent();
			}
			if (parentScrollHandler == null)
			{
				return;
			}
			if (_Adapter.Parameters.ScrollEnabled)
			{
				Vector2 rawDelta = new Vector2(eventData.scrollDelta.x, 0f - eventData.scrollDelta.y);
				delta = rawDelta;
				_Adapter.Parameters.ApplyScrollSensitivityTo(ref delta);
				if (delta[_InternalState.hor0_vert1] != 0f)
				{
					if (!CheckForForwardingToParent(delta))
					{
						return;
					}
				}
				else if (rawDelta[_InternalState.hor0_vert1] == 0f && delta[1 - _InternalState.hor0_vert1] != 0f)
				{
					return;
				}
			}
			_CurrentScrollConsumedByParent = true;
			parentScrollHandler.OnScroll(eventData);
		}

		private bool CheckForForwardingToParent(Vector2 delta)
		{
			if (!_Adapter.Parameters.ForwardDragSameDirectionAtBoundary)
			{
				return false;
			}
			float num = delta[_InternalState.hor0_vert1];
			if (0f <= num * (float)_InternalState.hor1_vertMinus1)
			{
				return _InternalState.ctVirtualInsetFromVPS_Cached >= -3.0;
			}
			return _Adapter.ContentVirtualInsetFromViewportEnd >= -3.0;
		}
	}
}
