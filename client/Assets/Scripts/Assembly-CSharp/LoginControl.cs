using Authentication;
using UnityEngine;
using UnityEngine.SignInWithApple;
using UnityEngine.UI;

public class LoginControl : MonoBehaviour
{
	[SerializeField]
	private Transform backgroundTf;

	[SerializeField]
	private GameObject loginButtonsObj;

	[SerializeField]
	private MButton faceBookButton;

	[SerializeField]
	private MButton googleButton;

	[SerializeField]
	private MButton appleButton;

	[SerializeField]
	private MButton signOutButton;

	[SerializeField]
	private Image signOutIcon;

	[SerializeField]
	private Sprite[] loginPlatformIcons;

	[SerializeField]
	private DialogShow dialog;

	private string _triggerPosition;

	public void Init(string triggerPosition)
	{
		_triggerPosition = triggerPosition;
		int platformID = DataCenterLoginMgr.I.GetPlatformID();
		bool flag = DataManager.I.bLogined && platformID != -1;
		loginButtonsObj.SetActive(!flag);
		signOutButton.gameObject.SetActive(flag);
		if (flag)
		{
			signOutIcon.sprite = loginPlatformIcons[platformID];
			signOutButton.onClick.AddListener(_ClickSignOut);
			if (backgroundTf)
			{
				backgroundTf.SetHeight(910f);
			}
		}
		else
		{
			if (backgroundTf)
			{
				backgroundTf.SetHeight(1008f);
			}
			googleButton.gameObject.SetActive(false);
			appleButton.gameObject.SetActive(false);
			faceBookButton.interactable = true;
			faceBookButton.onClick.AddListener(_ClickFaceBook);
			googleButton.gameObject.SetActive(true);
			googleButton.interactable = true;
			googleButton.onClick.AddListener(_ClickGoogle);
		}
	}

	private void _ClickSignOut(MButton button)
	{
		button.interactable = false;
		Global.CreateMsgBoxDialog("Language101", delegate(bool isTrue)
		{
			if (isTrue)
			{
				DataCenterLoginMgr.I.Logout();
				DataCenterLoginMgr.I.SignOut(true);
				dialog._Close();
			}
			else
			{
				button.interactable = true;
			}
		}, true, "");
	}

	private void _ClickFaceBook(MButton button)
	{
		LogEvent.I.Track("LogIn_Click").Add("OpenWay", _triggerPosition).CommonAttr(true).Send(false);
		if (!_IsNetworkReachability())
		{
			return;
		}
		button.interactable = false;
		dialog._Close();
		OwnServerLogin.Instance.Login(PlatformType.Facebook, DataCenterLoginMgr.I.LoginFinish);
		LogEvent.I.Track("LogIn_choose").Add("LogInType", "facebook").Add("OpenWay", _triggerPosition).CommonAttr(true).Send(false);
	}

	private void _ClickGoogle(MButton button)
	{
		LogEvent.I.Track("LogIn_Click").Add("OpenWay", _triggerPosition).CommonAttr(true).Send(false);
		if (!_IsNetworkReachability())
		{
			return;
		}
		button.interactable = false;
		dialog._Close();
		OwnServerLogin.Instance.Login(PlatformType.Google, DataCenterLoginMgr.I.LoginFinish);
		LogEvent.I.Track("LogIn_choose").Add("LogInType", "google").Add("OpenWay", _triggerPosition).CommonAttr(true).Send(false);
	}

	private void _ClickApple(MButton button)
	{
		LogEvent.I.Track("LogIn_Click").Add("OpenWay", _triggerPosition).CommonAttr(true).Send(false);
		if (!_IsNetworkReachability())
		{
			return;
		}
		button.interactable = false;
		dialog._Close();
		SignInWithApple component = appleButton.gameObject.GetComponent<SignInWithApple>();
		OwnServerLogin.Instance.Login(PlatformType.Apple, DataCenterLoginMgr.I.LoginFinish, component);
		LogEvent.I.Track("LogIn_choose").Add("LogInType", "apple").Add("OpenWay", _triggerPosition).CommonAttr(true).Send(false);
	}

	private bool _IsNetworkReachability()
	{
		bool flag = Util.Helper.IsNetworkReachability();
		if (!flag)
		{
			SceneMgr.OpenWaiting(3f, 0f, delegate
			{
				Global.CreateMsgBoxDialog("Language124", null, false, "");
			});
		}
		return flag;
	}
}
