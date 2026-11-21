using System;
using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;
using Util;
using _FrameWork.Http;

public class UserLabelGetCountry : Singleton<UserLabelGetCountry>
{
	private class NationInfo
	{
		[JsonProperty("ip")]
		public string Ip;

		[JsonProperty("nation")]
		public string Nation;
	}

	public static string Nation = "";

	private bool _isRequesting;

	private static string GetUserNationUrl => "https://service.fuqiangame.com/utils/api/v1/getUserNation";

	public void RequestCountry()
	{
		if (!_isRequesting)
		{
			Nation = PlayerPrefs.GetString("CountryCode", "");
			_isRequesting = true;
			SingleHttp singleHttp = RequestBase<RepeatRequestsManager>.Instance.Create("User_Label_Get_Country", HTTPMethods.Post, GetUserNationUrl, delegate(ResultInfo result)
			{
				_FrameWork.Http.ServerDataRoot<NationInfo> serverDataRoot = ParsingServerData.ParsingData<NationInfo>(result);
				if (serverDataRoot != null && serverDataRoot.code == 1 && serverDataRoot.data != null)
				{
					_isRequesting = false;
					string text = serverDataRoot.data.Nation.ToLower();
					if (Nation != text)
					{
						Debug.Log("Change CountryCode:" + text);
						PlayerPrefs.SetString("CountryCode", text);
						Nation = text;
						Global.NeedReloadConfig = true;
					}
					Debug.Log("ip:" + serverDataRoot.data.Ip + "  nation:" + serverDataRoot.data.Nation);
				}
			}, -1, isSuccessRemoveRequest: true, isAbortRemoveRequest: true, 10f);
			singleHttp.AddCustomTimeout(5f);
			singleHttp.AddHeader("Content-Type", "application/json");
			singleHttp.AddHeader("timestamp", Helper.DateTimeToTimeStamp(DateTime.Now).ToString());
			singleHttp.Send();
		}
	}
}
