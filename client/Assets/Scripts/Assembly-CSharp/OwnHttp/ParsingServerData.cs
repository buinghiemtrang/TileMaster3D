using System.Collections.Generic;
using Language;
using Newtonsoft.Json;

namespace OwnHttp
{
	public static class ParsingServerData
	{
		public const string ServerUrl = "https://service.fuqiangame.com";

		public const float Timeout = 5f;

		public static ServerDataListRoot<T> SendListInfo<T>(ResultInfo info)
		{
			if (info.State == RequestState.Fail)
			{
				ServerDataListRoot<T> serverDataListRoot = new ServerDataListRoot<T>();
				serverDataListRoot.code = -1;
				serverDataListRoot.msg = _RequestFailWord();
				serverDataListRoot.Tag = info.Tag;
				serverDataListRoot.Data = new List<T>();
				return serverDataListRoot;
			}
			if (info.State != RequestState.Success)
			{
				return null;
			}
			ServerDataListRoot<T> serverDataListRoot2 = JsonConvert.DeserializeObject<ServerDataListRoot<T>>(info.Response.DataAsText);
			if (serverDataListRoot2 != null)
			{
				serverDataListRoot2.Tag = info.Tag;
			}
			return serverDataListRoot2;
		}

		public static ServerDataRoot<T> SendInfo<T>(ResultInfo info)
		{
			if (info.State == RequestState.Fail)
			{
				ServerDataRoot<T> serverDataRoot = new ServerDataRoot<T>();
				serverDataRoot.code = -1;
				serverDataRoot.msg = _RequestFailWord();
				serverDataRoot.Tag = info.Tag;
				serverDataRoot.Data = default(T);
				return serverDataRoot;
			}
			if (info.State != RequestState.Success)
			{
				return null;
			}
			ServerDataRoot<T> serverDataRoot2 = JsonConvert.DeserializeObject<ServerDataRoot<T>>(info.Response.DataAsText);
			if (serverDataRoot2 != null)
			{
				serverDataRoot2.Tag = info.Tag;
			}
			return serverDataRoot2;
		}

		public static ServerDataRootBase SendInfo(ResultInfo info)
		{
			if (info.State == RequestState.Fail)
			{
				ServerDataRootBase serverDataRootBase = new ServerDataRootBase();
				serverDataRootBase.code = -1;
				serverDataRootBase.msg = _RequestFailWord();
				serverDataRootBase.Tag = info.Tag;
				return serverDataRootBase;
			}
			if (info.State != RequestState.Success)
			{
				return null;
			}
			ServerDataRootBase serverDataRootBase2 = JsonConvert.DeserializeObject<ServerDataRootBase>(info.Response.DataAsText);
			if (serverDataRootBase2 != null)
			{
				serverDataRootBase2.Tag = info.Tag;
			}
			return serverDataRootBase2;
		}

		private static string _RequestFailWord()
		{
			string text = "Please check the network！";
			if (LanguageManager.Instance != null)
			{
				LanguageData languageDataByKey = LanguageManager.Instance.GetLanguageDataByKey("Language55");
				if (languageDataByKey != null)
				{
					text = languageDataByKey.value;
				}
			}
			return text;
		}
	}
}
