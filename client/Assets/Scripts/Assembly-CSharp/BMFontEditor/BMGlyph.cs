using System;
using System.Collections.Generic;

namespace BMFontEditor
{
	[Serializable]
	public class BMGlyph
	{
		public int index;

		public int x;

		public int y;

		public int width;

		public int height;

		public int offsetX;

		public int offsetY;

		public int advance;

		public int channel;

		public List<int> kerning;

		public int GetKerning(int previousChar)
		{
			if (previousChar != 0 && kerning != null)
			{
				for (int i = 0; i < kerning.Count; i += 2)
				{
					if (kerning[i] == previousChar)
					{
						return kerning[i + 1];
					}
				}
			}
			return 0;
		}

		public void SetKerning(int previousChar, int amount)
		{
			if (kerning == null)
			{
				kerning = new List<int>();
			}
			for (int i = 0; i < kerning.Count; i += 2)
			{
				if (kerning[i] == previousChar)
				{
					kerning[i + 1] = amount;
					return;
				}
			}
			kerning.Add(previousChar);
			kerning.Add(amount);
		}

		public void Trim(int xMin, int yMin, int xMax, int yMax)
		{
			int num = xMin - x;
			if (num > 0)
			{
				x = xMin;
				width -= num;
				offsetX += num;
			}
			int num2 = yMin - y;
			if (num2 > 0)
			{
				y = yMin;
				height -= num2;
				offsetY += num2;
			}
			int num3 = x + width - xMax;
			if (num3 > 0)
			{
				width -= num3;
			}
			int num4 = y + height - yMax;
			if (num4 > 0)
			{
				height -= num4;
			}
		}
	}
}
