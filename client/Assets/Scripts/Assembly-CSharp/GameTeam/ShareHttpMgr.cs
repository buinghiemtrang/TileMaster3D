using BestHTTP;

namespace GameTeam
{
	public class ShareHttpMgr : HttpMgrBase
	{
		public const string GETMYTEAMKEY = "TeamHttp_GetMyTeam";

		public const string GETENERGYKEY = "TeamHttp_GetEnergy";

		public void GetMyTeam(string key = "TeamHttp_GetMyTeam")
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			OwnHttpMgr.I.SendGet("https://service.fuqiangame.com/team/GetMyTeam", headers, (resp) =>
			{
				SendInfo<TeamInfo>(key, resp);
			}, key);
		}

		public void GetEnergy(string key = "TeamHttp_GetEnergy")
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			OwnHttpMgr.I.SendGet("https://service.fuqiangame.com/team/GetLives", headers, (resp) =>
			{
				SendListInfo<EnergysInfo>(key, resp);
			}, key);
		}

		protected override ServerDataListRoot<T> SendListInfo<T>(string key, HTTPResponse resp)
		{
			ServerDataListRoot<T> serverDataListRoot = base.SendListInfo<T>(key, resp);
			DispatcherMgr.Emit(key, serverDataListRoot);
			return serverDataListRoot;
		}

		protected override ServerDataRoot<T> SendInfo<T>(string key, HTTPResponse resp)
		{
			ServerDataRoot<T> serverDataRoot = base.SendInfo<T>(key, resp);
			DispatcherMgr.Emit(key, serverDataRoot);
			return serverDataRoot;
		}

		protected override ServerDataRootBase SendInfo(string key, HTTPResponse resp)
		{
			ServerDataRootBase serverDataRootBase = base.SendInfo(key, resp);
			DispatcherMgr.Emit(key, serverDataRootBase);
			return serverDataRootBase;
		}
	}
}
