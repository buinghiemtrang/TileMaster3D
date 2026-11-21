using UnityEngine;
using UnityEngine.UI;

namespace FQDev.WordBlocker
{
	[RequireComponent(typeof(Text))]
	public class FilteredText : FilteredLabelBase
	{
		private Text _text;

		public override string Text
		{
			get
			{
				return _text.text;
			}
			set
			{
				_text.text = FilterContent(value);
			}
		}

		private void Awake()
		{
			_text = GetComponent<Text>();
		}
	}
}
