using System.Collections.Generic;
using GameScene_Element;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameScene
{
	public class FingerTouchMgr : MonoBehaviour
	{
		public class TouchInfo
		{
			public int FingerId;

			public bool IsDown;

			public Element SelectedElement;

			public Vector3 Position;
		}

		public static FingerTouchMgr I;

		public Camera elementCamera;

		private Dictionary<int, TouchInfo> _infos;

		private bool _isCanOperation;

		private int _raycastMask;

		private void Awake()
		{
			I = this;
			_infos = new Dictionary<int, TouchInfo>();
			DispatcherMgr.On("InGame_Interstitial_Start", _ClearInfos);
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off("InGame_Interstitial_Start", _ClearInfos);
		}

		public void Init()
		{
			_isCanOperation = true;
			_infos.Clear();
		}

		public void SetRaycastMask(bool isGuide)
		{
			_raycastMask = (isGuide ? 16384 : 5120);
		}

		private void Update()
		{
			GameStatus gameStatus = GameMgr.I.GameStatus;
			if (gameStatus == GameStatus.None)
			{
				return;
			}
			if (gameStatus == GameStatus.Playing)
			{
				_isCanOperation = true;
				_Mouse();
				_Touch();
			}
			else if (_isCanOperation)
			{
				_isCanOperation = false;
				_ClearInfos();
			}
		}

		private void _Down(TouchInfo info)
		{
			if (!info.IsDown)
			{
				info.IsDown = true;
				_Check(info);
			}
		}

		private void _Move(TouchInfo info)
		{
			if (info.IsDown)
			{
				_Check(info);
			}
		}

		private void _Up(TouchInfo info, bool isFalseUp)
		{
			if (!info.IsDown)
			{
				return;
			}
			if (isFalseUp)
			{
				info.SelectedElement.CancelSelectedByAnim();
				info.SelectedElement = null;
				OperationTip.I.CancelHighlightTip();
				OperationTip.I.CancelHintTip();
				return;
			}
			_infos.Remove(info.FingerId);
			Element selectedElement = info.SelectedElement;
			if (selectedElement.IsLocked)
			{
				return;
			}
			if (PlatformMgr.I.JudgePlace(selectedElement))
			{
				AudioMgr.I.PlaySound("selected");
				info.SelectedElement.CancelSelected();
				PlatformMgr.I.Place(info.SelectedElement, false);
			}
			else
			{
				info.SelectedElement.CancelSelectedByAnim();
			}
			OperationTip.I.CancelHighlightTip();
			OperationTip.I.CancelHintTip();
		}

		private void _Check(TouchInfo info)
		{
			List<Vector3> list = _RayOrigins(info.Position);
			for (int i = 0; i < list.Count; i++)
			{
				Ray ray = elementCamera.ScreenPointToRay(list[i]);
				if (Physics.Raycast(ray, out var hitInfo, 100f, _raycastMask))
				{
					Element element = ElementsMgr.I.Data.GetElement(hitInfo.transform);
					if (element == null || element.IsLocked)
					{
						break;
					}
					if (element == info.SelectedElement)
					{
						return;
					}
					if (info.SelectedElement != null)
					{
						_Up(info, true);
					}
					element.SelectedByAnim(null);
					info.SelectedElement = element;
					return;
				}
			}
			_Up(info, true);
		}

		private void _ClearInfos()
		{
			foreach (KeyValuePair<int, TouchInfo> info in _infos)
			{
				TouchInfo value = info.Value;
				if (value.SelectedElement != null)
				{
					value.SelectedElement.CancelSelectedByAnim();
				}
			}
			_infos.Clear();
		}

		private TouchInfo _Info(int id)
		{
			if (!_infos.ContainsKey(id))
			{
				_infos.Add(id, new TouchInfo());
			}
			return _infos[id];
		}

		private List<Vector3> _RayOrigins(Vector3 position)
		{
			List<Vector3> list = new List<Vector3>();
			list.Add(position);
			for (int i = 0; i < 8; i++)
			{
				Vector3 item;
				switch (i)
				{
				case 0:
					item = new Vector3(position.x - 20f, position.y, 0f);
					break;
				case 1:
					item = new Vector3(position.x - 16f, position.y + 16f, 0f);
					break;
				case 2:
					item = new Vector3(position.x, position.y + 20f, 0f);
					break;
				case 3:
					item = new Vector3(position.x + 16f, position.y + 16f, 0f);
					break;
				case 4:
					item = new Vector3(position.x + 20f, position.y, 0f);
					break;
				case 5:
					item = new Vector3(position.x + 16f, position.y - 16f, 0f);
					break;
				case 6:
					item = new Vector3(position.x, position.y - 20f, 0f);
					break;
				default:
					item = new Vector3(position.x - 16f, position.y - 16f, 0f);
					break;
				}
				list.Add(item);
			}
			return list;
		}

		private void _Mouse()
		{
#if UNITY_EDITOR // [rebuild] Editor luôn nhận chuột để test; bản build giữ nguyên gốc (config isOpenMouseCheck=false)
			if (Input.touchCount > 0)
#else
			if (!cf_const.isOpenMouseCheck || Input.touchCount > 0)
#endif
			{
				return;
			}
			if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
			{
				return;
			}
			TouchInfo touchInfo = _Info(1);
			touchInfo.FingerId = 1;
			touchInfo.Position = Input.mousePosition;
			if (Input.GetMouseButtonDown(0))
			{
				_Down(touchInfo);
				return;
			}
			if (Input.GetMouseButton(0))
			{
				_Move(touchInfo);
			}
			if (Input.GetMouseButtonUp(0))
			{
				_Up(touchInfo, false);
			}
		}

		private void _Touch()
		{
			if (Input.touchCount != 0)
			{
				for (int i = 0; i < Input.touchCount; i++)
				{
					Touch touch = Input.GetTouch(i);
					if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId) && touch.fingerId < 2)
					{
						TouchInfo touchInfo = _Info(touch.fingerId);
						touchInfo.FingerId = touch.fingerId;
						touchInfo.Position = new Vector3(touch.position.x, touch.position.y, 0f);
						switch (touch.phase)
						{
						case TouchPhase.Ended:
							_Up(touchInfo, false);
							break;
						case TouchPhase.Moved:
							_Move(touchInfo);
							break;
						case TouchPhase.Began:
							_Down(touchInfo);
							break;
						}
					}
				}
			}
#if UNITY_EDITOR // [rebuild] không xoá thông tin chạm của chuột (_Mouse) khi đang giữ chuột; build giữ gốc
			else if (_infos.Count != 0 && !Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0))
#else
			else if (_infos.Count != 0)
#endif
			{
				foreach (KeyValuePair<int, TouchInfo> info in _infos)
				{
					TouchInfo value = info.Value;
					if (value.SelectedElement != null && !value.SelectedElement.IsLocked)
					{
						value.SelectedElement.CancelSelectedByAnim();
					}
				}
				_infos.Clear();
			}
		}
	}
}
