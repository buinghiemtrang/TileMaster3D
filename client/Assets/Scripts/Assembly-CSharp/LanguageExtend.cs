using System;
using Language;
using UnityEngine;
using UnityEngine.UI;

public static class LanguageExtend
{
	public static void SetLBaseKey(this Text self, string key)
	{
		LanguageBase languageBase = self.GetComponent<LanguageBase>();
		if (languageBase == null)
		{
			languageBase = self.gameObject.AddComponent<LanguageText>();
		}
		languageBase.key = key;
		self.SetLBy(Array.Empty<object>());
	}

	public static void SetLKey(this Text self, string key, params object[] param)
	{
		LanguageData languageDataByKey = LanguageManager.Instance.GetLanguageDataByKey(key);
		if (languageDataByKey != null)
		{
			_SetLanguageText(self, languageDataByKey, param);
		}
		else if (param.Length == 0)
		{
			self.text = key;
		}
		else
		{
			self.text = string.Format(key, param);
		}
	}

	private static void _SetLanguageText(Text self, LanguageData data, params object[] param)
	{
		if (param.Length == 0)
		{
			self.text = data.value;
		}
		else
		{
			self.text = string.Format(data.value, param);
		}
		if (data.fontSize != -1)
		{
			self.fontSize = data.fontSize;
		}
		if (data.fontIndex != -1)
		{
			self.font = LanguageManager.Instance.font.GetFont(data.fontIndex);
		}
	}

	public static void SetLBy(this Text self, params object[] param)
	{
		LanguageBase component = self.GetComponent<LanguageBase>();
		if (!(component == null))
		{
			LanguageData languageDataByKey = LanguageManager.Instance.GetLanguageDataByKey(component.key);
			if (languageDataByKey != null)
			{
				_SetLanguageText(self, languageDataByKey, param);
			}
		}
	}

	public static void SetLBy(this Image self)
	{
		LanguageBase component = self.GetComponent<LanguageBase>();
		if (!(component == null))
		{
			LanguageData languageDataByKey = LanguageManager.Instance.GetLanguageDataByKey(component.key);
			if (languageDataByKey != null)
			{
				self.sprite = Resources.Load<Sprite>("Language/Picture/" + languageDataByKey.value);
				self.SetNativeSize();
			}
		}
	}

	public static void SetLEvent(this Text self, LanguageManager.EventFunc callback)
	{
		LanguageBase component = self.GetComponent<LanguageBase>();
		if (!(component == null))
		{
			component.SetEventFunc(callback);
		}
	}

	public static void ExecuteLEvent(this Text self)
	{
		LanguageBase component = self.GetComponent<LanguageBase>();
		if (!(component == null))
		{
			component.ExecuteEventFunc();
		}
	}
}
