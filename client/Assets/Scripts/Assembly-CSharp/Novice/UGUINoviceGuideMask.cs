using UnityEngine;
using UnityEngine.UI;

namespace Novice
{
	public class UGUINoviceGuideMask : MaskableGraphic, ICanvasRaycastFilter
	{
		public RectTransform _HollowingOutTarget;

		public RectTransform _RaycastTarget;

		private Vector3 _targetMin = Vector3.zero;

		private Vector3 _targetMax = Vector3.zero;

		private bool _canRefresh = true;

		private Transform _cacheTrans;

		public void SetTarget(RectTransform target)
		{
			_canRefresh = true;
			_HollowingOutTarget = target;
			_RefreshView();
		}

		private void _SetTarget(Vector3 tarMin, Vector3 tarMax)
		{
			if (!((tarMin - _targetMin).sqrMagnitude < 1E-10f) || !((tarMax - _targetMax).sqrMagnitude < 1E-10f))
			{
				_targetMin = tarMin;
				_targetMax = tarMax;
				SetVerticesDirty();
			}
		}

		private void _RefreshView()
		{
			if (_canRefresh)
			{
				_canRefresh = false;
				if (_HollowingOutTarget == null)
				{
					_SetTarget(Vector3.zero, Vector3.zero);
					SetVerticesDirty();
					return;
				}
				Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(_cacheTrans, _HollowingOutTarget);
				_SetTarget(bounds.min, bounds.max);
			}
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			if (_targetMin == Vector3.zero && _targetMax == Vector3.zero)
			{
				base.OnPopulateMesh(vh);
				return;
			}
			vh.Clear();
			UIVertex simpleVert = UIVertex.simpleVert;
			simpleVert.color = color;
			Vector2 pivot = base.rectTransform.pivot;
			Rect rect = base.rectTransform.rect;
			float num = (0f - pivot.x) * rect.width;
			float num2 = (0f - pivot.y) * rect.height;
			float num3 = (1f - pivot.x) * rect.width;
			float num4 = (1f - pivot.y) * rect.height;
			simpleVert.position = new Vector3(num, num4);
			vh.AddVert(simpleVert);
			simpleVert.position = new Vector3(num3, num4);
			vh.AddVert(simpleVert);
			simpleVert.position = new Vector3(num3, num2);
			vh.AddVert(simpleVert);
			simpleVert.position = new Vector3(num, num2);
			vh.AddVert(simpleVert);
			simpleVert.position = new Vector3(_targetMin.x, _targetMax.y);
			vh.AddVert(simpleVert);
			simpleVert.position = new Vector3(_targetMax.x, _targetMax.y);
			vh.AddVert(simpleVert);
			simpleVert.position = new Vector3(_targetMax.x, _targetMin.y);
			vh.AddVert(simpleVert);
			simpleVert.position = new Vector3(_targetMin.x, _targetMin.y);
			vh.AddVert(simpleVert);
			vh.AddTriangle(4, 0, 1);
			vh.AddTriangle(4, 1, 5);
			vh.AddTriangle(5, 1, 2);
			vh.AddTriangle(5, 2, 6);
			vh.AddTriangle(6, 2, 3);
			vh.AddTriangle(6, 3, 7);
			vh.AddTriangle(7, 3, 0);
			vh.AddTriangle(7, 0, 4);
		}

		bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 screenPos, Camera eventCamera)
		{
			if (_RaycastTarget == null)
			{
				return true;
			}
			return !RectTransformUtility.RectangleContainsScreenPoint(_RaycastTarget, screenPos, eventCamera);
		}

		protected override void Awake()
		{
			base.Awake();
			_cacheTrans = GetComponent<RectTransform>();
		}

		private void Update()
		{
			_canRefresh = true;
			_RefreshView();
		}
	}
}
