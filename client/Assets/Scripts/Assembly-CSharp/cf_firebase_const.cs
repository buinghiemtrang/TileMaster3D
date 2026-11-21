using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Collections.Generic;

public static class cf_firebase_const
{
	public static List<string> ad_impression_multiple;

	public static List<string> purchase_multiple;

	public static List<string> ad_total_value_multiple;

	public static string cum_version;

	public static string cum_log_name;

	public static float cum_rev_startValue;

	public static float cum_rev_addValue;

	public static float cum_rev_changeValue;

	public static float cum_rev_changeAddValue;

	public static float cum_rev_max;

	public static Dictionary<string, float> ad_revenue_main;

	public static List<string> ad_revenue_source_media;

	public static Dictionary<string, float> ad_purchase_revenue_main;

	public static List<string> ad_purchase_revenue_source_media;

	public static string ad_revenue_tidu_version;

	public static float ad_revenue_001_tidu_001_start;

	public static float ad_revenue_001_tidu_001_interval;

	public static float ad_revenue_01_tidu_03_start;

	public static float ad_revenue_01_tidu_03_interval;

	public static void Init()
	{
		Type typeFromHandle = typeof(cf_firebase_const);
		Dictionary<string, object> dictionary = LoadMgr.LoadJson<Dictionary<string, object>>("Json/cf_firebase_const");
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
