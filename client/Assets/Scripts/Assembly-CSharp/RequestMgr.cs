using System.Collections.Generic;
using BestHTTP;
using Newtonsoft.Json;

public static class RequestMgr
{
	public const string ServerUrl = "https://service.fuqiangame.com";

	public static Dictionary<string, string> Headers = new Dictionary<string, string>();

	public static ServerDataListRoot<T> SendListInfo<T>(HTTPResponse resp)
	{
		if (resp == null)
		{
			ServerDataListRoot<T> serverDataListRoot = new ServerDataListRoot<T>();
			serverDataListRoot.msg = "Please check the network！";
			serverDataListRoot.code = -1;
			serverDataListRoot.data = new List<T>();
			return serverDataListRoot;
		}
		return JsonConvert.DeserializeObject<ServerDataListRoot<T>>(resp.DataAsText);
	}

	public static ServerDataRoot<T> SendInfo<T>(HTTPResponse resp)
	{
		if (resp == null)
		{
			ServerDataRoot<T> serverDataRoot = new ServerDataRoot<T>();
			serverDataRoot.code = -1;
			serverDataRoot.msg = "Please check the network！";
			serverDataRoot.data = default(T);
			return serverDataRoot;
		}
		return JsonConvert.DeserializeObject<ServerDataRoot<T>>(resp.DataAsText);
	}

	public static ServerDataRootBase SendInfo(HTTPResponse resp)
	{
		if (resp == null)
		{
			ServerDataRootBase serverDataRootBase = new ServerDataRootBase();
			serverDataRootBase.msg = "Please check the network！";
			serverDataRootBase.code = -1;
			return serverDataRootBase;
		}
		return JsonConvert.DeserializeObject<ServerDataRootBase>(resp.DataAsText);
	}
}
