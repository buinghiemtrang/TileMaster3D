using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using DataCenter;
using FQDev.UserLabel;
using Newtonsoft.Json;
using UnityEngine;
using Util;

public class SDKAppsFlyerDelegate : SingletonMono<SDKAppsFlyerDelegate>, AppsflyerConversionDelegate
{
	private Coroutine coroutine;

	public void ManualGetConversionData(string uid)
	{
		coroutine = StartCoroutine(IEManualGetConversionData(uid));
	}

	private IEnumerator IEManualGetConversionData(string uid)
	{
		yield return new WaitForSeconds(5f);
		Debug.Log("-----IEManualGetConversionData uid:" + uid);
		string deviceId = uid;
		string uriString = string.Concat("https://gcdsdk.appsflyer.com/install_data/v4.0/", "com.higgs.tilemaster3d", "?devkey=", "X3sgfYhYXWhDoD8DhW2aaJ", "&device_id=", deviceId);
		HTTPRequest hTTPRequest = HttpManager.Get(new Uri(uriString), delegate(HTTPRequest req, HTTPResponse resp)
		{
			if (resp != null)
			{
				OnConversionDataSuccess(resp.DataAsText, "PushApi");
				Debug.Log("-----AppsflyerManualConversion :" + resp.DataAsText + " deviceId:" + deviceId);
			}
		});
		hTTPRequest.AddHeader("accept", "application/json");
		hTTPRequest.Send();
	}

	public void OnConversionDataSuccess(string conversionData, string getType = "SDK")
	{
		if (string.IsNullOrEmpty(conversionData))
		{
			return;
		}
		Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(conversionData);
		if (dictionary == null || LogEvent.I == null)
		{
			return;
		}
		string text = "organic";
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
		}
		string afId = AppsflyerSDK.GetAfId();
		if (dictionary.ContainsKey("campaign"))
		{
			string text2 = Util.Object.AsString(dictionary["campaign"]);
			if (string.IsNullOrEmpty(UserLabelGetters.Campaign))
			{
				Global.NeedReloadConfig = true;
			}
			UserLabelGetters.Campaign = text2.ToLower();
			LogEvent.I.Track("").Add("Label_campaign", UserLabelGetters.Campaign).UserSet();
			if (!LogEvent.I.IsLogCampaign())
			{
				LogEvent.I.SetLogCampaign();
				LogEvent.I.LogCampaign(text2, afId, getType);
			}
		}
		if (dictionary.TryGetValue("media_source", out var value))
		{
			text = value as string;
			if (string.IsNullOrEmpty(UserLabelGetters.MediaSource))
			{
				UserLabelGetters.MediaSource = text;
				Global.NeedReloadConfig = true;
			}
			if (!LogEvent.I.IsLogChannel())
			{
				LogEvent.I.SetLogChannel();
				LogEvent.I.LogChannel(text, afId, getType);
			}
			LogEvent.I.AF_Media(text);
		}
		if (text != null)
		{
			UserLabelGetters.Organic = text.ToLower().Contains("organic");
		}
		if (dictionary.ContainsKey("is_first_launch") && dictionary.ContainsKey("media_source") && Util.Object.AsBool(dictionary["is_first_launch"]))
		{
			LogEvent.I.Track("").Add("Source", dictionary["media_source"]).UserSet();
		}
	}

	public void OnConversionDataFail(string error)
	{
	}

	public void OnAppOpenAttribution(string attributionData)
	{
	}

	public void OnAppOpenAttributionFailure(string error)
	{
	}
}
