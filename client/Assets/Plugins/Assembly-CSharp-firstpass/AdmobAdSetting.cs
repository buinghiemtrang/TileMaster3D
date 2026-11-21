using System;
using UnityEngine;

public class AdmobAdSetting : ScriptableObject
{
	[Serializable]
	public class AdConfig
	{
		public string AppID = "";

		public string RewardUnitID = "";

		public string InterstitialID = "";

		public string BannerID = "";
	}

	public AdConfig android = new AdConfig();

	public AdConfig iOS = new AdConfig();
}
