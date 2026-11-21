using UnityEngine;
using UnityEngine.UI;

namespace Language
{
	[RequireComponent(typeof(Image))]
	[AddComponentMenu("Language/LanguageImage")]
	public class LanguageImage : LanguageBase
	{
		protected override void OnSet()
		{
			GetComponent<Image>().SetLBy();
		}
	}
}
