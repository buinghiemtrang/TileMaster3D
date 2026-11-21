namespace _FrameWork.Http
{
	public class SendServerDataByEvent
	{
		public static ServerDataListRoot<T> SendListInfo<T>(ResultInfo info)
		{
			ServerDataListRoot<T> serverDataListRoot = ParsingServerData.ParsingListData<T>(info);
			DispatcherMgr.Emit(info.Key, serverDataListRoot);
			return serverDataListRoot;
		}

		public static ServerDataRoot<T> SendInfo<T>(ResultInfo info)
		{
			ServerDataRoot<T> serverDataRoot = ParsingServerData.ParsingData<T>(info);
			DispatcherMgr.Emit(info.Key, serverDataRoot);
			return serverDataRoot;
		}

		public static ServerDataRootBase SendInfo(ResultInfo info)
		{
			ServerDataRootBase serverDataRootBase = ParsingServerData.ParsingData(info);
			DispatcherMgr.Emit(info.Key, serverDataRootBase);
			return serverDataRootBase;
		}
	}
}
