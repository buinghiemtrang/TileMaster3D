using UnityEngine;

namespace Festival
{
	public class OperationBase : MonoBehaviour
	{
		[SerializeField]
		protected string pointKey = string.Empty;

		protected cfgFestivalPointInformation PointInfo;

		protected virtual void Awake()
		{
			PointInfo = Singleton<FestivalMgr>.I.GetFestivalInfo(pointKey);
			DispatcherMgr.On("UPDATE_FESTIVAL", Refresh);
			Refresh();
		}

		protected virtual void Refresh()
		{
		}

		protected virtual void OnDestroy()
		{
			DispatcherMgr.Off("UPDATE_FESTIVAL", Refresh);
		}
	}
}
