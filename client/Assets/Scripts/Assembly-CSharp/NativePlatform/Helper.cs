using System;
using System.Globalization;
using UnityEngine;

namespace NativePlatform
{
	public class Helper : MonoBehaviour
	{
		private static Helper _instance;

		private AndroidJavaObject _jo;

		private void Awake()
		{
			_jo = _getActivityJavaObject();
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}

		public static void Init()
		{
			if (!(_instance != null))
			{
				_instance = new GameObject("PlatformHelper").AddComponent<Helper>();
			}
		}

		private AndroidJavaObject _getActivityJavaObject()
		{
#if UNITY_EDITOR || FAKE_SDK // [rebuild] Editor/FAKE_SDK không có JNI SDK
			return null;
#endif
			if (_jo == null)
			{
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.higgs.tilemaster3d.mUnityPlayerActivity");
				_jo = androidJavaClass.GetStatic<AndroidJavaObject>("instance");
			}
			return _jo;
		}

		public static string GetCountryCode()
		{
			if (!string.IsNullOrEmpty(UserLabelGetCountry.Nation))
			{
				return UserLabelGetCountry.Nation;
			}
			if (_instance._jo != null)
			{
				return _instance._jo.Call<string>("getCountryZipCode", Array.Empty<object>());
			}
			return RegionInfo.CurrentRegion.TwoLetterISORegionName.ToLower();
		}

		public static void CallBack(string param)
		{
		}
	}
}
