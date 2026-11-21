using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace aaaaa
{
	public class ScrollMoveToPage : MonoBehaviour, IEndDragHandler, IEventSystemHandler, IBeginDragHandler
	{
		[SerializeField]
		private Transform _container;

		[SerializeField]
		private List<Transform> activeListItems;

		[SerializeField]
		private Text pageTex;

		[SerializeField]
		private GameObject leftArrowObj;

		[SerializeField]
		private GameObject rightArrowObj;

		private float _targetPos;

		private bool _centering;

		private Vector2 downPos;

		private Vector2 upPos;

		private int SelectedIndex;

		private int activeIndex = -1;

		private void Update()
		{
			if (!_centering)
			{
				return;
			}
			Vector3 localPosition = _container.localPosition;
			Vector3 localPosition2 = _container.localPosition;
			float t = Mathf.Clamp01(Time.deltaTime * 10f);
			localPosition.x = localPosition2.x + (_targetPos - localPosition2.x) * t;
			_container.localPosition = localPosition;
			Vector3 localPosition3 = _container.localPosition;
			if (Mathf.Abs(localPosition3.x - _targetPos) < 0.1f)
			{
				localPosition3.x = _targetPos;
				localPosition3.y = localPosition.y;
				localPosition3.z = localPosition.z;
				_container.localPosition = localPosition3;
				_centering = false;
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			_centering = false;
			downPos = eventData.position;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			upPos = eventData.position;
			int i = (downPos.x - upPos.x > 0f) ? 1 : -1;
			_changeSelecIndex(i);
		}

		private void _changeSelecIndex(int i)
		{
			if (i != 0)
			{
				int num = activeIndex + i;
				if (num >= 0 && num < activeListItems.Count)
				{
					SelectedIndex = num;
					activeIndex = num;
					pageTex.text = string.Format("{0}/{1}", num + 1, activeListItems.Count);
					_setItemScrollViewCenter();
				}
			}
		}

		private void _setItemScrollViewCenter()
		{
			Transform transform = activeListItems[SelectedIndex];
			Vector3 localPosition = transform.gameObject.transform.localPosition;
			_centering = true;
			_targetPos = -localPosition.x;
		}
	}
}
