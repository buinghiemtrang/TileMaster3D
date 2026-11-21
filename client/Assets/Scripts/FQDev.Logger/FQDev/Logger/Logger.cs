using System;
using System.Diagnostics;
using System.Text;

namespace FQDev.Logger
{
	public class Logger
	{
		public delegate string Formatter(string module, ILogIdentifier identifier, string format, params object[] args);

		private string mUserScriptRootPath;

		private readonly Formatter _formatter;

		public string Name { get; }

		public ILogIdentifier Identifier { get; }

		public LogLevel Level { get; set; }

		public LogStackTraceMode StackTraceMode { get; set; }

		public string UserScriptRootPath
		{
			set
			{
				mUserScriptRootPath = (string.IsNullOrEmpty(value) ? string.Empty : value.Replace('\\', '/'));
			}
		}

		public Logger(string name, ILogIdentifier identifier, Formatter formatter)
		{
			Name = name;
			Identifier = identifier;
			_formatter = formatter;
			Level = LogLevel.Info;
		}

		private string GenerateStackTrace()
		{
			StringBuilder stringBuilder = new StringBuilder();
			StackTrace stackTrace = new StackTrace((StackTraceMode == LogStackTraceMode.All) ? 1 : 3, fNeedFileInfo: true);
			StackFrame[] frames = stackTrace.GetFrames();
			for (int i = 0; i < frames.Length; i++)
			{
				StackFrame stackFrame = frames[i];
				string text = stackFrame.GetFileName() ?? string.Empty;
				text = text.Replace('\\', '/');
				if (StackTraceMode == LogStackTraceMode.Script)
				{
					if (string.IsNullOrEmpty(text))
					{
						continue;
					}
					if (!string.IsNullOrEmpty(mUserScriptRootPath) && !text.Contains(mUserScriptRootPath))
					{
						continue;
					}
				}
				int num = text.IndexOf(mUserScriptRootPath, StringComparison.Ordinal);
				if (num != -1)
				{
					text = text.Substring(mUserScriptRootPath.Length + num).TrimStart('/');
				}
				// UNVERIFIED: the 3 literals below (" (", ".", ")") come from a rank-based reading of the literal names (SL______7 / SL_____45 / SL_____40), not from the string table directly
				stringBuilder.Append(stackFrame.GetMethod().Name);
				stringBuilder.Append(" (");
				stringBuilder.Append(text);
				stringBuilder.Append(".");
				stringBuilder.Append(stackFrame.GetFileLineNumber());
				stringBuilder.AppendLine(")");
			}
			return stringBuilder.ToString();
		}

		private void LogOut(LogLevel level, string format, params object[] args)
		{
			if (Level < level || LogManager.Appenders.Count <= 0)
			{
				return;
			}
			string message = _formatter(Name, Identifier, format, args);
			string stackTrace = ((level < LogLevel.Warning || StackTraceMode != LogStackTraceMode.None) ? GenerateStackTrace() : string.Empty);
			for (int i = 0; i < LogManager.Appenders.Count; i++)
			{
				LogManager.Appenders[i].Append(level, message, stackTrace);
			}
		}

		private void LogOut(LogLevel level, object message)
		{
			if (Level < level || LogManager.Appenders.Count <= 0)
			{
				return;
			}
			string text = _formatter(Name, Identifier, message.ToString(), Array.Empty<object>());
			string stackTrace = ((level < LogLevel.Warning || StackTraceMode != LogStackTraceMode.None) ? GenerateStackTrace() : string.Empty);
			string message2 = ((text != null) ? text.ToString() : "null");
			for (int i = 0; i < LogManager.Appenders.Count; i++)
			{
				LogManager.Appenders[i].Append(level, message2, stackTrace);
			}
		}

		public void Exception(object message)
		{
			LogOut(LogLevel.Exception, message);
		}

		public void Exception(string format, params object[] args)
		{
			LogOut(LogLevel.Exception, format, args);
		}

		public void Error(string format, params object[] args)
		{
			LogOut(LogLevel.Error, format, args);
		}

		public void Warning(string format, params object[] args)
		{
			LogOut(LogLevel.Warning, format, args);
		}

		public void Info(string format, params object[] args)
		{
			LogOut(LogLevel.Info, format, args);
		}
	}
}
