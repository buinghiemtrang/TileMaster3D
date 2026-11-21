using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace _FrameWork.Http
{
	public class ParsingServerData
	{
		public static ServerDataListRoot<T> ParsingListData<T>(ResultInfo info)
		{
			ServerDataListRoot<T> serverDataListRoot = null;
			if (info.State == RequestState.Success)
			{
				Debug.Log(info.Response.DataAsText);
				serverDataListRoot = JsonConvert.DeserializeObject<ServerDataListRoot<T>>(info.Response.DataAsText);
			}
			if (serverDataListRoot == null)
			{
				serverDataListRoot = new ServerDataListRoot<T>();
				serverDataListRoot.code = -1;
				serverDataListRoot.msg = string.Format("http error. Key:{0}  State:{1}", info.Key, info.State);
				serverDataListRoot.data = new List<T>();
			}
			serverDataListRoot.tag = info.Tag;
			if (serverDataListRoot.code != 1)
			{
				Debug.Log(string.Format("Request own server. code:{0} error:{1}", serverDataListRoot.code, serverDataListRoot.msg));
			}
			return serverDataListRoot;
		}

		public static ServerDataRoot<T> ParsingData<T>(ResultInfo info)
		{
			ServerDataRoot<T> serverDataRoot = null;
			if (info.State == RequestState.Success)
			{
				Debug.Log(info.Response.DataAsText);
				serverDataRoot = JsonConvert.DeserializeObject<ServerDataRoot<T>>(info.Response.DataAsText);
			}
			if (serverDataRoot == null)
			{
				serverDataRoot = new ServerDataRoot<T>();
				serverDataRoot.code = -1;
				serverDataRoot.msg = string.Format("http error. Key:{0}  State:{1}", info.Key, info.State);
				serverDataRoot.data = default(T);
			}
			serverDataRoot.tag = info.Tag;
			if (serverDataRoot.code != 1)
			{
				Debug.Log(string.Format("Request own server. code:{0}   error:{1}", serverDataRoot.code, serverDataRoot.msg));
			}
			return serverDataRoot;
		}

		public static ServerDataRootBase ParsingData(ResultInfo info)
		{
			ServerDataRootBase serverDataRootBase = null;
			if (info.State == RequestState.Success)
			{
				Debug.Log(info.Response.DataAsText);
				serverDataRootBase = JsonConvert.DeserializeObject<ServerDataRootBase>(info.Response.DataAsText);
			}
			if (serverDataRootBase == null)
			{
				serverDataRootBase = new ServerDataRootBase();
				serverDataRootBase.code = -1;
				serverDataRootBase.msg = string.Format("http error. Key:{0}  State:{1}", info.Key, info.State);
			}
			serverDataRootBase.tag = info.Tag;
			if (serverDataRootBase.code != 1)
			{
				Debug.Log(string.Format("Request own server. code:{0}   error:{1}", serverDataRootBase.code, serverDataRootBase.msg));
			}
			return serverDataRootBase;
		}
	}
}
