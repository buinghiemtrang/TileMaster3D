using System;
using UnityEngine;

namespace frame8.Logic.Misc.Other.Extensions
{
	public static class RectTransformExtensions
	{
		private static Func<RectTransform, RectTransform, float>[] _GetInsetFromParentEdge_MappedActions = new Func<RectTransform, RectTransform, float>[4]
		{
			GetInsetFromParentLeftEdge,
			GetInsetFromParentRightEdge,
			GetInsetFromParentTopEdge,
			GetInsetFromParentBottomEdge
		};

		public static bool IsLocalPointInRect(this RectTransform rt, Vector2 localPoint)
		{
			localPoint = rt.ConvertLocalPointToPointNormalizedBySize(localPoint);
			if (localPoint.x >= 0f && localPoint.x <= 1f && localPoint.y >= 0f)
			{
				return localPoint.y <= 1f;
			}
			return false;
		}

		public static Vector2 GetBottomLeftCornerDisplacementFromParentBottomLeftCorner(this RectTransform rt)
		{
			Rect rect = rt.rect;
			Rect rect2 = (rt.parent as RectTransform).rect;
			Vector3 localPosition = rt.localPosition;
			return new Vector2(rect.x - rect2.x + localPosition.x, rect.y - rect2.y + localPosition.y);
		}

		public static Vector2 ConvertLocalPointToPointNormalizedBySize(this RectTransform rt, Vector2 localPoint)
		{
			Vector2 size = rt.rect.size;
			Vector2 pivot = rt.pivot;
			pivot.Scale(size);
			localPoint += pivot;
			localPoint.x /= size.x;
			localPoint.y /= size.y;
			return localPoint;
		}

		public static Vector2 ConvertPointNormalizedBySizeToLocalPoint(this RectTransform rt, Vector2 normalizedBySize01)
		{
			Vector2 size = rt.rect.size;
			normalizedBySize01.Scale(size);
			Vector2 pivot = rt.pivot;
			pivot.Scale(size);
			return normalizedBySize01 - pivot;
		}

		public static float GetInsetFromParentLeftEdge(this RectTransform child, RectTransform parentHint)
		{
			Vector2 parentPivot = parentHint.pivot;
			Rect parentRect = parentHint.rect;
			return child.GetInsetFromParentLeftEdge(ref parentPivot, ref parentRect);
		}

		public static float GetInsetFromParentLeftEdge(this RectTransform child, ref Vector2 parentPivot, ref Rect parentRect)
		{
			float num = parentPivot.x * parentRect.width;
			return child.localPosition.x - child.pivot.x * child.rect.width + num;
		}

		public static float GetInsetFromParentRightEdge(this RectTransform child, RectTransform parentHint)
		{
			Vector2 parentPivot = parentHint.pivot;
			Rect parentRect = parentHint.rect;
			return child.GetInsetFromParentRightEdge(ref parentPivot, ref parentRect);
		}

		public static float GetInsetFromParentRightEdge(this RectTransform child, ref Vector2 parentPivot, ref Rect parentRect)
		{
			float num = (1f - parentPivot.x) * parentRect.width;
			return num - (child.localPosition.x + (1f - child.pivot.x) * child.rect.width);
		}

		public static float GetInsetFromParentTopEdge(this RectTransform child, RectTransform parentHint)
		{
			Vector2 parentPivot = parentHint.pivot;
			Rect parentRect = parentHint.rect;
			return child.GetInsetFromParentTopEdge(ref parentPivot, ref parentRect);
		}

		public static float GetInsetFromParentTopEdge(this RectTransform child, ref Vector2 parentPivot, ref Rect parentRect)
		{
			float num = (1f - parentPivot.y) * parentRect.height;
			return num - (child.localPosition.y + (1f - child.pivot.y) * child.rect.height);
		}

		public static float GetInsetFromParentBottomEdge(this RectTransform child, RectTransform parentHint)
		{
			Vector2 parentPivot = parentHint.pivot;
			Rect parentRect = parentHint.rect;
			return child.GetInsetFromParentBottomEdge(ref parentPivot, ref parentRect);
		}

		public static float GetInsetFromParentBottomEdge(this RectTransform child, ref Vector2 parentPivot, ref Rect parentRect)
		{
			float num = parentPivot.y * parentRect.height;
			return child.localPosition.y - child.pivot.y * child.rect.height + num;
		}

		public static float GetInsetFromParentEdge(this RectTransform child, RectTransform parentHint, RectTransform.Edge parentEdge)
		{
			return _GetInsetFromParentEdge_MappedActions[(int)parentEdge](child, parentHint);
		}

		public static void SetInsetAndSizeFromParentLeftEdgeWithCurrentAnchors(this RectTransform child, float newInset, float newSize)
		{
			child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge.Left, newInset, newSize);
		}

		public static void SetInsetAndSizeFromParentRightEdgeWithCurrentAnchors(this RectTransform child, float newInset, float newSize)
		{
			child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge.Right, newInset, newSize);
		}

		public static void SetInsetAndSizeFromParentTopEdgeWithCurrentAnchors(this RectTransform child, float newInset, float newSize)
		{
			child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge.Top, newInset, newSize);
		}

		public static void SetInsetAndSizeFromParentBottomEdgeWithCurrentAnchors(this RectTransform child, float newInset, float newSize)
		{
			child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge.Bottom, newInset, newSize);
		}

		private static void AfterSetInsetAndSizeFromParentEdgeWithCurrentAnchors_RestoreAnchorsIfNeeded(this RectTransform child, int axisIndex, Vector2 origSizeDelta, ref Vector2 origAnchorMin, ref Vector2 origAnchorMax, float sizeChange)
		{
			Vector3 localPosition = child.localPosition;
			bool flag = false;
			if (child.anchorMin != origAnchorMin)
			{
				child.anchorMin = origAnchorMin;
				flag = true;
			}
			if (child.anchorMax != origAnchorMax)
			{
				child.anchorMax = origAnchorMax;
				flag = true;
			}
			if (flag)
			{
				child.localPosition = localPosition;
				origSizeDelta[axisIndex] += sizeChange;
				child.sizeDelta = origSizeDelta;
			}
		}

		public static void SetSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform parentCached, RectTransform.Edge fixedEdge, float newSize)
		{
			float insetFromParentEdge = child.GetInsetFromParentEdge(parentCached, fixedEdge);
			child.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(fixedEdge, insetFromParentEdge, newSize);
		}

		public static void SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this RectTransform child, RectTransform.Edge fixedEdge, float newInset, float newSize)
		{
			int num = (int)fixedEdge / 2;
			Vector2 origAnchorMin = child.anchorMin;
			Vector2 origAnchorMax = child.anchorMax;
			Vector2 sizeDelta = child.sizeDelta;
			float num2 = child.rect.size[num];
			child.SetInsetAndSizeFromParentEdge(fixedEdge, newInset, newSize);
			child.AfterSetInsetAndSizeFromParentEdgeWithCurrentAnchors_RestoreAnchorsIfNeeded(num, sizeDelta, ref origAnchorMin, ref origAnchorMax, newSize - num2);
		}

		public static void MatchParentSize(this RectTransform rt, bool preservePivot)
		{
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.sizeDelta = Vector3.zero;
			Vector2 pivot = rt.pivot;
			rt.pivot = Vector2.one * 0.5f;
			rt.anchoredPosition = Vector3.zero;
			if (preservePivot)
			{
				rt.pivot = pivot;
			}
		}

		public static void TryClampPositionToParentBoundary(this RectTransform rt)
		{
			Canvas canvas = null;
			Transform parent = rt;
			while ((bool)(parent = parent.parent))
			{
				Canvas component = parent.GetComponent<Canvas>();
				if ((bool)component)
				{
					canvas = component;
				}
			}
			if ((bool)canvas)
			{
				RectTransform obj = canvas.transform as RectTransform;
				Vector3[] array = new Vector3[4];
				Vector3[] array2 = new Vector3[4];
				obj.GetWorldCorners(array);
				rt.GetWorldCorners(array2);
				if (array2[0].y <= array[0].y)
				{
					rt.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge.Bottom, 20f, rt.rect.height);
				}
				else if (array2[1].y >= array[1].y)
				{
					rt.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(RectTransform.Edge.Top, 20f, rt.rect.height);
				}
			}
		}
	}
}
