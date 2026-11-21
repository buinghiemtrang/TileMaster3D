using System;
using System.Globalization;
using System.Reflection;
using NativePlatform;
using UnityEngine;

namespace FQDev.UserLabel
{
	public static class UserLabelGetters
	{
		public static string Campaign
		{
			get
			{
				return PlayerPrefs.GetString("UserGroup.sWorth", "");
			}
			set
			{
				if (string.IsNullOrWhiteSpace(Campaign))
				{
					PlayerPrefs.SetString("UserGroup.sWorth", value);
				}
			}
		}

		public static string GetCampaign => Campaign;

		public static int Percentage
		{
			get
			{
				return PlayerPrefs.GetInt("UserGroup.Percentage");
			}
			set
			{
				PlayerPrefs.SetInt("UserGroup.Percentage", value);
			}
		}

		public static string GetMediaSource => GetMediaSourceLabel();

		public static string MediaSource
		{
			get
			{
				return StorageMgr.GetString("UserGroup.MediaSource", "");
			}
			set
			{
				StorageMgr.SetString("UserGroup.MediaSource", value);
			}
		}

		public static bool Organic
		{
			get
			{
				return PlayerPrefs.GetInt("UserGroup.Organic", 0) == 1;
			}
			set
			{
				PlayerPrefs.SetInt("UserGroup.Organic", value ? 1 : 0);
			}
		}

		[UserLabelGetter("region")]
		private static string GetRegionLabel()
		{
			if (!string.IsNullOrEmpty(UserLabelGetCountry.Nation))
			{
				return UserLabelGetCountry.Nation;
			}
			string text = Helper.GetCountryCode();
			if (string.IsNullOrEmpty(text))
			{
				text = RegionInfo.CurrentRegion.Name;
			}
			return text.ToLower();
		}

		[UserLabelGetter("ad_free")]
		private static string GetAdFreeLabel()
		{
			return DB.User.AdFree ? "free" : "";
		}

		[UserLabelGetter("campaign")]
		private static string GetCampaignLabel()
		{
			return Campaign;
		}

		[UserLabelGetter("total_consume")]
		private static string GetCostLevelLabel()
		{
			int num = (int)Math.Round(DB.Track.PayTotalMoney);
			return num.ToString();
		}

		[UserLabelGetter("percentage")]
		private static string GetPercentageLabel()
		{
			if (Percentage == 0)
			{
				Percentage = UnityEngine.Random.Range(1, 100);
			}
			int percentage = Percentage;
			return percentage.ToString();
		}

		[UserLabelGetter("play_day")]
		private static string GetPlayDay()
		{
			int playDays = DB.Track.PlayDays;
			return playDays.ToString();
		}

		[UserLabelGetter("media_source")]
		private static string GetMediaSourceLabel()
		{
			if (string.IsNullOrEmpty(MediaSource))
			{
				return null;
			}
			return MediaSource;
		}

		[UserLabelGetter("is_current_version_new")]
		private static string GetCurrentVersionNewUser()
		{
			if (!DB.Account.IsCurrentVersionNewUser)
			{
				DB.Account.IsCurrentVersionNewUser = DB.Track.OpenAppCount == 0;
				if (DB.Account.IsCurrentVersionNewUser)
				{
					DB.Account.NewUserVersion = Application.version;
				}
			}
			if (!DB.Account.IsCurrentVersionNewUser)
			{
				return "";
			}
			return DB.Account.NewUserVersion;
		}

		[UserLabelGetter("organic")]
		private static string GetOrganicLabel()
		{
			return Organic ? "organic" : null;
		}

		public static void Fetch()
		{
			UserLabelManager.Instance.FetchLabels(typeof(UserLabelGetters).GetMethods(BindingFlags.Static | BindingFlags.NonPublic));
		}
	}
}
