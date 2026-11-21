using System.Collections.Generic;
using UnityEngine;

namespace Language
{
	public class FontManager
	{
		private List<Font> fonts;

		private string[] na = new string[1] { "BalooBhaijaan-Regular" };

		public FontManager()
		{
			if (fonts == null)
			{
				fonts = new List<Font>();
				for (int i = 0; i < na.Length; i++)
				{
					fonts.Add(LoadMgr.Load<Font>("_Fonts/" + na[i]));
				}
			}
		}

		public Font GetFont(int index)
		{
			if (index < fonts.Count)
			{
				return fonts[index];
			}
			return null;
		}
	}
}
