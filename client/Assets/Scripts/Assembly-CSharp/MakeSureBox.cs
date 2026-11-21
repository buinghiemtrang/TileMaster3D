using System;
using Language;
using TMPro;
using UnityEngine;

public class MakeSureBox : DialogBase
{
	[SerializeField]
	private LanguageText _title;

	[SerializeField]
	private TextMeshProUGUI _description;

	[SerializeField]
	private MButton _greeMButton;

	[SerializeField]
	private MButton _redButton;

	public MakeSureBox()
	{
		fMaskAlpha = 0.8f;
	}

	public void InitData(string languageKey, Action redAction, Action greeAction, string titleLanguageKey = null)
	{
		if (!string.IsNullOrEmpty(titleLanguageKey))
		{
			_title.SetContents(titleLanguageKey);
		}
		LanguageData languageDataByKey = LanguageManager.Instance.GetLanguageDataByKey(languageKey);
		if (languageDataByKey != null)
		{
			_description.text = languageDataByKey.value;
		}
		if (_greeMButton != null)
		{
			_greeMButton.onClick.AddListener(delegate
			{
				greeAction?.Invoke();
			});
		}
		if (_redButton != null)
		{
			_redButton.onClick.AddListener(delegate
			{
				redAction?.Invoke();
			});
		}
	}
}
