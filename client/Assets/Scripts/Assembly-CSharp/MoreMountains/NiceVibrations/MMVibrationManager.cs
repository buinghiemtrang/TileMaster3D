using UnityEngine;

namespace MoreMountains.NiceVibrations
{
	public static class MMVibrationManager
	{
		public static long LightDuration;

		public static long MediumDuration;

		public static long HeavyDuration;

		public static int LightAmplitude;

		public static int MediumAmplitude;

		public static int HeavyAmplitude;

		private static int _sdkVersion;

		private static long[] _successPattern;

		private static int[] _successPatternAmplitude;

		private static long[] _warningPattern;

		private static int[] _warningPatternAmplitude;

		private static long[] _failurePattern;

		private static int[] _failurePatternAmplitude;

		private static AndroidJavaClass UnityPlayer;

		private static AndroidJavaObject CurrentActivity;

		private static AndroidJavaObject AndroidVibrator;

		private static AndroidJavaClass VibrationEffectClass;

		private static AndroidJavaObject VibrationEffect;

		private static int DefaultAmplitude;

		private static bool iOSHapticsInitialized;

		static MMVibrationManager()
		{
			LightDuration = 20L;
			MediumDuration = 40L;
			HeavyDuration = 80L;
			LightAmplitude = 40;
			MediumAmplitude = 120;
			HeavyAmplitude = 255;
			_sdkVersion = -1;
			_successPattern = new long[4] { 0L, LightDuration, LightDuration, HeavyDuration };
			_successPatternAmplitude = new int[4] { 0, LightAmplitude, 0, HeavyAmplitude };
			_warningPattern = new long[4] { 0L, HeavyDuration, LightDuration, MediumDuration };
			_warningPatternAmplitude = new int[4] { 0, HeavyAmplitude, 0, MediumAmplitude };
			_failurePattern = new long[8] { 0L, MediumDuration, LightDuration, MediumDuration, LightDuration, HeavyDuration, LightDuration, LightDuration };
			_failurePatternAmplitude = new int[8] { 0, MediumAmplitude, 0, MediumAmplitude, 0, HeavyAmplitude, 0, LightAmplitude };
#if !UNITY_EDITOR // [rebuild] chặn JNI trong Editor; bản build giữ như gốc
			UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			CurrentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidVibrator = CurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#endif
			iOSHapticsInitialized = false;
		}

		public static bool Android()
		{
			return true;
		}

		public static bool iOS()
		{
			return false;
		}

		public static void Vibrate()
		{
#if UNITY_EDITOR // [rebuild] Editor không có vibrator Android
			return;
#endif
			AndroidVibrate(MediumDuration);
		}

		public static void Haptic(HapticTypes type)
		{
#if UNITY_EDITOR // [rebuild] Editor không có vibrator Android
			return;
#endif
			if (type == HapticTypes.Selection || type == HapticTypes.LightImpact)
			{
				AndroidVibrate(LightDuration, LightAmplitude);
			}
			else if (type == HapticTypes.Success)
			{
				AndroidVibrate(_successPattern, _successPatternAmplitude, -1);
			}
			else if (type == HapticTypes.Warning)
			{
				AndroidVibrate(_warningPattern, _warningPatternAmplitude, -1);
			}
			else if (type == HapticTypes.Failure)
			{
				AndroidVibrate(_failurePattern, _failurePatternAmplitude, -1);
			}
			else if (type == HapticTypes.MediumImpact)
			{
				AndroidVibrate(MediumDuration, MediumAmplitude);
			}
			else if (type == HapticTypes.HeavyImpact)
			{
				AndroidVibrate(HeavyDuration, HeavyAmplitude);
			}
		}

		public static void AndroidVibrate(long milliseconds)
		{
			AndroidVibrator.Call("vibrate", milliseconds);
		}

		public static void AndroidVibrate(long milliseconds, int amplitude)
		{
			if (AndroidSDKVersion() < 26)
			{
				AndroidVibrate(milliseconds);
				return;
			}
			VibrationEffectClassInitialization();
			VibrationEffect = VibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, amplitude);
			AndroidVibrator.Call("vibrate", VibrationEffect);
		}

		public static void AndroidVibrate(long[] pattern, int repeat)
		{
			if (AndroidSDKVersion() < 26)
			{
				AndroidVibrator.Call("vibrate", pattern, repeat);
				return;
			}
			VibrationEffectClassInitialization();
			VibrationEffect = VibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", pattern, repeat);
			AndroidVibrator.Call("vibrate", VibrationEffect);
		}

		public static void AndroidVibrate(long[] pattern, int[] amplitudes, int repeat)
		{
			if (AndroidSDKVersion() < 26)
			{
				AndroidVibrator.Call("vibrate", pattern, repeat);
				return;
			}
			VibrationEffectClassInitialization();
			VibrationEffect = VibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", pattern, amplitudes, repeat);
			AndroidVibrator.Call("vibrate", VibrationEffect);
		}

		public static void AndroidCancelVibrations()
		{
			AndroidVibrator.Call("cancel");
		}

		private static void VibrationEffectClassInitialization()
		{
			if (VibrationEffectClass == null)
			{
				VibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
			}
		}

		public static int AndroidSDKVersion()
		{
			if (_sdkVersion == -1)
			{
				int num = SystemInfo.operatingSystem.IndexOf("-");
				_sdkVersion = int.Parse(SystemInfo.operatingSystem.Substring(num + 1, 3));
			}
			return _sdkVersion;
		}

		private static void InstantiateFeedbackGenerators()
		{
		}

		private static void ReleaseFeedbackGenerators()
		{
		}

		private static void SelectionHaptic()
		{
		}

		private static void SuccessHaptic()
		{
		}

		private static void WarningHaptic()
		{
		}

		private static void FailureHaptic()
		{
		}

		private static void LightImpactHaptic()
		{
		}

		private static void MediumImpactHaptic()
		{
		}

		private static void HeavyImpactHaptic()
		{
		}

		public static void iOSInitializeHaptics()
		{
		}

		public static void iOSReleaseHaptics()
		{
		}

		public static bool HapticsSupported()
		{
			return true;
		}

		public static void iOSTriggerHaptics(HapticTypes type)
		{
		}

		public static string iOSSDKVersion()
		{
			return null;
		}
	}
}
