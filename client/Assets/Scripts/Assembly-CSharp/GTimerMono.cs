using UnityEngine;

public class GTimerMono : MonoBehaviour
{
	private static GTimerMono instance;

	private int pauseSeconds;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
		GSingleton<GTimer>.ins.update();
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			Logs.Common.Info("GTimer OnApplicationPause pauseSeconds: " + pauseSeconds);
			pauseSeconds = SuperTimer.GetDeviceRunTime();
		}
		else if (pauseSeconds > 0)
		{
			int deviceRunTime = SuperTimer.GetDeviceRunTime();
			int num = deviceRunTime - pauseSeconds;
			Logs.Common.Info(string.Format("GTimer OnApplicationPause pauseSeconds: {0} nowSeconds: {1} deltaSeconds: {2} ", pauseSeconds, deviceRunTime, num));
			GSingleton<GTimer>.ins.onPauseResume?.Invoke(num);
		}
	}
}
