using System;
using System.Globalization;
using UnityEngine;

namespace FQDev.Logger
{
	public static class UnityLogAdaptation
	{
		private static Logger _logger;

		private static ILogAppender _consoleAppender;

		private static FileLogAppender _fileAppender;

		private static UnityLogAdaptor _adaptor;

		public static void Initialize()
		{
			if (LogManager.GetLogger("Unity") == null)
			{
				Debug.Log("UnityLogAdaptation.Init");
				string dir = Application.persistentDataPath + "/Logs";
				// UNVERIFIED: the timestamp format is either "yyyy-MM-dd HH:mm:ss" or "yyyy.MM.dd_HH.mm.ss" (both exist in the metadata and map to the same literal name); the first one is chosen
				string filename = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo) + ".log";
				_fileAppender = new FileLogAppender(dir, filename);
				LogManager.RegisterAppender(_fileAppender);
				_logger = LogManager.CreateLogger("Unity", LogLevel.Info);
				_adaptor = new UnityLogAdaptor(_logger);
			}
		}

		public static void Destroy()
		{
			if (LogManager.GetLogger("Unity") != null)
			{
				if (_consoleAppender != null)
				{
					LogManager.UnregisterAppender(_consoleAppender);
				}
				_fileAppender.Close();
				LogManager.UnregisterAppender(_fileAppender);
				LogManager.RemoveLogger(_logger);
				_adaptor.Destroy();
			}
		}
	}
}
