using System;
using UnityEngine.UI;

public class RankNameDialog : DialogBase
{
	public InputField nameInputField;

	public MButton okButton;

	public MButton closeButton;

	public Action func_callback_ok;

	public Action func_callback_close;

	private string user_name = "";

	private void Start()
	{
		okButton.onClick.AddListener(OnOkButtonClick);
		closeButton.onClick.AddListener(OnCloseButtonClick);
		nameInputField.onValueChanged.AddListener(Changed_Value);
		nameInputField.onEndEdit.AddListener(End_Value);
		_Open();
	}

	public void Changed_Value(string inp)
	{
	}

	public void End_Value(string inp)
	{
		Logs.Common.Info("文本内容:" + inp);
		user_name = inp.Trim();
	}

	private void OnOkButtonClick(MButton mButton)
	{
		if (user_name != "" && user_name.Length >= 1)
		{
			DB.Account.Name = user_name;
			_Close(func_callback_ok);
		}
	}

	private void OnCloseButtonClick(MButton mButton)
	{
		_Close(func_callback_close);
	}

	public void OnNameValidation(bool valid)
	{
		if (!valid)
		{
			Global.ShowMsgTip("Language153");
			nameInputField.text = "";
		}
	}

	public static void Show(Action closeCallback, Action okCallback)
	{
		RankNameDialog rankNameDialog = SceneMgr.AddTopNode<RankNameDialog>("Prefabs/SceneMain/RankNameDialog");
		rankNameDialog.func_callback_close = closeCallback;
		rankNameDialog.func_callback_ok = okCallback;
	}
}
