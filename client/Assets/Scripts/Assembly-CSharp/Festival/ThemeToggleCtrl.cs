using Language;
using UnityEngine;
using UnityEngine.UI;

namespace Festival
{
	public class ThemeToggleCtrl : MonoBehaviour
	{
		[Header("-------------------")]
		public Toggle ThemeToggle;

		public Image OffIcon;

		public Image OnIcon;

		public Text OffTxt;

		public LanguageText ThemeName;

		private Image ThemeImg;

		private void Start()
		{
			ThemeName.SetContents(Singleton<FestivalMgr>.I.GetFestivalNameKey());
		}

		private void OnThemeToggleChange(bool isOn)
		{
		}
	}
}
