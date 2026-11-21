using System;
using System.Collections.Generic;
using UnityEngine;

namespace BMFontEditor
{
	[Serializable]
	public class BMFont
	{
		[HideInInspector]
		[SerializeField]
		private int mSize = 16;

		[HideInInspector]
		[SerializeField]
		private int mBase;

		[HideInInspector]
		[SerializeField]
		private int mWidth;

		[HideInInspector]
		[SerializeField]
		private int mHeight;

		[HideInInspector]
		[SerializeField]
		private string mSpriteName;

		[HideInInspector]
		[SerializeField]
		private List<BMGlyph> mSaved = new List<BMGlyph>();

		private Dictionary<int, BMGlyph> mDict = new Dictionary<int, BMGlyph>();

		public bool isValid => mSaved.Count > 0;

		public int charSize
		{
			get
			{
				return mSize;
			}
			set
			{
				mSize = value;
			}
		}

		public int baseOffset
		{
			get
			{
				return mBase;
			}
			set
			{
				mBase = value;
			}
		}

		public int texWidth
		{
			get
			{
				return mWidth;
			}
			set
			{
				mWidth = value;
			}
		}

		public int texHeight
		{
			get
			{
				return mHeight;
			}
			set
			{
				mHeight = value;
			}
		}

		public int glyphCount => isValid ? mSaved.Count : 0;

		public string spriteName
		{
			get
			{
				return mSpriteName;
			}
			set
			{
				mSpriteName = value;
			}
		}

		public List<BMGlyph> glyphs => mSaved;

		public BMGlyph GetGlyph(int index, bool createIfMissing)
		{
			BMGlyph glyph = null;
			if (mDict.Count == 0)
			{
				int count = mSaved.Count;
				for (int i = 0; i < count; i++)
				{
					BMGlyph bmg = mSaved[i];
					mDict.Add(bmg.index, bmg);
				}
			}
			if (!mDict.TryGetValue(index, out glyph) && createIfMissing)
			{
				glyph = new BMGlyph();
				glyph.index = index;
				mSaved.Add(glyph);
				mDict.Add(index, glyph);
			}
			return glyph;
		}

		public BMGlyph GetGlyph(int index)
		{
			return GetGlyph(index, false);
		}

		public void Clear()
		{
			mDict.Clear();
			mSaved.Clear();
		}

		public void Trim(int xMin, int yMin, int xMax, int yMax)
		{
			if (isValid)
			{
				int count = mSaved.Count;
				for (int i = 0; i < count; i++)
				{
					BMGlyph bmg = mSaved[i];
					if (bmg != null)
					{
						bmg.Trim(xMin, yMin, xMax, yMax);
					}
				}
			}
		}
	}
}
