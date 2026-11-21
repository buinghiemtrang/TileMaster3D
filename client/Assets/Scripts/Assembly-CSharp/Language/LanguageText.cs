using System;
using UnityEngine;
using UnityEngine.UI;

namespace Language
{
	[RequireComponent(typeof(Text))]
	[AddComponentMenu("Language/LanguageText")]
	public class LanguageText : LanguageBase
	{
		protected override void OnSet()
		{
			Text component = GetComponent<Text>();
			if (!string.IsNullOrEmpty(key))
			{
				component.SetLBy(Array.Empty<object>());
			}
		}
	}
}
