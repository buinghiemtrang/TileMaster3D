using System;
using UnityEngine;

public static class SuperTimer
{
	private static AndroidJavaObject _jo;

	private static AndroidJavaObject jo
	{
		get
		{
			if (_jo == null)
			{
				_jo = new AndroidJavaObject("com.pushangame.andriodtime.TimeActivity");
			}
			return _jo;
		}
	}

	public static int GetDeviceRunTime()
	{
#if UNITY_EDITOR // [rebuild] Editor không có JNI
		return (int)Math.Floor(Time.realtimeSinceStartup);
#elif FAKE_SDK // [rebuild] không có lớp TimeActivity của SDK → dùng thẳng SystemClock.elapsedRealtime (cùng giá trị gốc trả về)
		using (AndroidJavaClass clock = new AndroidJavaClass("android.os.SystemClock"))
		{
			return (int)Math.Floor((float)clock.CallStatic<long>("elapsedRealtime") * 0.001f);
		}
#else
		return (int)Math.Floor((float)jo.CallStatic<long>("GetElapsedRealtime") * 0.001f);
#endif
	}
}
