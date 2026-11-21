using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class cf_regionRank_const
{
	public static int StartLevelGroup;

	public static List<string> RankingAddGroup;

	public static int IsOpen;

	public static int PreviewLevel;

	public static int UnlockLevel;

	public static string FirstStartTime;

	public static int ContinueTime;

	public static int CoolTime;

	public static int CrownModelId;

	public static List<string> WinningStreakGenerateModel;

	public static int NationalRewardMinScore;

	public static int ChallengeCount;

	public static int FailReduceLevelOrder;

	public static void Init()
	{
		Type typeFromHandle = typeof(cf_regionRank_const);
		Dictionary<string, object> dictionary = LoadMgr.LoadJson<Dictionary<string, object>>("Json/cf_regionRank_const");
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
