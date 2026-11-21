using System;
using UnityEngine;

namespace BMFontEditor
{
	public static class BMFontReader
	{
		private static string GetString(string s)
		{
			int num = s.IndexOf('=');
			if (num == -1)
			{
				return "";
			}
			return s.Substring(num + 1);
		}

		private static int GetInt(string s)
		{
			int result = 0;
			int.TryParse(GetString(s), out result);
			return result;
		}

		public static void Load(BMFont font, string name, byte[] bytes)
		{
			font.Clear();
			if (bytes == null)
			{
				return;
			}
			ByteReader byteReader = new ByteReader(bytes);
			char[] separator = new char[1] { ' ' };
			while (byteReader.canRead)
			{
				string text = byteReader.ReadLine();
				if (string.IsNullOrEmpty(text))
				{
					break;
				}
				string[] array = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length == 0)
				{
					continue;
				}
				string text2 = array[0];
				if (text2 == "char")
				{
					int channel = 15;
					if (array.Length < 9)
					{
						Debug.LogError(string.Concat(new string[6]
						{
							"Unexpected number of entries for the 'char' field (",
							name,
							", ",
							array.Length.ToString(),
							"):\n",
							text
						}));
					}
					else
					{
						if (array.Length >= 11)
						{
							int.TryParse(GetString(array[10]), out channel);
						}
						if (array.Length >= 10)
						{
							int result = 0;
							int.TryParse(GetString(array[9]), out result);
							if (result > 0)
							{
								Debug.LogError("Your font was exported with more than one texture. Only one texture is supported by NGUI.\nYou need to re-export your font, enlarging the texture's dimensions until everything fits into just one texture.");
								return;
							}
						}
						int result2 = 0;
						int.TryParse(GetString(array[1]), out result2);
						BMGlyph glyph = font.GetGlyph(result2, true);
						if (glyph == null)
						{
							Debug.Log("Char: " + array[1] + " (" + result2.ToString() + ") is NULL");
						}
						else
						{
							int.TryParse(GetString(array[2]), out glyph.x);
							int.TryParse(GetString(array[3]), out glyph.y);
							int.TryParse(GetString(array[4]), out glyph.width);
							int.TryParse(GetString(array[5]), out glyph.height);
							int.TryParse(GetString(array[6]), out glyph.offsetX);
							int.TryParse(GetString(array[7]), out glyph.offsetY);
							int.TryParse(GetString(array[8]), out glyph.advance);
							glyph.channel = channel;
						}
					}
				}
				else if (text2 == "kerning")
				{
					if (array.Length < 4)
					{
						Debug.LogError(string.Concat(new string[6]
						{
							"Unexpected number of entries for the 'kerning' field (",
							name,
							", ",
							array.Length.ToString(),
							"):\n",
							text
						}));
					}
					else
					{
						int result3 = 0;
						int.TryParse(GetString(array[1]), out result3);
						int result4 = 0;
						int.TryParse(GetString(array[2]), out result4);
						int result5 = 0;
						int.TryParse(GetString(array[3]), out result5);
						BMGlyph glyph2 = font.GetGlyph(result4, true);
						if (glyph2 != null)
						{
							glyph2.SetKerning(result3, result5);
						}
					}
				}
				else if (text2 == "common")
				{
					if (array.Length < 6)
					{
						Debug.LogError(string.Concat(new string[6]
						{
							"Unexpected number of entries for the 'common' field (",
							name,
							", ",
							array.Length.ToString(),
							"):\n",
							text
						}));
					}
					else
					{
						int result6 = 0;
						int.TryParse(GetString(array[1]), out result6);
						font.charSize = result6;
						int.TryParse(GetString(array[2]), out result6);
						font.baseOffset = result6;
						int.TryParse(GetString(array[3]), out result6);
						font.texWidth = result6;
						int.TryParse(GetString(array[4]), out result6);
						font.texHeight = result6;
						int.TryParse(GetString(array[5]), out result6);
						if (result6 != 1)
						{
							Debug.LogError("Font '" + name + "' must be created with only 1 texture, not " + result6.ToString());
						}
					}
				}
				else if (text2 == "page" && array.Length > 2)
				{
					font.spriteName = GetString(array[2]).Replace("\"", "").Replace(".png", "").Replace(".tga", "");
				}
			}
		}
	}
}
