using System;
using UnityEngine;

public class AppLovinMaxSetting : ScriptableObject
{
	[Serializable]
	public class AdConfig
	{
		public string AppID = "";

		public string AdmobAppID = "";

		public string RewardUnitID = "";

		public string InterstitialID = "";

		public string BannerID = "";

		public string TopBannerID = "";

		public string SDKID = "";

		public string ApsAppID = "";

		public string ApsRewardUUID = "";

		public string ApsInterstitialUUID = "";

		public string ApsBannerUUID = "";

		public string TopApsBannerUUID = "";
	}

	public AdConfig android = new AdConfig();

	public AdConfig iOS = new AdConfig();
}
