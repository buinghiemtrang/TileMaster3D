using System.Collections.Generic;
using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;

public class HttpMgrBase : MonoBehaviour
{
	protected const string serverUrl = "https://service.fuqiangame.com";

	protected Dictionary<string, string> headers;

	protected virtual void Awake()
	{
		headers = new Dictionary<string, string>();
	}

	protected virtual ServerDataListRoot<T> SendListInfo<T>(string key, HTTPResponse resp)
	{
		if (resp == null)
		{
			ServerDataListRoot<T> serverDataListRoot = new ServerDataListRoot<T>();
			serverDataListRoot.code = -1;
			serverDataListRoot.msg = "Please check the network！";
			serverDataListRoot.data = new List<T>();
			return serverDataListRoot;
		}
		return JsonConvert.DeserializeObject<ServerDataListRoot<T>>(resp.DataAsText);
	}

	protected virtual ServerDataRoot<T> SendInfo<T>(string key, HTTPResponse resp)
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

	protected virtual ServerDataRootBase SendInfo(string key, HTTPResponse resp)
	{
		if (resp == null)
		{
			ServerDataRootBase serverDataRootBase = new ServerDataRootBase();
			serverDataRootBase.code = -1;
			serverDataRootBase.msg = "Please check the network！";
			return serverDataRootBase;
		}
		return JsonConvert.DeserializeObject<ServerDataRootBase>(resp.DataAsText);
	}
}
