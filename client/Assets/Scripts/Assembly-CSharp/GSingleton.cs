public abstract class GSingleton<T> : ISetup where T : class, ISetup, new()
{
	private static readonly object lockObj = new object();

	private static T _ins;

	public static T ins
	{
		get
		{
			if (_ins == null)
			{
				lock (lockObj)
				{
					if (_ins == null)
					{
						_ins = new T();
						_ins.Setup();
						Logs.Common.Info("=====>[ " + typeof(T).ToString() + " ] Setup");
					}
				}
			}
			return _ins;
		}
	}

	public abstract void Setup();
}
