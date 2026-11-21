using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using frame8.Logic.Misc.Other.Extensions;

namespace frame8.Logic.Misc.Other
{
	public class UIUtils8 : Singleton8<UIUtils8>
	{
		public Vector2 WorldToScreenPointForCanvas(Canvas canvas, Camera camera, Vector3 position)
		{
			return RectTransformUtility.WorldToScreenPoint((canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : camera, position);
		}

		public Vector2? WorldToCanvasLocalPosition(Canvas canvas, RectTransform canvasRect, Camera camera, Vector3 position)
		{
			Camera cam = ((canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : camera);
			Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, position);
			Vector2 localPoint;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out localPoint))
			{
				return null;
			}
			return localPoint;
		}

		public Vector2? ScreenPointToLocalPointInCanvas(Canvas canvas, RectTransform canvasRect, Camera camera, Vector2 screenPoint)
		{
			Camera cam = ((canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : camera);
			Vector2 localPoint;
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out localPoint))
			{
				return null;
			}
			return localPoint;
		}

		public Vector3? ScreenPointToWorldPointInCanvas(Canvas canvas, RectTransform canvasRect, Camera camera, Vector2 screenPoint)
		{
			Camera cam = ((canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : camera);
			Vector3 worldPoint;
			if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screenPoint, cam, out worldPoint))
			{
				return null;
			}
			return worldPoint;
		}

		public Vector2 ScreenPointToLocalPointInRectangle(RectTransform rt, PointerEventData pointerEventData)
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, pointerEventData.position, pointerEventData.pressEventCamera, out localPoint);
			return localPoint;
		}

		public Vector2 GetWorldVectorBetweenCustomLocalPivots(RectTransform firstRT, Vector2 customPivotOnFirstRect01, RectTransform secondRT, Vector2 customPivotOnSecondRect01, Canvas canvas, RectTransform canvasRectTransform)
		{
			Vector2 vector = firstRT.ConvertPointNormalizedBySizeToLocalPoint(customPivotOnFirstRect01);
			Vector2 vector2 = secondRT.ConvertPointNormalizedBySizeToLocalPoint(customPivotOnSecondRect01);
			Vector3 vector3 = firstRT.TransformPoint(vector);
			return secondRT.TransformPoint(vector2) - vector3;
		}

		public void BringRectToAnchorsFor(Transform[] transforms)
		{
			foreach (Transform transform in transforms)
			{
				BringRectToAnchorsFor(transform);
			}
		}

		public void BringRectToAnchorsFor(Transform transform)
		{
			RectTransform rectTransform = transform as RectTransform;
			if (rectTransform != null)
			{
				Vector2 zero = Vector2.zero;
				rectTransform.offsetMax = zero;
				Vector2 vector = zero - (rectTransform.anchoredPosition - rectTransform.sizeDelta * rectTransform.pivot);
				rectTransform.sizeDelta -= vector;
				rectTransform.anchoredPosition += vector * (Vector2.one - rectTransform.pivot);
			}
		}

		public void BringAnchorsToRectFor(Transform[] transforms)
		{
			foreach (Transform transform in transforms)
			{
				BringAnchorsToRectFor(transform);
			}
		}

		public void BringAnchorsToRectFor(Transform transform)
		{
			RectTransform rectTransform = transform as RectTransform;
			if (!(rectTransform != null))
			{
				return;
			}
			RectTransform rectTransform2 = rectTransform.parent as RectTransform;
			if (rectTransform2 == null)
			{
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;
			}
			else
			{
				Rect rect = rectTransform.rect;
				Rect rect2 = rectTransform2.rect;
				Vector2 bottomLeftCornerDisplacementFromParentBottomLeftCorner = rectTransform.GetBottomLeftCornerDisplacementFromParentBottomLeftCorner();
				if (rect2.width > 0f && rect2.height > 0f)
				{
					rectTransform.anchorMin = new Vector2(Mathf.Clamp01(bottomLeftCornerDisplacementFromParentBottomLeftCorner.x / rect2.width), Mathf.Clamp01(bottomLeftCornerDisplacementFromParentBottomLeftCorner.y / rect2.height));
					rectTransform.anchorMax = new Vector2(Mathf.Clamp01((rect.width + bottomLeftCornerDisplacementFromParentBottomLeftCorner.x) / rect2.width), Mathf.Clamp01((rect.height + bottomLeftCornerDisplacementFromParentBottomLeftCorner.y) / rect2.height));
				}
			}
			BringRectToAnchorsFor(rectTransform);
		}

		public void GrowSizeDirectionally(RectTransform rectTransform, RectTransform.Edge edge, float amount)
		{
			Rect rect = rectTransform.rect;
			bool flag = (int)edge > 1;
			rectTransform.SetSizeWithCurrentAnchors(flag ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal, (flag ? rect.height : rect.width) + amount);
			Vector3 zero = Vector3.zero;
			float num = amount * 0.5f;
			if (flag)
			{
				zero.y = ((edge == RectTransform.Edge.Top) ? (zero.y + num) : (zero.y - num));
			}
			else
			{
				zero.x = ((edge == RectTransform.Edge.Right) ? (zero.x + num) : (zero.x - num));
			}
			rectTransform.localPosition = zero + rectTransform.localPosition;
		}

		public UIVertex[] GetCharQuadVertsInObjectSpaceUnscaledByCanvas(Text textComp, int charIndex)
		{
			if (textComp.text == null || charIndex >= textComp.text.Length)
			{
				return null;
			}
			TextGenerator cachedTextGenerator = textComp.cachedTextGenerator;
			int num = charIndex * 4;
			if (num >= cachedTextGenerator.vertexCount)
			{
				return null;
			}
			UIVertex[] obj = new UIVertex[4];
			IList<UIVertex> verts = cachedTextGenerator.verts;
			obj[0] = verts[num];
			obj[1] = verts[num | 1];
			obj[2] = verts[num | 2];
			obj[3] = verts[num | 3];
			return obj;
		}

		public void GetUITextCharQuadVertsPositionsInObjectSpaceScaledByCanvas(Text textComp, int charIndex, out Vector3[] positions)
		{
			UIVertex[] charQuadVertsInObjectSpaceUnscaledByCanvas = GetCharQuadVertsInObjectSpaceUnscaledByCanvas(textComp, charIndex);
			if (charQuadVertsInObjectSpaceUnscaledByCanvas == null)
			{
				positions = null;
				return;
			}
			positions = new Vector3[charQuadVertsInObjectSpaceUnscaledByCanvas.Length];
			float scaleFactor = textComp.canvas.scaleFactor;
			for (int i = 0; i < positions.Length; i++)
			{
				positions[i] = charQuadVertsInObjectSpaceUnscaledByCanvas[i].position / scaleFactor;
			}
		}

		public Vector3[] GetCharQuadVertsPositionsInObjectSpaceScaledByCanvas(Text textComp, int charIndex)
		{
			Vector3[] positions;
			GetUITextCharQuadVertsPositionsInObjectSpaceScaledByCanvas(textComp, charIndex, out positions);
			return positions;
		}

		public Vector3[] GetCharQuadVertsPositionsInWorldSpaceScaledByCanvas(Text textComp, int charIndex)
		{
			Vector3[] positions;
			GetUITextCharQuadVertsPositionsInObjectSpaceScaledByCanvas(textComp, charIndex, out positions);
			if (positions == null)
			{
				return null;
			}
			Transform transform = textComp.transform;
			positions[0] = transform.TransformPoint(positions[0]);
			positions[1] = transform.TransformPoint(positions[1]);
			positions[2] = transform.TransformPoint(positions[2]);
			positions[3] = transform.TransformPoint(positions[3]);
			return positions;
		}

		public Vector3? GetCharQuadCenterPositionInObjectSpaceScaledByCanvas(Text textComp, int charIndex)
		{
			Vector3[] positions;
			GetUITextCharQuadVertsPositionsInObjectSpaceScaledByCanvas(textComp, charIndex, out positions);
			if (positions == null)
			{
				return null;
			}
			return (positions[0] + positions[1] + positions[2] + positions[3]) * 0.25f;
		}

		public Vector3? GetCharQuadCenterPositionInWorldSpaceScaledByCanvas(Text textComp, int charIndex)
		{
			Vector3[] charQuadVertsPositionsInWorldSpaceScaledByCanvas = GetCharQuadVertsPositionsInWorldSpaceScaledByCanvas(textComp, charIndex);
			if (charQuadVertsPositionsInWorldSpaceScaledByCanvas == null)
			{
				return null;
			}
			return (charQuadVertsPositionsInWorldSpaceScaledByCanvas[0] + charQuadVertsPositionsInWorldSpaceScaledByCanvas[1] + charQuadVertsPositionsInWorldSpaceScaledByCanvas[2] + charQuadVertsPositionsInWorldSpaceScaledByCanvas[3]) * 0.25f;
		}

		public Vector3? GetCharQuadCenterLeftPositionInWorldSpaceScaledByCanvas(Text textComp, int charIndex)
		{
			Vector3[] charQuadVertsPositionsInWorldSpaceScaledByCanvas = GetCharQuadVertsPositionsInWorldSpaceScaledByCanvas(textComp, charIndex);
			if (charQuadVertsPositionsInWorldSpaceScaledByCanvas == null)
			{
				return null;
			}
			return (charQuadVertsPositionsInWorldSpaceScaledByCanvas[0] + charQuadVertsPositionsInWorldSpaceScaledByCanvas[3]) * 0.5f;
		}

		public override void Init()
		{
		}
	}
}
