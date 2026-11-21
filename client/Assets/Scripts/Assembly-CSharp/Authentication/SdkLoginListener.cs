using System;
using UnityEngine;

namespace Authentication
{
	public class SdkLoginListener : MonoBehaviour
	{
		public Action<bool, PlatformType, string> onFinished;

		public void LoginCall(string param)
		{
			string[] array = param.Split('|');
			bool flag = array[1].Equals("succ");
			PlatformType platformType;
			if (array[2].Equals("facebook"))
			{
				platformType = PlatformType.Facebook;
			}
			else if (array[2].Equals("google"))
			{
				platformType = PlatformType.Google;
			}
			else
			{
				platformType = PlatformType.Apple;
				if (flag && !string.IsNullOrEmpty(array[4]))
				{
					PlayerPrefs.SetString("Apple_user_displayName", array[4]);
				}
			}
			if (!flag)
			{
				string errorMessage = (array.Length < 4 || string.IsNullOrEmpty(array[3])) ? "login fail message is empty" : array[3];
				LogEventLogin.LogInError(platformType.ToString(), errorMessage);
			}
			if (onFinished != null)
			{
				onFinished(flag, platformType, flag ? array[3] : string.Empty);
			}
		}
	}
}
