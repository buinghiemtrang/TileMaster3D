using System;
using UnityEngine;

namespace FQDev.Logger
{
	public class UnityLogAdaptor : ILogHandler
	{
		private readonly Logger _logger;

		public UnityLogAdaptor(Logger logger)
		{
			_logger = logger;
			Application.logMessageReceived += HandleLog;
		}

		public void Destroy()
		{
			Application.logMessageReceived -= HandleLog;
		}

		private void HandleLog(string logString, string stackTrace, LogType type)
		{
			if (logString.Length > 0 && logString[0] == '[')
			{
				return;
			}
			// UNVERIFIED: the separator in "{0}\n{1}" is one of '\n', ' ', '.', '/', ':', '|', '~' (metadata literal could not be resolved uniquely)
			switch (type)
			{
			case LogType.Error:
			case LogType.Assert:
				_logger.Error("{0}\n{1}", logString, stackTrace);
				break;
			case LogType.Warning:
				_logger.Warning("{0}\n{1}", logString, stackTrace);
				break;
			case LogType.Log:
				_logger.Info("{0}\n{1}", logString, stackTrace);
				break;
			case LogType.Exception:
				_logger.Exception("{0}\n{1}", logString, stackTrace);
				break;
			}
		}

		void ILogHandler.LogException(Exception exception, UnityEngine.Object context)
		{
			_logger.Exception(exception);
		}

		void ILogHandler.LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
		{
			switch (logType)
			{
			case LogType.Error:
			case LogType.Assert:
				_logger.Error(format, args);
				break;
			case LogType.Warning:
				_logger.Warning(format, args);
				break;
			case LogType.Log:
				_logger.Info(format, args);
				break;
			case LogType.Exception:
				_logger.Exception(format, args);
				break;
			}
		}
	}
}
