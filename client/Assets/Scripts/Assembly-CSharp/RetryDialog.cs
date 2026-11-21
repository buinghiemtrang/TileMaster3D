using System;
using UnityEngine;
using UnityEngine.UI;

public class RetryDialog : DialogBase
{
	[SerializeField]
	private Text msgTex;

	public Action<bool> cback;

	public RetryDialog()
	{
		fMaskAlpha = 0.8f;
	}

	public void Show(string msg)
	{
		msgTex.text = msg;
		_Open();
	}

	public void ClickClose(MButton btn)
	{
		_Close(delegate
		{
			cback?.Invoke(obj: false);
		});
	}

	public void ClickRetry(MButton btn)
	{
		_Close(delegate
		{
			cback?.Invoke(obj: true);
		});
	}
}
