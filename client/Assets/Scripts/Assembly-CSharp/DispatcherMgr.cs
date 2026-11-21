using System;
using System.Collections.Generic;

public static class DispatcherMgr
{
	private static int auto_increment = 0;

	private static Dictionary<string, List<Delegate>> gmethods = new Dictionary<string, List<Delegate>>();

	private static Dictionary<string, List<Delegate>> methods = new Dictionary<string, List<Delegate>>();

	private static Dictionary<string, List<Delegate>> methods_once = new Dictionary<string, List<Delegate>>();

	private static Dictionary<string, Func<object, object>> methodsback = new Dictionary<string, Func<object, object>>();

	private static void AddMethod(string key, Delegate method)
	{
		if (methods.ContainsKey(key))
		{
			if (methods[key].IndexOf(method) == -1)
			{
				methods[key].Add(method);
			}
		}
		else
		{
			List<Delegate> list = new List<Delegate>();
			list.Add(method);
			methods[key] = list;
		}
	}

	private static void AddMethodOnce(string key, Delegate method)
	{
		if (methods_once.ContainsKey(key))
		{
			methods_once[key].Add(method);
		}
		else
		{
			List<Delegate> list = new List<Delegate>();
			list.Add(method);
			methods_once[key] = list;
		}
	}

	private static void AddMethodBack(string key, Func<object, object> method)
	{
		if (!methodsback.ContainsKey(key))
		{
			methodsback.Add(key, method);
		}
	}

	private static void DelMethod(string key, Delegate method)
	{
		if (methods.ContainsKey(key))
		{
			methods[key].Remove(method);
			if (methods[key].Count == 0)
			{
				methods.Remove(key);
			}
		}
	}

	private static void AddMethodG(string key, Delegate method)
	{
		if (gmethods.ContainsKey(key))
		{
			if (gmethods[key].IndexOf(method) == -1)
			{
				gmethods[key].Add(method);
			}
		}
		else
		{
			List<Delegate> list = new List<Delegate>();
			list.Add(method);
			gmethods[key] = list;
		}
	}

	private static void DelMethodG(string key, Delegate method)
	{
		if (gmethods.ContainsKey(key))
		{
			gmethods[key].Remove(method);
			if (gmethods[key].Count == 0)
			{
				gmethods.Remove(key);
			}
		}
	}

	public static void Clear()
	{
		methods.Clear();
		methods_once.Clear();
		methodsback.Clear();
	}

	public static void Emit(string key, params object[] param)
	{
		if (gmethods.ContainsKey(key))
		{
			int length = param.Length;
			List<Delegate> list = gmethods[key];
			for (int num = list.Count - 1; num >= 0; num--)
			{
				Delegate obj = list[num];
				if (obj.Method.GetParameters().Length == length)
				{
					obj.DynamicInvoke(param);
				}
			}
		}
		if (methods_once.ContainsKey(key))
		{
			int length2 = param.Length;
			List<Delegate> list2 = methods_once[key];
			for (int num2 = list2.Count - 1; num2 >= 0; num2--)
			{
				Delegate obj2 = list2[num2];
				if (obj2.Method.GetParameters().Length == length2)
				{
					list2[num2] = null;
					obj2.DynamicInvoke(param);
				}
			}
			for (int num3 = list2.Count - 1; num3 >= 0; num3--)
			{
				if (list2[num3] == null)
				{
					list2.RemoveAt(num3);
				}
			}
			if (list2.Count == 0)
			{
				methods_once.Remove(key);
			}
		}
		if (!methods.ContainsKey(key))
		{
			return;
		}
		int length3 = param.Length;
		List<Delegate> list3 = methods[key];
		for (int num4 = list3.Count - 1; num4 >= 0; num4--)
		{
			Delegate obj3 = list3[num4];
			if (obj3.Method.GetParameters().Length == length3)
			{
				obj3.DynamicInvoke(param);
			}
		}
	}

	public static T EmitB<T>(string key, object obj = null)
	{
		if (methodsback.ContainsKey(key))
		{
			return (T)methodsback[key].DynamicInvoke(new object[1] { obj });
		}
		return default(T);
	}

	public static void OnB(string key, Func<object, object> method)
	{
		AddMethodBack(key, method);
	}

	public static void On(string key, Action method)
	{
		AddMethod(key, method);
	}

	public static void On<T1>(string key, Action<T1> method)
	{
		AddMethod(key, method);
	}

	public static void On<T1, T2>(string key, Action<T1, T2> method)
	{
		AddMethod(key, method);
	}

	public static void On<T1, T2, T3>(string key, Action<T1, T2, T3> method)
	{
		AddMethod(key, method);
	}

	public static void On<T1, T2, T3, T4>(string key, Action<T1, T2, T3, T4> method)
	{
		AddMethod(key, method);
	}

	public static void Off(string key)
	{
		methods.Remove(key);
	}

	public static void Off(string key, Action method)
	{
		DelMethod(key, method);
	}

	public static void Off<T1>(string key, Action<T1> method)
	{
		DelMethod(key, method);
	}

	public static void Off<T1, T2>(string key, Action<T1, T2> method)
	{
		DelMethod(key, method);
	}

	public static void Off<T1, T2, T3>(string key, Action<T1, T2, T3> method)
	{
		DelMethod(key, method);
	}

	public static void Off<T1, T2, T3, T4>(string key, Action<T1, T2, T3, T4> method)
	{
		DelMethod(key, method);
	}

	public static void Once(string key, Action method)
	{
		AddMethodOnce(key, method);
	}

	public static void Once<T1>(string key, Action<T1> method)
	{
		AddMethodOnce(key, method);
	}

	public static void Once<T1, T2>(string key, Action<T1, T2> method)
	{
		AddMethodOnce(key, method);
	}

	public static void Once<T1, T2, T3>(string key, Action<T1, T2, T3> method)
	{
		AddMethodOnce(key, method);
	}

	public static void Once<T1, T2, T3, T4>(string key, Action<T1, T2, T3, T4> method)
	{
		AddMethodOnce(key, method);
	}

	public static void OnG(string key, Action method)
	{
		AddMethodG(key, method);
	}

	public static void OnG<T1>(string key, Action<T1> method)
	{
		AddMethodG(key, method);
	}

	public static void OnG<T1, T2>(string key, Action<T1, T2> method)
	{
		AddMethodG(key, method);
	}

	public static void OnG<T1, T2, T3>(string key, Action<T1, T2, T3> method)
	{
		AddMethodG(key, method);
	}

	public static void OnG<T1, T2, T3, T4>(string key, Action<T1, T2, T3, T4> method)
	{
		AddMethodG(key, method);
	}

	public static void OffG(string key)
	{
		gmethods.Remove(key);
	}

	public static void OffG(string key, Action method)
	{
		DelMethodG(key, method);
	}

	public static void OffG<T1>(string key, Action<T1> method)
	{
		DelMethodG(key, method);
	}

	public static void OffG<T1, T2>(string key, Action<T1, T2> method)
	{
		DelMethodG(key, method);
	}

	public static void OffG<T1, T2, T3>(string key, Action<T1, T2, T3> method)
	{
		DelMethodG(key, method);
	}

	public static void OffG<T1, T2, T3, T4>(string key, Action<T1, T2, T3, T4> method)
	{
		DelMethodG(key, method);
	}
}
