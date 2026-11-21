using TMPro;
using UnityEngine;

namespace FQDev.WordBlocker
{
	[RequireComponent(typeof(TMP_Text))]
	public class FilteredTMPText : FilteredLabelBase
	{
		private TMP_Text _text;

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
			_text = GetComponent<TMP_Text>();
		}
	}
}
