using System;
using System.Collections.Generic;
using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;

namespace Downloaded
{
	public class UpdateVersion
	{
		private const string VersionFile = "version_manifest.json";

		public Action func_progress_add;

		public Action func_progress_del;

		public Action func_finish;

		private string ServerUrl => "https://match3d.oss-accelerate.aliyuncs.com/tilemaster3d/config/android/" + Application.version + "/";

		public void CheckVersion()
		{
			if (!Util.Helper.IsNetworkReachability())
			{
				if (func_finish != null)
				{
					func_finish();
				}
				return;
			}
			HTTPMgr.Request(ServerUrl + "version_manifest.json", delegate(HTTPRequest req, HTTPResponse resp)
			{
				if (resp == null)
				{
					if (func_finish != null)
					{
						func_finish();
					}
				}
				else
				{
					ComparisonVersion(resp.DataAsText);
				}
			}).Send();
		}

		private void ComparisonVersion(string s_msg)
		{
			VersionInfo versionInfo = JsonConvert.DeserializeObject<VersionInfo>(s_msg);
			string text = LoadMgr.LoadText("version_manifest.json", "");
			VersionInfo localInfo;
			if (text == null)
			{
				localInfo = new VersionInfo();
				localInfo.cfgs = new Dictionary<string, string>();
			}
			else
			{
				localInfo = JsonConvert.DeserializeObject<VersionInfo>(text);
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (versionInfo != null && versionInfo.cfgs != null)
			{
				foreach (KeyValuePair<string, string> cfg in versionInfo.cfgs)
				{
					if (!localInfo.cfgs.ContainsKey(cfg.Key) || !localInfo.cfgs[cfg.Key].Equals(cfg.Value))
					{
						dictionary.Add(cfg.Key, cfg.Value);
					}
				}
			}
			CounterMgr.Create(delegate
			{
				Util.File.WriteFile("version_manifest.json", JsonConvert.SerializeObject(localInfo));
				if (func_finish != null)
				{
					func_finish();
				}
			}, true);
			if (dictionary != null)
			{
				foreach (KeyValuePair<string, string> item in dictionary)
				{
					CounterMgr.Add("");
					if (func_progress_add != null)
					{
						func_progress_add();
					}
					HTTPMgr.Request(ServerUrl + item.Key, delegate(HTTPRequest req, HTTPResponse resp)
					{
						if (func_progress_del != null)
						{
							func_progress_del();
						}
						if (resp != null)
						{
							Util.File.WriteFile(item.Key, resp.DataAsText);
							localInfo.cfgs[item.Key] = item.Value;
							DispatcherMgr.Emit("UpdateVersionData", item.Key);
						}
						CounterMgr.Del("");
					}).Send();
				}
			}
			CounterMgr.Finish("");
		}
	}
}
