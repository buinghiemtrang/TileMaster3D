using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace FQDev.Logger
{
	internal class FileLogAppender : ILogAppender
	{
		private readonly int _mainThreadID;

		private readonly StreamWriter _writer;

		public FileLogAppender(string dir, string filename)
		{
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			_mainThreadID = Thread.CurrentThread.ManagedThreadId;
			_writer = new StreamWriter(Path.Combine(dir, filename), false);
			_writer.AutoFlush = true;
		}

		void ILogAppender.Append(LogLevel level, string message, string stackTrace)
		{
			string text;
			if (Thread.CurrentThread.ManagedThreadId == _mainThreadID)
			{
				text = string.Format("({0:HH:mm:ss.fff}|{1}|{2:f3}) [{3}]{4}\n{5}\n", DateTime.Now, Time.frameCount, Time.time, level, message, stackTrace);
			}
			else
			{
				text = string.Format("({0:HH:mm:ss.fff}) [{1}]{2}\n{3}\n", DateTime.Now, level, message, stackTrace);
			}
			lock (_writer)
			{
				_writer.Write(text);
			}
		}

		public void Close()
		{
			lock (_writer)
			{
				_writer.Close();
			}
		}
	}
}
