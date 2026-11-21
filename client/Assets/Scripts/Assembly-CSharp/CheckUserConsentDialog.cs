using System;
using UnityEngine;

public class CheckUserConsentDialog : DialogBase
{
	[SerializeField]
	private MButton okButton;

	[SerializeField]
	private MButton cancelButton;

	public Action<bool> UseChooseCallBack;

	private void Start()
	{
		okButton.onClick.AddListener(delegate
		{
			UseChooseCallBack?.Invoke(obj: true);
			LogEvent.I.Custom_Attpop("allow");
			_Close();
		});
		cancelButton.onClick.AddListener(delegate
		{
			UseChooseCallBack?.Invoke(obj: false);
			LogEvent.I.Custom_Attpop("not allow");
			_Close();
		});
	}

	public static void Create(Action<bool> useChooseCallBack)
	{
		SceneMgr.AddTopNode<CheckUserConsentDialog>("CheckUserConsentDialog").UseChooseCallBack = useChooseCallBack;
	}
}
