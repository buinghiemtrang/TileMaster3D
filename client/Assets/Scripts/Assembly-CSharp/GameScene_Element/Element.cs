using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using GameScene_Level;
using UnityEngine;

namespace GameScene_Element
{
	public class Element
	{
		private Rigidbody _rigidbody;

		private Outline3D _line;

		private int _layer;

		public Transform DefaultParentNode { get; private set; }

		public ElementType Type { get; private set; }

		public Transform Self { get; private set; }

		public cfgModel Config { get; private set; }

		public bool IsSelected { get; private set; }

		public bool IsLocked { get; private set; }

		public bool IsGuided { get; private set; }

		public bool IsPlaced { get; private set; }

		public bool IsFree { get; private set; }

		public float ScaleBase { get; private set; }

		public void Init(Transform parentNode, Transform self, cfgModel config)
		{
			DefaultParentNode = parentNode;
			Type = (ElementType)config.type;
			Self = self;
			Config = config;
			IsFree = false;
			IsSelected = false;
			IsLocked = false;
			IsGuided = false;
			IsPlaced = false;
			ScaleBase = 1f;
			_rigidbody = self.GetComponent<Rigidbody>();
			_line = self.FindC<Outline3D>("Model");
			_layer = 10;
			Self.SetScale3(1f);
			_rigidbody.constraints = RigidbodyConstraints.None;
			AddEffect();
		}

		protected virtual void AddEffect()
		{
			GameObject gameObject = LoadMgr.Load<GameObject>("Effect/Element_Special_Effect");
			if ((bool)gameObject)
			{
				Transform transform = UnityEngine.Object.Instantiate(gameObject).transform;
				transform.SetParent(Self);
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.Euler(Vector3.zero);
			}
		}

		public virtual void Highlight(bool isLight)
		{
			Self.DOKill();
			SetLine(isLight);
			float endValue = ScaleBase;
			float duration;
			if (isLight)
			{
				endValue *= 1.1f;
				duration = 0.2f;
			}
			else
			{
				duration = 0.5f;
			}
			Self.DOScale(endValue, duration).SetEase(Ease.Linear);
		}

		public virtual void Selected()
		{
			Self.DOKill();
			SetLine(isUse: true);
			IsSelected = true;
			_rigidbody.constraints = (RigidbodyConstraints)126;
			SetLayer(IsGuided ? 14 : 10);
		}

		public virtual void SelectedByAnim(Action<Element> callback)
		{
			Selected();
			Self.DOScale(ScaleBase * 1.1f, 0.2f).SetEase(Ease.Linear).OnComplete(delegate
			{
				callback.InvokeSafe(this);
			});
		}

		public virtual void CancelSelected()
		{
			Self.DOKill();
			IsSelected = false;
		}

		public virtual void CancelSelectedByAnim(float duration)
		{
			CancelSelected();
			SetLine(isUse: false);
			if (LevelMgr.Data.Type == LevelType.Ordinary && LevelMgr.Data.Lv == 1)
			{
				_rigidbody.constraints = (RigidbodyConstraints)122;
			}
			else
			{
				_rigidbody.constraints = RigidbodyConstraints.None;
			}
			Self.DOScale(ScaleBase, duration).SetEase(Ease.Linear);
		}

		public virtual void CancelSelectedByAnim()
		{
			CancelSelectedByAnim(0.5f);
		}

		public virtual void FallDown(float duration, Action callback)
		{
			CancelSelected();
			SetLine(isUse: false);
			if (LevelMgr.Data.Type == LevelType.Ordinary && LevelMgr.Data.Lv == 1)
			{
				_rigidbody.constraints = (RigidbodyConstraints)122;
			}
			else
			{
				_rigidbody.constraints = (RigidbodyConstraints)10;
				DelayCall(duration / 5f, delegate
				{
					_rigidbody.constraints = RigidbodyConstraints.None;
				});
			}
			Self.DOScale(ScaleBase, duration).SetEase(Ease.Linear).OnComplete(callback.Invoke); // UNVERIFIED: ctor target của TweenCallback bị decompiler mất, giả định callback.Invoke
		}

		public TweenerCore<Vector3, Vector3, VectorOptions> MoveToPosition(Vector3 position, float duration, bool isLocal = false)
		{
			if (isLocal)
			{
				return Self.DOLocalMove(position, duration);
			}
			return Self.DOMove(position, duration);
		}

		public void SetPosition(Vector3 position, bool isLocal = false)
		{
			if (isLocal)
			{
				Self.localPosition = position;
			}
			else
			{
				Self.position = position;
			}
		}

		public TweenerCore<Quaternion, Vector3, QuaternionOptions> RotateToAngle(Vector3 angle, float duration, RotateMode mode)
		{
			return Self.DOLocalRotate(angle, duration, mode);
		}

		public void SetAngle(Vector3 angle)
		{
			Self.localRotation = Quaternion.Euler(angle);
		}

		public TweenerCore<Vector3, Vector3, VectorOptions> ScaleToSize(float size, float duration)
		{
			return Self.DOScale(size, duration);
		}

		public void SetScale(float size)
		{
			Self.localScale = new Vector3(size, size, size);
		}

		public void DelayCall(float delay, Action callback)
		{
			Self.DelayCall(delay, callback.Invoke);
		}

		public void Freeze(RigidbodyConstraints type)
		{
			_rigidbody.constraints = type;
		}

		public void AddForce(Vector3 force, ForceMode mode)
		{
			_rigidbody.AddForce(force, mode);
		}

		public void ModifyLockStatus(bool isLocked)
		{
			IsLocked = isLocked;
		}

		public void ModifySelectionStatus(bool isSelected)
		{
			IsSelected = isSelected;
		}

		public void ModifyGuideStatus(bool isGuided)
		{
			IsGuided = isGuided;
		}

		public void ModifyPlaceStatus(bool isPlaced)
		{
			IsPlaced = isPlaced;
		}

		public void ModifyParent(Transform node, bool worldPositionStays)
		{
			Self.SetParent(node, worldPositionStays);
		}

		public void ModifyScaleBase(float value)
		{
			ScaleBase = value;
		}

		public void SetGravity(bool isUse)
		{
			if (_rigidbody.useGravity != isUse)
			{
				_rigidbody.useGravity = isUse;
			}
		}

		public void SetLine(bool isUse)
		{
			if (_line.enabled != isUse)
			{
				_line.enabled = isUse;
			}
		}

		public void SetLayer(int layer)
		{
			if (_layer != layer)
			{
				_layer = layer;
				Transform[] componentsInChildren = Self.GetComponentsInChildren<Transform>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].gameObject.layer = layer;
				}
			}
		}

		public void SetActive(bool isActive)
		{
			Self.gameObject.SetActive(isActive);
		}

		public virtual void ExecuteDestroyEffect()
		{
		}

		public virtual void Destroy()
		{
			IsFree = true;
			ClearAnim();
			UnityEngine.Object.Destroy(Self.gameObject);
			Self = null;
			_rigidbody = null;
			_line = null;
		}

		public virtual void ClearAnim()
		{
			DOTween.Kill(Self);
		}
	}
}
