using System;
using DG.Tweening;
using GameScene_Element;
using UnityEngine;

namespace GameScene
{
	public class PlacePoint : MonoBehaviour
	{
		[SerializeField]
		private Transform pNode;

		private Transform _self;

		private float _offsetX;

		private Sequence _placeSequence;

		private Sequence _upSequence;

		private Sequence _moveSequence;

		public bool IsFree { get; private set; }

		public bool IsPlacing { get; private set; }

		public Element PlacedElement { get; private set; }

		public int Index { get; private set; }

		private void Awake()
		{
			_self = base.transform;
		}

		public void Init(int index, float offset)
		{
			Index = index;
			_offsetX = (float)index * offset;
			Clear();
		}

		public void Place(Element element, Action<Element> callback, bool isUp)
		{
			if (!IsFree)
			{
				Debug.LogError("place_point_no_free");
				return;
			}
			PlacedElement = element;
			IsFree = false;
			IsPlacing = true;
			if (!isUp)
			{
				_PlaceAnim(callback);
				return;
			}
			_upSequence = DOTween.Sequence().SetId("Place_Point_Up");
			_upSequence.Join(PlacedElement.ScaleToSize(PlacedElement.ScaleBase * 1.1f, 0.2f).SetEase(Ease.Linear));
			_upSequence.OnComplete(delegate
			{
				_PlaceAnim(callback);
			});
		}

		private void _PlaceAnim(Action<Element> callback)
		{
			PlacedElement.SetLine(false);
			PlacedElement.SetLayer(12);
			cfgModel config = PlacedElement.Config;
			Vector3 position = _SetPosition(config);
			_placeSequence = DOTween.Sequence().SetId("Place_Point_Place");
			_placeSequence.Append(PlacedElement.MoveToPosition(position, 0.15f, true).SetEase(Ease.Linear));
			_placeSequence.Join(PlacedElement.ScaleToSize(config.pointScale, 0.15f).SetEase(Ease.Linear));
			_placeSequence.Join(PlacedElement.RotateToAngle(config.pointAngle, 0.1f, RotateMode.Fast).SetEase(Ease.Linear));
			_placeSequence.OnComplete(delegate
			{
				IsPlacing = false;
				PlacedElement.ModifyParent(pNode, false);
				PlacedElement.SetPosition(Vector3.zero, true);
				callback.InvokeSafe(PlacedElement);
			});
		}

		public void Move(Element element, Action<Element> callback = null)
		{
			if (!IsFree)
			{
				Debug.LogError("place point no free");
				return;
			}
			PlacedElement = element;
			IsFree = false;
			PlacedElement.SetLine(false);
			PlacedElement.SetLayer(12);
			cfgModel config = PlacedElement.Config;
			Vector3 position = _SetPosition(config);
			_moveSequence = DOTween.Sequence().SetId("Place_Point_Local_Move");
			_moveSequence.Append(PlacedElement.MoveToPosition(position, 0.15f, true).SetEase(Ease.Linear));
			_moveSequence.Join(PlacedElement.ScaleToSize(config.pointScale, 0.15f).SetEase(Ease.Linear));
			_moveSequence.OnComplete(delegate
			{
				PlacedElement.ModifyParent(pNode, true);
				PlacedElement.SetAngle(config.pointAngle);
				callback.InvokeSafe(PlacedElement);
			});
		}

		public void Clear()
		{
			PlacedElement = null;
			IsFree = true;
			IsPlacing = false;
			if (_moveSequence != null)
			{
				_moveSequence.Kill();
			}
			if (_placeSequence != null)
			{
				_placeSequence.Kill();
			}
			if (_upSequence != null)
			{
				_upSequence.Kill();
			}
		}

		private Vector3 _SetPosition(cfgModel config)
		{
			Vector3 vector = new Vector3(config.pointPos.x + _offsetX, config.pointPos.y, config.pointPos.z);
			_self.localPosition = vector;
			return vector;
		}

		public void SetElement(Element element)
		{
			PlacedElement = element;
			IsFree = false;
			IsPlacing = false;
			cfgModel config = PlacedElement.Config;
			_SetPosition(config);
			PlacedElement.Selected();
			PlacedElement.ModifyLockStatus(true);
			PlacedElement.SetLine(false);
			PlacedElement.SetLayer(12);
			PlacedElement.ModifyParent(pNode, false);
			PlacedElement.SetScale(config.pointScale);
			PlacedElement.SetPosition(Vector3.zero, true);
			PlacedElement.SetAngle(config.pointAngle);
		}
	}
}
