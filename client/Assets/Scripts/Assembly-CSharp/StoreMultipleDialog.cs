using System;
using UnityEngine.UI;

public class StoreMultipleDialog : DialogBase
{
	public Action closeCallback;

	public Text saleText;

	private void Start()
	{
		saleText.text = "Reward X\ufe0f " + (cf_const.storeMultiple + 1);
	}

	public void JumpStore(MButton btn)
	{
		SceneMainBottom.instance.ClickBtnShop(btn);
		_Close(closeCallback);
	}

	public void OnClickClose(MButton btn)
	{
		_Close(closeCallback);
	}
}
