using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using DG.Tweening;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class InitManager : MonoBehaviour
{
	public Transform tfMainRoot;

	public Transform tfTopRoot;

	public Transform tfSuperRoot;

	public Transform tfMainRoot3D;

	public Transform tfTopRoot3D;

	public string sLoadScene = "";

	private void Awake()
	{
		CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
#if !UNITY_EDITOR && !FAKE_SDK // [rebuild] gốc tắt log mọi nơi; giữ log trong Editor + bản test FAKE_SDK để debug
		Debug.unityLogger.logEnabled = false;
#endif
		Input.multiTouchEnabled = true;
		Application.targetFrameRate = 60;
		Logs.Init();
		DOTween.SetTweensCapacity(500, 50);
		SceneMgr.tfMainRoot = tfMainRoot;
		SceneMgr.tfTopRoot = tfTopRoot;
		SceneMgr.tfSuperRoot = tfSuperRoot;
		SceneMgr.tfMainRoot3D = tfMainRoot3D;
		SceneMgr.tfTopRoot3D = tfTopRoot3D;
		SceneMgr.SetBasicValues(GameObject.Find("Main Canvas").transform);
		DispatcherMgr.OnG<string>("UpdateVersionData", UpdateVersionData);
	}

	private void Start()
	{
		SceneMgr.LoadScene(sLoadScene);
	}

	private void OnApplicationPause(bool pause)
	{
		AdvertisingMgr.I.UnfocusBackPlay(pause);
	}

	private void UpdateVersionData(string file)
	{
		if (file != "Json/cf_const" && file != "Json2/cf_const")
		{
			return;
		}
		Type typeFromHandle = typeof(cf_const);
		Dictionary<string, object> dictionary = LoadMgr.LoadJson<Dictionary<string, object>>("Json/cf_const");
		FieldInfo[] fields = typeFromHandle.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!dictionary.ContainsKey(fieldInfo.Name))
			{
				continue;
			}
			object obj = dictionary[fieldInfo.Name];
			if (obj is JArray || obj is JObject)
			{
				fieldInfo.SetValue(typeFromHandle, JsonConvert.DeserializeObject(dictionary[fieldInfo.Name].ToString(), fieldInfo.FieldType));
			}
			else if (fieldInfo.FieldType.Name == "Int32")
			{
				fieldInfo.SetValue(typeFromHandle, Convert.ToInt32(dictionary[fieldInfo.Name]));
			}
			else if (fieldInfo.FieldType.Name == "Single")
			{
				fieldInfo.SetValue(typeFromHandle, Convert.ToSingle(dictionary[fieldInfo.Name]));
			}
			else
			{
				fieldInfo.SetValue(typeFromHandle, dictionary[fieldInfo.Name]);
			}
		}
	}

	private void OnApplicationQuit()
	{
		LogEvent.I.AddChannelWaitTime();
	}

	private void OnDestroy()
	{
		Logs.Destroy();
	}
}
