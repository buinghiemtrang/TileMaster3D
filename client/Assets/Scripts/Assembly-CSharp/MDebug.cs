using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using UnityEngine;

public static class MDebug
{
	public enum Color
	{
		green = 0,
		black = 1,
		blue = 2,
		cyan = 3,
		white = 4,
		yellow = 5,
		red = 6
	}

	private static readonly Stopwatch ms_stopwatch = new Stopwatch();

	public static void WatchStart()
	{
		if (!ms_stopwatch.IsRunning)
		{
			ms_stopwatch.Start();
		}
	}

	public static void WatchRStart()
	{
		ms_stopwatch.Restart();
	}

	public static long WatchEnd()
	{
		ms_stopwatch.Stop();
		return ms_stopwatch.ElapsedMilliseconds;
	}

	private static string Message(object message, Color color = Color.black, bool bold = false, bool italic = false)
	{
		string text = string.Format("<color={0}>{1}</color>", color.ToString(), message);
		if (bold)
		{
			text = "<b>" + text + "</b>";
		}
		if (italic)
		{
			text = "<i>" + text + "</i>";
		}
		return text;
	}

	private static string Title(object title, int level, Color color)
	{
		string message = string.Format("{0}{1}{2}{1}", Space(level * 2), Character(16 - level * 2), title);
		return string.Format("<color={0}>{1}</color>", color.ToString(), message);
	}

	private static string ObjectMessage<T>(T content)
	{
		// UNVERIFIED: body = Aggregate over typeof(T).GetFields()/GetProperties() with lambdas using unresolved literals (SL___, SL_______5, SL_____11); left as stub
		return null;
	}

	private static string ValueToString(object value)
	{
		// UNVERIFIED: the pseudo-C first checks value against 5 runtime types (globals lRam29a45b0/45f8/4590/45e0/45e8) and returns value.ToString() for them; type list not resolved (RVA 0x11643d8)
		JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
		StringBuilder stringBuilder = new StringBuilder(256);
		StringWriter stringWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
		{
			jsonTextWriter.Formatting = jsonSerializer.Formatting;
			jsonSerializer.Serialize(jsonTextWriter, value, null);
		}
		return stringWriter.ToString();
	}

	private static void _log(string message, LogType type = LogType.Log)
	{
		UnityEngine.Debug.unityLogger.logHandler.LogFormat(type, null, "{0}", message);
	}

	[Conditional("OPEN_LOGGER")]
	public static void Log(object message, Color color = Color.cyan)
	{
		_log(Message(message, color));
	}

	[Conditional("OPEN_LOGGER")]
	public static void Log(string tag, object message, Color color = Color.cyan)
	{
		// UNVERIFIED: separator literal (SL_____35) not resolved, "#" assumed (RVA 0x1164714)
		_log("#" + tag + "#" + Message(message, color));
	}

	[Conditional("OPEN_LOGGER")]
	public static void LogError(object message)
	{
		_log(message.ToString(), LogType.Error);
	}

	[Conditional("OPEN_LOGGER")]
	public static void LogWarning(object message)
	{
		_log(Message(message, Color.yellow), LogType.Warning);
	}

	[Conditional("OPEN_LOGGER")]
	public static void LogBold(object message, Color color = Color.cyan)
	{
		_log(Message(message, color, bold: true));
	}

	[Conditional("OPEN_LOGGER")]
	public static void LogItalic(object message, Color color = Color.cyan)
	{
		_log(Message(message, color, bold: false, italic: true));
	}

	[Conditional("OPEN_LOGGER")]
	public static void LogBoldItalic(object message, Color color = Color.cyan)
	{
		_log(Message(message, color, bold: true, italic: true));
	}

	[Conditional("OPEN_LOGGER")]
	public static void LogTitle(object title, int level, Color color)
	{
	}

	[Conditional("OPEN_LOGGER")]
	public static void LogObject<T>(T content, Color color = Color.cyan)
	{
		// UNVERIFIED: generic body needs string literals that the pseudo-C only names lossily (SL_ names, e.g. SL___List_/SL_____11/SL______2); debug-only ([Conditional("OPEN_LOGGER")]), left empty
	}

	[Conditional("OPEN_LOGGER")]
	public static void LogObjects<T>(T[] content)
	{
		// UNVERIFIED: generic body needs string literals that the pseudo-C only names lossily (SL_ names, e.g. SL___List_/SL_____11/SL______2); debug-only ([Conditional("OPEN_LOGGER")]), left empty
	}

	[Conditional("OPEN_LOGGER")]
	public static void LogList<T>(List<T> content, string title = null)
	{
		// UNVERIFIED: generic body needs string literals that the pseudo-C only names lossily (SL_ names, e.g. SL___List_/SL_____11/SL______2); debug-only ([Conditional("OPEN_LOGGER")]), left empty
	}

	public static void LogDict<k, v>(Dictionary<k, v> content)
	{
		// UNVERIFIED: generic body needs string literals that the pseudo-C only names lossily (SL_ names, e.g. SL___List_/SL_____11/SL______2); debug-only ([Conditional("OPEN_LOGGER")]), left empty
	}

	[Conditional("OPEN_LOGGER")]
	public static void LogLine<T>(List<T> content, string title = null)
	{
		// UNVERIFIED: generic body needs string literals that the pseudo-C only names lossily (SL_ names, e.g. SL___List_/SL_____11/SL______2); debug-only ([Conditional("OPEN_LOGGER")]), left empty
	}

	[Conditional("OPEN_LOGGER")]
	public static void LogLine<T>(T[] content, string title = null)
	{
		// UNVERIFIED: generic body needs string literals that the pseudo-C only names lossily (SL_ names, e.g. SL___List_/SL_____11/SL______2); debug-only ([Conditional("OPEN_LOGGER")]), left empty
	}

	[Conditional("OPEN_LOGGER")]
	public static void LogArray<T>(T[] content)
	{
		// UNVERIFIED: generic body needs string literals that the pseudo-C only names lossily (SL_ names, e.g. SL___List_/SL_____11/SL______2); debug-only ([Conditional("OPEN_LOGGER")]), left empty
	}

	[Conditional("OPEN_LOGGER")]
	public static void LogList2<T>(List<List<T>> content)
	{
		// UNVERIFIED: generic body needs string literals that the pseudo-C only names lossily (SL_ names, e.g. SL___List_/SL_____11/SL______2); debug-only ([Conditional("OPEN_LOGGER")]), left empty
	}

	[Conditional("OPEN_LOGGER")]
	public static void Assert(bool condition, string message = "")
	{
		if (!condition)
		{
			if (!string.IsNullOrEmpty(message))
			{
				_log(message, LogType.Assert);
		}
			throw new ArgumentException();
		}
	}

	private static string Space(int number)
	{
		string text = "";
		for (int i = 0; i < number; i++)
		{
			text += " "; // UNVERIFIED: literal SL_____32 assumed to be a single space
		}
		return text;
	}

	private static string Character(int number, string character = "=")
	{
		string text = "";
		for (int i = 0; i < number; i++)
		{
			text += "="; // UNVERIFIED: literal SL_____47 assumed "="; pseudo-C ignores the character parameter
		}
		return text;
	}
}
