using System;
using Language;
using UnityEngine.UI;

public class MsgBoxDialog : DialogBase
{
	public MButton yesBtn;

	public MButton noBtn;

	public Text yesTex;

	public Text contens;

	public Text TextTitle;

	private Action<bool> cBack;

	public MsgBoxDialog()
	{
		fMaskAlpha = 0.8f;
	}

	protected override void Awake()
	{
		base.Awake();
		yesBtn.onClick.AddListener(delegate
		{
			_Close(delegate
			{
				cBack?.Invoke(obj: true);
			});
		});
		noBtn.onClick.AddListener(delegate
		{
			_Close(delegate
			{
				cBack?.Invoke(obj: false);
			});
		});
	}

	public void Show(string msg, Action<bool> cBack, bool isTwoBtn = false, string title = "")
	{
		this.cBack = cBack;
		contens.text = msg;
		if (TextTitle != null && !string.IsNullOrEmpty(title))
		{
			TextTitle.text = title;
		}
		yesTex.SetLKey(isTwoBtn ? "Language50" : "Language27");
		if (!isTwoBtn)
		{
			noBtn.gameObject.SetActive(value: false);
			yesBtn.transform.SetLocPosX(0f);
		}
		_Open();
	}

	public void SetMsg(string msg, params object[] param)
	{
		LanguageData languageDataByKey = LanguageManager.Instance.GetLanguageDataByKey(msg);
		if (languageDataByKey != null)
		{
			msg = languageDataByKey.value;
		}
		contens.text = string.Format(msg, param);
	}
}
