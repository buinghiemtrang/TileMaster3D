using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Collections.Generic;

public static class cf_fail_dialog_const
{
	public static int FailBannerAutoMoveIntervalTime;

	public static List<int> FailBannerOrder;

	public static List<int> FailBannerOrderCoinFull;

	public static int FailBannerOpenLevel;

	public static int FailBannerMaxShowCount;

	public static int StoreGiftMaxShowCount;

	public static void Init()
	{
		Type typeFromHandle = typeof(cf_fail_dialog_const);
		Dictionary<string, object> dictionary = LoadMgr.LoadJson<Dictionary<string, object>>("Json/cf_fail_dialog_const");
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
