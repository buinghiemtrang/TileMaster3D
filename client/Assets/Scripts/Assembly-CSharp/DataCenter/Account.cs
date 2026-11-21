using System;
using Newtonsoft.Json;
using UnityEngine;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Account : IAccount
	{
		public const string USERINVALIDNAME = "Player";

		public string GetName()
		{
			if (string.IsNullOrEmpty(name))
			{
				if (!string.IsNullOrEmpty(DefaultName))
				{
					return DefaultName;
				}
				string result = _DefaultName();
				DefaultName = "";
				return result;
			}
			return name;
		}

		public bool IsEditorName()
		{
			return !string.IsNullOrEmpty(name);
		}

		public void RefreshAccount()
		{
			var pAuth = DataCenterLoginMgr.I.pAuth;
			if (pAuth == null)
			{
				return;
			}
			Token = pAuth.token;
			if (Email != pAuth.user.Email)
			{
				Email = pAuth.user.Email;
			}
			if (PhotoUrl != pAuth.user.PhotoUrl)
			{
				PhotoUrl = pAuth.user.PhotoUrl;
			}
			if (Providerid != pAuth.user.ProviderId)
			{
				Providerid = pAuth.user.ProviderId;
			}
			if (Userid != pAuth.user.UserId)
			{
				Userid = pAuth.user.UserId;
			}
			if (Uid != pAuth.user.ExternalId)
			{
				Uid = pAuth.user.ExternalId;
			}
			if (!string.IsNullOrEmpty(pAuth.user.DisplayName) && Name != pAuth.user.DisplayName)
			{
				Name = pAuth.user.DisplayName;
			}
			if (string.IsNullOrEmpty(deviceid) || deviceid != SystemInfo.deviceUniqueIdentifier)
			{
				Deviceid = SystemInfo.deviceUniqueIdentifier;
			}
		}

		public static string _DefaultName()
		{
			string text = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds).ToString();
			if (text.Length > 7)
			{
				text = text.Substring(2);
			}
			return "Player" + text;
		}
	}
}
