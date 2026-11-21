using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameTeam;
using Newtonsoft.Json;
using UnityEngine;

namespace GameMail
{
	public class MailData
	{
		public string Id;

		public string Sender;

		public string Title;

		public string DefaultTitle;

		public string StartTime;

		public string EndTime;

		public string Content;

		public string DefaultContent;

		public string Rewards;

		public int Status;

		public int Type;

		public MailData(EmailServerInfo info)
		{
			Rewards = string.Empty;
			Id = info.emailId;
			Title = FormatEmailText(info.title);
			DefaultTitle = Title;
			Content = FormatEmailText(info.content);
			DefaultContent = Content;
			Sender = "Tile Master 3D";
			Status = info.status;
			Type = 2;
			long localTimeOffsetSeconds = GetLocalTimeOffsetSeconds();
			StartTime = MDateTime.GetDateTimeBySeconds(info.createAt + localTimeOffsetSeconds).ToString("yyyy-MM-dd HH:mm:ss");
			EndTime = MDateTime.GetDateTimeBySeconds(info.expireAt + localTimeOffsetSeconds).ToString("yyyy-MM-dd HH:mm:ss");
			if (!string.IsNullOrEmpty(info.addition))
			{
				EmailServerRewardData emailServerRewardData = JsonConvert.DeserializeObject<EmailServerRewardData>(info.addition);
				if (emailServerRewardData == null)
				{
					Debug.LogError("奖励数据无法正确序列化");
				}
				else
				{
					Rewards = EmailServerRewardData.GetEmailRewardDataList(emailServerRewardData);
				}
			}
		}

		private static string FormatEmailText(string emailText)
		{
			List<string> list = ParseEmailContent(emailText);
			foreach (string text in list)
			{
				string newValue = EmailKeyMapToGameProperty(text);
				emailText = emailText.Replace(text, newValue);
			}
			return emailText;
		}

		private static List<string> ParseEmailContent(string emailContent)
		{
			List<string> list = new List<string>();
			foreach (Match item in Regex.Matches(emailContent, "{.{1,}?}", RegexOptions.Multiline))
			{
				list.Add(item.ToString());
			}
			return list;
		}

		private static string EmailKeyMapToGameProperty(string emailKey)
		{
			string text = emailKey.Replace("{", "").Replace("}", "");
			if (text == "UserName")
			{
				return DB.Account.Name;
			}
			if (text == "n")
			{
				return "\n\r";
			}
			return "{" + text + "}";
		}

		public long GetLocalTimeOffsetSeconds()
		{
			return (long)(DateTime.Now - DateTime.UtcNow).TotalSeconds;
		}

		public MailData()
		{
			Rewards = string.Empty;
		}

		public MailData(cfgMail cfg)
		{
			Rewards = string.Empty;
			Id = cfg.id.ToString();
			Sender = cfg.sender;
			Title = cfg.title;
			DefaultTitle = cfg.defaultTitle;
			Content = cfg.content;
			DefaultContent = cfg.defaultContent;
			Rewards = cfg.rewards.Trim();
			Status = 0;
			Type = 0;
		}

		public MailData(EnergysInfo.EnergyInfo info)
		{
			Rewards = string.Empty;
			Id = info.GUID;
			Sender = info.SendUserName;
			Title = "Language263";
			DefaultTitle = string.Empty;
			Content = "Mail_Life";
			DefaultContent = string.Empty;
			Rewards = string.Format("{0}|{1}", 2, 1);
			Status = 0;
			Type = 1;
		}

		public DateTime GetStartTime()
		{
			return MDateTime.Parse(StartTime);
		}

		public DateTime GetEndTime()
		{
			return MDateTime.Parse(EndTime);
		}

		public MailType GetMailType()
		{
			return (MailType)Type;
		}

		public MailStatus GetMailStatus()
		{
			return (MailStatus)Status;
		}

		public string GetRewards()
		{
			return Rewards.Trim();
		}
	}
}
