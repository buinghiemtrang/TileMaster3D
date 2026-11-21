using System;

public class Singleton<T> where T : class, new()
{
	private static T _ins;

	public static T I
	{
		get
		{
			if (_ins == null)
			{
				_ins = Activator.CreateInstance<T>();
			}
			return _ins;
		}
	}

	public virtual void Destroy()
	{
		_ins = null;
	}
}
