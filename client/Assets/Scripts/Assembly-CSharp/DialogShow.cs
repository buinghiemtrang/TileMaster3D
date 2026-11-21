using System;
using DG.Tweening;

public class DialogShow : DialogBase
{
	public MButton closeBtn;

	public bool bStartOpen = true;

	public Action func_callback_close;

	public DialogShow()
	{
		bStartOpen = true;
		fMaskAlpha = 0.8f;
	}

	protected virtual void Start()
	{
		if (bStartOpen)
		{
			_Open();
		}
	}

	public void Open(float y, TweenCallback cback)
	{
		vecOrign.y = y;
		_Open(cback);
	}

	public void ClickClose(MButton btn)
	{
		btn.interactable = false;
		_Close(func_callback_close);
	}

	public void ClickClose()
	{
		_Close(func_callback_close);
	}
}
