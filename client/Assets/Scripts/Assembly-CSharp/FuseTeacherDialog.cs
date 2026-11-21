using System;

public class FuseTeacherDialog : DialogBase
{
	public Action onClose;

	public FuseTeacherDialog()
	{
		fMaskAlpha = 0.8f;
	}

	public void Show()
	{
		_Open();
	}

	public void ClickCloseBtn(MButton btn)
	{
		btn.interactable = false;
		_Close(onClose);
	}
}
