using System.Collections.Generic;
using GameScene_Element;
using UnityEngine;

namespace GameScene
{
	public class BackMgr : MonoBehaviour
	{
		private class OperationInfo
		{
			public int id;

			public bool isMove;

			public Element element;
		}

		public static BackMgr I;

		private List<OperationInfo> infos;

		private void Awake()
		{
			I = this;
			infos = new List<OperationInfo>();
		}

		public void Init()
		{
			_clear();
		}

		public bool Judge()
		{
			if (infos.Count == 0)
			{
				return false;
			}
			return !infos[infos.Count - 1].isMove;
		}

		public void Back()
		{
			OperationInfo operationInfo = infos[infos.Count - 1];
			if (operationInfo.element != null)
			{
				PlatformMgr.I.OutAppointElement(operationInfo.element);
			}
			_remove(operationInfo);
		}

		public void Record(Element ele)
		{
			List<OperationInfo> list = infos;
			OperationInfo operationInfo = new OperationInfo();
			operationInfo.element = ele;
			operationInfo.id = ele.Config.id;
			operationInfo.isMove = false;
			list.Add(operationInfo);
			if ((bool)GameMgr.I)
			{
				GameMgr.I.propControl.pPropBack.SetState(false);
			}
		}

		public void StartMove(Element ele)
		{
			OperationInfo operationInfo = _getInfo(ele);
			if (operationInfo != null)
			{
				operationInfo.isMove = true;
			}
		}

		public void StopMove(Element ele)
		{
			OperationInfo operationInfo = _getInfo(ele);
			if (operationInfo != null)
			{
				operationInfo.isMove = false;
			}
		}

		public void Fuse(Element ele)
		{
			OperationInfo operationInfo = _getInfo(ele);
			if (operationInfo != null)
			{
				_remove(operationInfo);
			}
		}

		public void Out(Element ele)
		{
			OperationInfo operationInfo = _getInfo(ele);
			if (operationInfo != null)
			{
				_remove(operationInfo);
			}
		}

		private void _remove(OperationInfo info)
		{
			infos.Remove(info);
			if (infos.Count < 1 && (bool)GameMgr.I)
			{
				GameMgr.I.propControl.pPropBack.SetState(true);
			}
		}

		private void _clear()
		{
			infos.Clear();
			if ((bool)GameMgr.I)
			{
				GameMgr.I.propControl.pPropBack.SetState(true);
			}
		}

		private OperationInfo _getInfo(Element ele)
		{
			int count = infos.Count;
			for (int i = 0; i < count; i++)
			{
				if (infos[i].element == ele)
				{
					return infos[i];
				}
			}
			return null;
		}
	}
}
