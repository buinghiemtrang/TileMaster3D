using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Festival
{
	public class FestivalPrefab : OperationBase
	{
		[Serializable]
		public class PrefabInfo
		{
			public FestivalType type;

			public string name;

			public int index;
		}

		public Transform target;

		public List<PrefabInfo> intos;

		private GameObject originalObj;

		private GameObject _obj;

		protected override void Awake()
		{
			if (originalObj == null && target != null)
			{
				originalObj = target.gameObject;
			}
			DispatcherMgr.On("UPDATE_FESTIVAL", Refresh);
			Refresh();
		}

		protected override void Refresh()
		{
			string realFestivalType = Singleton<FestivalMgr>.I.GetRealFestivalType();
			PrefabInfo prefabInfo = intos.SingleOrDefault(s => s.type.ToString().Equals(realFestivalType));
			if (prefabInfo == null)
			{
				return;
			}
			PointInfo = Singleton<FestivalMgr>.I.GetFestivalInfo(prefabInfo.name);
			if (!Singleton<FestivalMgr>.I.IsShow)
			{
				if (originalObj != null)
				{
					originalObj.SetActive(true);
				}
				if (_obj != null)
				{
					_obj.SetActive(false);
				}
				return;
			}
			if (PointInfo == null)
			{
				Debug.Log("没有找到节日 " + realFestivalType + " 对应的 " + prefabInfo.name + " 预制体");
				return;
			}
			if (_obj == null)
			{
				GameObject gameObject = Resources.Load(PointInfo.information) as GameObject;
				_obj = UnityEngine.Object.Instantiate(gameObject.transform, transform).gameObject;
				_obj.transform.SetSiblingIndex(prefabInfo.index);
			}
			if (target != null)
			{
				originalObj.SetActive(false);
				_obj.SetActive(true);
			}
			else
			{
				_obj.transform.SetActive(true);
			}
		}
	}
}
