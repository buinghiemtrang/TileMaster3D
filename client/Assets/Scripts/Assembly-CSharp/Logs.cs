using FQDev.Logger;

public static class Logs
{
	public static Logger Common => LogManager.Common;

	public static Logger Net { get; private set; }

	public static void Init()
	{
		UnityLogAdaptation.Initialize();
		Net = LogManager.CreateLogger("Net", LogLevel.Info);
	}

	public static void Destroy()
	{
		UnityLogAdaptation.Destroy();
		LogManager.RemoveLogger(Net);
	}
}
