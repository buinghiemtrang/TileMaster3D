using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Collections.Generic;

public static class cf_gift_no_ads_const
{
	public static int IsUseFollowConfig;

	public static int DailyAutoPopMaxCount;

	public static string AutoPopPlayInterstitialCountCondition;

	public static int OutGameAutoPopConditionUnlocklv;

	public static int UnlockNoadsLv;

	public static List<int> TriggerPositions;

	public static void Init()
	{
		Type typeFromHandle = typeof(cf_gift_no_ads_const);
		Dictionary<string, object> dictionary = LoadMgr.LoadJson<Dictionary<string, object>>("Json/cf_gift_no_ads_const");
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
