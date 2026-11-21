using System;
using System.Collections.Generic;
using BestHTTP;
using DataCenter;
using Newtonsoft.Json;
using System.Text;
using UnityEngine;

public class DataManager : AbsDispatchServlet
{
	private string _sToken;

	private Uri _urlHot;

	private readonly Dictionary<string, object> _dictPostData = new Dictionary<string, object>();

	public static DataManager I { get; } = new DataManager();

	public string Token => _sToken;

	public void SetUrl(string sURL)
	{
		_urlHot = new Uri(sURL);
	}

	public override HTTPRequest Post(string dbname, DataInfo info, Action<bool> cbFinish)
	{
		_dictPostData.Clear();
		foreach (KeyValuePair<string, object> datum in info.data)
		{
			// UNVERIFIED: separator literal prints as SL____ in pseudo-C ('.' or ','); chose '.'
			_dictPostData.Add(dbname + "." + datum.Key, datum.Value);
		}
		return _post(JsonConvert.SerializeObject(_dictPostData), cbFinish);
	}

	private HTTPRequest _post(string body, Action<bool> cbFinish)
	{
		HTTPRequest hTTPRequest = DataCenter.HttpManager.Post(_urlHot, delegate(HTTPRequest req, HTTPResponse resp)
		{
			if (resp == null)
			{
				if (cbFinish != null)
				{
					cbFinish(false);
				}
			}
			else
			{
				DataInfo dataInfo = JsonConvert.DeserializeObject<DataInfo>(resp.DataAsText);
				if (cbFinish != null)
				{
					cbFinish(dataInfo.code == 1);
				}
			}
		});
		hTTPRequest.AddHeader("client-version", Application.version);
		hTTPRequest.AddHeader("Content-Type", "application/json");
		hTTPRequest.AddHeader("Authorization", "Bearer " + _sToken);
		hTTPRequest.RawData = Encoding.UTF8.GetBytes(body);
		hTTPRequest.Send();
		return hTTPRequest;
	}

	public HTTPRequest HttpGet(Action<Dictionary<string, object>, string> cbFinish)
	{
		HTTPRequest hTTPRequest = DataCenter.HttpManager.Get(_urlHot, delegate(HTTPRequest req, HTTPResponse resp)
		{
			if (resp == null)
			{
				if (cbFinish != null)
				{
					cbFinish(null, string.Format("HttpGet err req.state:{0}", req.State));
				}
			}
			else
			{
				DataInfo dataInfo = JsonConvert.DeserializeObject<DataInfo>(resp.DataAsText);
				if (cbFinish != null)
				{
					cbFinish((dataInfo.code == 1) ? dataInfo.data : null, string.Format("code:{0}.{1}", dataInfo.code, dataInfo.msg));
				}
			}
		});
		hTTPRequest.AddHeader("client-version", Application.version);
		hTTPRequest.AddHeader("Content-Type", "application/json");
		hTTPRequest.AddHeader("Authorization", "Bearer " + _sToken);
		hTTPRequest.Send();
		return hTTPRequest;
	}

	public void Logined(string token, string userid)
	{
		base.bLogined = true;
		_sToken = token;
		sUid = userid;
		foreach (IAssembleDB value in dictCls.Values)
		{
			value.InitQueueDispatch(userid);
		}
		LogEvent.I.Track().Add("LoginUser", DB.Account.LoginPlatform).UserSet();
	}

	public void Logout()
	{
		CLearData();
		LogEvent.I.Track().Add("LoginUser", "Null").UserSet();
	}

	public void CLearData()
	{
		base.bLogined = false;
		_sToken = string.Empty;
		sUid = string.Empty;
		DequeueDisable();
	}

	public void LoadLocalData()
	{
		foreach (IAssembleDB value in dictCls.Values)
		{
			value.ReadLocalData();
		}
	}

	public void LoadRemoteData(Dictionary<string, object> data)
	{
		foreach (IAssembleDB value in dictCls.Values)
		{
			value.ReadLocalData();
			if (data.ContainsKey(value.GetName()))
			{
				value.ReadRemoteData(data[value.GetName()].ToString());
			}
		}
	}

	public void DequeueEnabled()
	{
		if (!base.bLogined)
		{
			return;
		}
		foreach (IAssembleDB value in dictCls.Values)
		{
			value.DequeueEnabled();
		}
	}

	public void DequeueDisable()
	{
		if (!base.bLogined)
		{
			return;
		}
		foreach (IAssembleDB value in dictCls.Values)
		{
			value.DequeueDisable();
		}
	}

	public void LoadData(Action callback)
	{
		if (base.bLogined)
		{
			HttpGet(FuncFinish);
			return;
		}
		FuncFinish(null, null);
		void FuncFinish(Dictionary<string, object> data, string msg)
		{
			if (data == null)
			{
				LoadLocalData();
			}
			else
			{
				LoadRemoteData(data);
			}
			if (callback != null)
			{
				callback();
			}
			DequeueEnabled();
		}
	}

	public void WriteLocalDataAll()
	{
		foreach (IAssembleDB value in dictCls.Values)
		{
			value.WriteLocalData();
		}
	}

	public void DelLocalDataAll()
	{
		foreach (IAssembleDB value in dictCls.Values)
		{
			value.DelLocalData();
		}
	}

	public void InitLocalDataAll()
	{
		foreach (IAssembleDB value in dictCls.Values)
		{
			value.InitLocalData();
		}
	}

	public void UploadDataAllToRemote()
	{
		foreach (IAssembleDB value in dictCls.Values)
		{
			value.UploadDataAllToRemote();
		}
	}
}
