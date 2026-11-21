using UnityEngine;
using UnityEngine.UI;

public class BoosterHelpDialog : DialogBase
{
	[SerializeField]
	private Text[] lvTex;

	public BoosterHelpDialog()
	{
		fMaskAlpha = 0.8f;
	}

	public void Show()
	{
		for (int i = 0; i < lvTex.Length; i++)
		{
			lvTex[i].SetLKey("Language29", string.Format("{0}", i + 1));
		}
		_Open();
	}

	public void ClickClose(MButton btn)
	{
		btn.interactable = false;
		_Close();
	}

	public void ClickTrue(MButton btn)
	{
		btn.interactable = false;
		_Close();
	}
}
