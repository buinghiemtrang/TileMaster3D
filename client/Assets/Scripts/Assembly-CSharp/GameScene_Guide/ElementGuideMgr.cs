using System.Collections.Generic;
using GameScene;
using GameScene_Element;
using GameScene_Level;
using _FrameWork;
using UnityEngine;

namespace GameScene_Guide
{
	public class ElementGuideMgr : MonoBehaviour
	{
		public static ElementGuideMgr I;

		[SerializeField]
		private Transform hideNode;

		private bool _isMove;

		private bool _isHidden;

		private List<Element> _elements;

		private Element _guidedElement;

		private ElementGuideView _view;

		private void Awake()
		{
			I = this;
			_elements = new List<Element>();
			_isHidden = true;
			hideNode.gameObject.SetActive(false);
		}

		public void Init(ElementGuideView view)
		{
			_view = view;
			_view.Init();
			_isMove = false;
			_elements.Clear();
			_guidedElement = null;
		}

		private void Update()
		{
			if (!_isMove)
			{
				return;
			}
			bool flag = _guidedElement != null && (bool)_guidedElement.Self && !_guidedElement.IsLocked;
			if (_elements.Count != 0 || flag)
			{
				if (flag)
				{
					if (!_guidedElement.IsSelected)
					{
						if (_isHidden)
						{
							if (hideNode.IsActive())
							{
								return;
							}
							_isMove = true;
							_isHidden = false;
							hideNode.SetActive(true);
						}
						_SetPosition();
					}
					else if (hideNode.IsActive())
					{
						_Hide();
					}
				}
				else
				{
					_Guide();
				}
				return;
			}
			_isHidden = true;
			hideNode.SetActive(false);
			_isMove = false;
			_guidedElement = null;
			_view.Hide();
			FingerTouchMgr.I.SetRaycastMask(false);
			DispatcherMgr.Emit("GameScene_Guide_SpecialElement_End");
		}

		public bool CheckGuide()
		{
			int lv = LevelMgr.Data.Lv;
			Dictionary<int, List<Element>> displayedElementsId = ElementsMgr.I.Data.DisplayedElementsId;
			int[,] elementGuide = cf_const.ElementGuide;
			for (int i = 0; i < elementGuide.GetLength(0); i++)
			{
				if (lv == elementGuide[i, 0] && displayedElementsId.TryGetValue(elementGuide[i, 1], out var value))
				{
					for (int j = 0; j < value.Count; j++)
					{
						Element element = value[j];
						if (!element.IsGuided && !element.IsSelected && !element.IsLocked)
						{
							_elements.Add(element);
							if (_elements.Count > 2)
							{
								break;
							}
						}
					}
					if (_elements.Count == 0)
					{
						return false;
					}
					DispatcherMgr.Emit("GameScene_Guide_SpecialElement_Start");
					_Guide();
					return true;
				}
			}
			return false;
		}

		private void _Guide()
		{
			if (_guidedElement != null)
			{
				_guidedElement.ModifyGuideStatus(false);
			}
			_guidedElement = _elements[0];
			_elements.RemoveAt(0);
			_guidedElement.SetLayer(14);
			_guidedElement.ModifyGuideStatus(true);
			_guidedElement.ModifyScaleBase(1.4f);
			_guidedElement.ScaleToSize(1.4f, 0.1f);
			_view.Show(_guidedElement.Config);
			_Show();
		}

		private void _AddEffect(Element element)
		{
			GameObject gameObject = Object.Instantiate(LoadMgr.Load<GameObject>("Effect/Element_Guide_Effect"));
			ParticleSystemInfo component = gameObject.GetComponent<ParticleSystemInfo>();
			component.SetParent(element.Self);
			component.SetPosition(Vector3.zero);
			component.SetAngle(Vector3.zero);
			component.Play();
		}

		private void _Show()
		{
			_isMove = true;
			_isHidden = false;
			hideNode.SetActive(true);
			_SetPosition();
		}

		private void _SetPosition()
		{
			Vector3 position = _guidedElement.Self.position;
			hideNode.position = new Vector3(position.x, position.y + 2f, position.z - 0.7f);
		}

		private void _Hide()
		{
			_isHidden = true;
			hideNode.gameObject.SetActive(false);
		}
	}
}
