using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class cf_ads_const
{
	public static int IsOpenVideoWheel;

	public static int SpinsPerVideoLimit;

	public static int OpenLevelVideoWheelLevel;

	public static int VideoWheelTime;

	public static void Init()
	{
		Type typeFromHandle = typeof(cf_ads_const);
		Dictionary<string, object> dictionary = LoadMgr.LoadJson<Dictionary<string, object>>("Json/cf_ads_const");
		FieldInfo[] fields = typeFromHandle.GetFields();
		for (int i = 0; i < fields.Length; i++)
		{
			FieldInfo fieldInfo = fields[i];
			if (!dictionary.ContainsKey(fieldInfo.Name))
			{
				continue;
			}
			if (dictionary[fieldInfo.Name] is JArray || dictionary[fieldInfo.Name] is JObject)
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
}
