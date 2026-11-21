using System;
using UnityEngine;

public class IronsourceSetting : ScriptableObject
{
	[Serializable]
	public class AdConfig
	{
		public string AppID = "";

		public string AdmobAppID = "";
	}

	public AdConfig android = new AdConfig();

	public AdConfig iOS = new AdConfig();
}
