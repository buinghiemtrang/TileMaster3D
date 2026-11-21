using UnityEngine;
using UnityEngine.UI;

public class LoginDialog : MonoBehaviour
{
	[SerializeField]
	private Toggle remindToggle;

	[SerializeField]
	private LoginControl loginControl;

	private void Start()
	{
		loginControl.Init("PurchaseGuild");
		remindToggle.isOn = DB.Login.IsNotRemindAgainLogin;
		remindToggle.onValueChanged.AddListener(delegate(bool isNotRemind)
		{
			LoginMgr.I.SetNotRemindAgainLoginState(isNotRemind);
		});
	}
}
