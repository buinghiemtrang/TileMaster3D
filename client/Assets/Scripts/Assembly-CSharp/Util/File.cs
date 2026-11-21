using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Util
{
	public static class File
	{
		public static bool IsExistsByLoc(string s_file)
		{
			return System.IO.File.Exists(GetPath(s_file));
		}

		private static string GetPath(string s_file)
		{
			return Application.persistentDataPath + "/" + Application.version + "/" + s_file;
		}

		public static string ReadLocalText(string s_file)
		{
			return System.IO.File.ReadAllText(GetPath(s_file));
		}

		public static void WriteLocalText(string s_file, string s_body)
		{
			string path = GetPath(s_file);
			int length = path.LastIndexOf('/');
			Directory.CreateDirectory(path.Substring(0, length));
			StreamWriter streamWriter = new StreamWriter(path, append: false, Encoding.Default);
			streamWriter.Write(s_body);
			streamWriter.Flush();
			streamWriter.Close();
			streamWriter.Dispose();
		}

		public static void WriteFile(string s_file, string s_body)
		{
			string path = GetPath(s_file);
			int num = path.LastIndexOf('/');
			if (num > 0)
			{
				Directory.CreateDirectory(path.Substring(0, num));
			}
			FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
			byte[] bytes = Encoding.Default.GetBytes(s_body);
			fileStream.Write(bytes, 0, bytes.Length);
			fileStream.Flush();
			fileStream.Close();
		}
	}
}
