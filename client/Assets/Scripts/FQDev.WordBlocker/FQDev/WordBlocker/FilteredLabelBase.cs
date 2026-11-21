using UnityEngine;

namespace FQDev.WordBlocker
{
	public abstract class FilteredLabelBase : UIBase
	{
		[SerializeField]
		protected char _replaceChar = '*';

		public abstract string Text { get; set; }

		protected string FilterContent(string content)
		{
			return GetFilter().AlterWord(content, _replaceChar);
		}
	}
}
