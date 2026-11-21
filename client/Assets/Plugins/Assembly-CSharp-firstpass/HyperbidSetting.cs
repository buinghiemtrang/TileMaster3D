using System;
using UnityEngine;

public class HyperbidSetting : ScriptableObject
{
	[Serializable]
	public class AdConfig
	{
		public string AppID = "";

		public string APPKey = "";

		public string AdmobAppID = "";

		public string RewardUnitID = "";

		public string InterstitialID = "";

		public string BannerID = "";

		public string TopBannerID = "";

		public string ApsAppID = "";

		public string ApsRewardUUID = "";

		public string ApsInterstitialUUID = "";

		public string ApsBannerUUID = "";

		public string TopApsBannerUUID = "";
	}

	public AdConfig android = new AdConfig();
}
