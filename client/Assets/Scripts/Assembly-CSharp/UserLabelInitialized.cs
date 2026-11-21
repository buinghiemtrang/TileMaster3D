using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FQDev.UserLabel;

public static class UserLabelInitialized
{
	public static void Initialized(this UserLabelManager self)
	{
		self.Clear();
		self.LoadConfig();
		MethodInfo[] methods = typeof(UserLabelGetters).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
		self.FetchLabels(methods);
		self.RefreshMatchedRule();
		self.LogLabels();
	}

	private static void LoadConfig(this UserLabelManager self)
	{
		self.SetConfig(LoadMgr.LoadJson<UserLabelConfig>("Json/cfgUserGroup"));
	}

	private static void LogLabels(this UserLabelManager self)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, UserFilter> filter in self.Config.Filters)
		{
			if (self.Compare(filter.Value))
			{
				stringBuilder.Append("#").Append(filter.Key);
			}
		}
		LogEvent logEvent = LogEvent.I.Track("").Add("UserLabel", stringBuilder.ToString());
		foreach (KeyValuePair<string, string> fieldLabel in self.FieldLabels)
		{
			if (!string.IsNullOrEmpty(fieldLabel.Value))
			{
				logEvent.Add("Label_" + fieldLabel.Key, fieldLabel.Value);
			}
		}
		logEvent.Add("UserRule", (self.MatchedRule != null) ? self.MatchedRule.Name : "default");
		logEvent.UserSet();
	}
}
