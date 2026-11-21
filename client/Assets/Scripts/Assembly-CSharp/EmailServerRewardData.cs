using System.Collections.Generic;

public class EmailServerRewardData
{
	public List<EmailServerRewardInfo> list;

	public static string GetEmailRewardDataList(EmailServerRewardData data)
	{
		string text = "";
		foreach (EmailServerRewardInfo item in data.list)
		{
			text = text + item.id + "|" + item.count + ",";
		}
		return text.Substring(0, text.Length - 1);
	}
}
