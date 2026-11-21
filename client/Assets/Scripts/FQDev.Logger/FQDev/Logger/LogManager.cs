using System;
using System.Collections.Generic;

namespace FQDev.Logger
{
	public static class LogManager
	{
		private static string _userScriptRootPath;

		private static readonly List<ILogAppender> _appenders = new List<ILogAppender>();

		private static readonly Dictionary<(string, ILogIdentifier), Logger> _loggers = new Dictionary<(string, ILogIdentifier), Logger>();

		public static Logger Common { get; } = CreateLogger("Common");


		public static IReadOnlyList<ILogAppender> Appenders => _appenders;

		static LogManager()
		{
		}

		public static void RegisterAppender(ILogAppender appender)
		{
			if (appender == null)
			{
				throw new ArgumentNullException("appender");
			}
			if (_appenders.Contains(appender))
			{
				throw new Exception(string.Format("Appender {0} already registered.", appender));
			}
			_appenders.Add(appender);
		}

		public static void UnregisterAppender(ILogAppender appender)
		{
			if (appender == null)
			{
				throw new ArgumentNullException("appender");
			}
			_appenders.Remove(appender);
		}

		public static Logger CreateLogger(string name, LogLevel level = LogLevel.Info, LogStackTraceMode stackTraceMode = LogStackTraceMode.None, ILogIdentifier identifier = null, Logger.Formatter formatter = null)
		{
			if (_loggers.TryGetValue((name, identifier), out var value))
			{
				Common.Error(string.Format("Logger:\"{0}/{1}\" already existed.", name, identifier));
				return value;
			}
			if (formatter == null)
			{
				formatter = DefaultFormatter;
			}
			Logger logger = new Logger(name, identifier, formatter);
			logger.Level = level;
			logger.StackTraceMode = stackTraceMode;
			logger.UserScriptRootPath = _userScriptRootPath;
			_loggers.Add((name, identifier), logger);
			return logger;
		}

		public static Logger GetLogger(string name, ILogIdentifier identifier = null)
		{
			if (!_loggers.TryGetValue((name, identifier), out var value))
			{
				return null;
			}
			return value;
		}

		public static void RemoveLogger(Logger logger)
		{
			RemoveLogger(logger.Name, logger.Identifier);
		}

		private static void RemoveLogger(string name, ILogIdentifier identifier)
		{
			_loggers.Remove((name, identifier));
		}

		private static string DefaultFormatter(string module, ILogIdentifier identifier, string format, params object[] args)
		{
			// UNVERIFIED: the literals "[", ".", "] " come from a rank-based reading of the literal names (SL____ / SL_____45 / SL_____), not from the string table directly
			return string.Concat("[", identifier?.Name, ".", module, "] ", string.Format(format, args));
		}
	}
}
