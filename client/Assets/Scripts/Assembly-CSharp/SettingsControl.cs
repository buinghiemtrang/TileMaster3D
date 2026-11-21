using System;
using System.Collections.Generic;
using Festival;
using Fishtail.PlayTheBall.Vibration;
using Language;
using UnityEngine;
using UnityEngine.UI;

public class SettingsControl : MonoBehaviour
{
	public MButton btnSound;

	public MButton btnVibrate;

	public MButton btnExpand;

	public MButton btnFestival;

	public Sprite[] spBtnExpand;

	public ScrollRect pScroll;

	public MButton pCell;

	public Text ttVersion;

	public MButton btnReName;

	public MButton btnQuit;

	[SerializeField]
	private LoginControl loginControl;

	[SerializeField]
	private Text uerIdTex;

	private int nSelectIndex;

	private void Start()
	{
		btnSound.interactable = DB.User.BSound;
		btnVibrate.interactable = VibrationController.I.IsShockdOn;
		btnSound.onClick.AddListener(ClickBtnSound);
		btnSound.onUnClick.AddListener(ClickBtnSound);
		btnVibrate.onClick.AddListener(ClickBtnVibrate);
		btnVibrate.onUnClick.AddListener(ClickBtnVibrate);
		if (btnQuit != null)
		{
			btnQuit.onClick.AddListener(ClickQuitBtn);
		}
		if (btnReName != null)
		{
			btnReName.onClick.AddListener(ClickBtnReName);
			btnReName.gameObject.SetActive(_IsShowReNameBtn());
		}
		if (btnFestival != null)
		{
			GameObject gameObject = btnFestival.transform.parent.gameObject;
			if (Singleton<FestivalMgr>.I.HasFestival)
			{
				gameObject.SetActive(value: true);
				btnFestival.interactable = Singleton<FestivalMgr>.I.IsShow;
				btnFestival.onClick.AddListener(ClickBtnFestival);
				btnFestival.onUnClick.AddListener(ClickBtnFestival);
			}
			else
			{
				gameObject.SetActive(value: false);
			}
		}
		if ((bool)loginControl)
		{
			loginControl.Init("Settings");
		}
		if ((bool)uerIdTex)
		{
			uerIdTex.gameObject.SetActive(!string.IsNullOrEmpty(DB.Account.Userid));
			uerIdTex.text = "ID:" + DB.Account.Userid;
		}
		if ((bool)ttVersion)
		{
			ttVersion.text = "Version:" + Application.version;
		}
		if (pScroll == null)
		{
			return;
		}
		pCell.gameObject.SetActive(value: false);
		Dictionary<string, ConfigurationData> dictionary = LanguageManager.Instance.Cfgs();
		nSelectIndex = 0;
		Toggle deafultToggle = null;
		int num = 0;
		foreach (KeyValuePair<string, ConfigurationData> item in dictionary)
		{
			MButton mButton = UnityEngine.Object.Instantiate(pCell, pScroll.content);
			mButton.gameObject.SetActive(value: true);
			mButton.param = item.Key;
			mButton.nTag = num;
			mButton.transform.FindC<Text>("Value").text = item.Value.describe;
			if (item.Key.Equals(LanguageManager.Instance.Language))
			{
				nSelectIndex = num;
				deafultToggle = mButton.transform.FindC<Toggle>("Toggle");
			}
			num++;
		}
		SetScrollContentPosY(nSelectIndex);
		TweenMgr.DelayCall(0.1f, delegate
		{
			if (deafultToggle != null)
			{
				deafultToggle.isOn = true;
			}
			pScroll.verticalScrollbar.gameObject.SetActive(value: false);
			pScroll.enabled = false;
		});
		if (dictionary.Count == 1)
		{
			btnExpand.gameObject.SetActive(value: false);
		}
	}

	private bool _IsShowReNameBtn()
	{
		if (DB.Account.IsEditorName())
		{
			if (DB.Account.Name.Length < 6)
			{
				return false;
			}
			return DB.Account.Name.Substring(0, 6).Equals("Player");
		}
		return true;
	}

	private void SetScrollContentPosY(int index)
	{
		pScroll.content.SetLocPosY(pCell.transform.Rect().height * (float)index);
	}

	public void ClickLogin(MButton btn)
	{
		SceneMgr.AddTopNode<InitUserDetailInfoSet>("Prefabs/Login/UserDetailInfoDialog");
	}

	private void ClickQuitBtn(MButton button)
	{
		Global.CreateMsgBoxDialog("Language323", delegate(bool b)
		{
			if (b)
			{
				Application.Quit();
			}
		}, isTwoBtn: true);
	}

	private void ClickBtnSound(MButton button)
	{
		button.interactable = !button.interactable;
		AudioMgr.I.SetSoundOn(button.interactable);
	}

	private void ClickBtnMusic(MButton button)
	{
		button.interactable = !button.interactable;
		AudioMgr.I.SetMusicOn(button.interactable);
	}

	private void ClickBtnVibrate(MButton button)
	{
		button.interactable = !button.interactable;
		VibrationController.I.SetShockOn(button.interactable);
		if (button.interactable)
		{
			VibrationController.I.ImpactL();
		}
	}

	private void ClickBtnFestival(MButton button)
	{
		button.interactable = !button.interactable;
		Singleton<FestivalMgr>.I.IsShow = button.interactable;
	}

	public void ClickBtnExpand(MButton button)
	{
		if (button.nTag == 0)
		{
			button.nTag = 1;
			pScroll.transform.SetHeight(240f);
			pScroll.verticalScrollbar.gameObject.SetActive(value: true);
			pScroll.enabled = true;
			btnExpand.GetComponent<Image>().sprite = spBtnExpand[0];
		}
		else
		{
			button.nTag = 0;
			pScroll.transform.SetHeight(60f);
			pScroll.verticalScrollbar.gameObject.SetActive(value: false);
			pScroll.enabled = false;
			btnExpand.GetComponent<Image>().sprite = spBtnExpand[1];
		}
		SetScrollContentPosY(nSelectIndex);
	}

	public void ClickBtnItem(MButton button)
	{
		Toggle toggle = button.transform.FindC<Toggle>("Toggle");
		if (!toggle.isOn)
		{
			toggle.isOn = true;
			LanguageManager.Instance.Language = (string)button.param;
			StorageMgr.SetString("LanguageIdentify", (string)button.param);
			ClickBtnExpand(btnExpand);
			nSelectIndex = button.nTag;
			SetScrollContentPosY(nSelectIndex);
		}
	}

	private void ClickBtnReName(MButton button)
	{
		SceneMgr.AddTopNode<RankNameDialog>("Prefabs/SceneMain/RankNameDialog").func_callback_ok = delegate
		{
			DispatcherMgr.Emit("Update_User_Name");
			if (btnReName != null)
			{
				btnReName.gameObject.SetActive(_IsShowReNameBtn());
			}
		};
	}

	public void ClickCopyUid(MButton button)
	{
		if (!string.IsNullOrEmpty(uerIdTex.text))
		{
			GUIUtility.systemCopyBuffer = uerIdTex.text;
			Global.CreateMsgBoxDialog("Language125");
		}
	}

	private void OnDisable()
	{
		if (AudioMgr.I.IsSoundOn != DB.User.BSound)
		{
			DB.User.BSound = AudioMgr.I.IsSoundOn;
		}
		if (AudioMgr.I.IsMusicOn != DB.User.BMusic)
		{
			DB.User.BMusic = AudioMgr.I.IsMusicOn;
		}
		if (VibrationController.I.IsShockdOn != DB.User.BVibrate)
		{
			DB.User.BVibrate = VibrationController.I.IsShockdOn;
		}
	}

	public void ClickBtnContactUs(MButton button)
	{
		SendMail();
	}

	private void SendMail()
	{
		string text = "Editor";
		text = "Android";
		string text2 = (DB.Account.Userid.Equals("") ? "" : (DB.Account.Userid ?? ""));
		string stringToEscape = string.Concat("Thank you for your support and feedback!", Environment.NewLine, "To assist you better, please provide:", Environment.NewLine, "Issue Description: Explain the problem, how it occurred, and when. Screenshots are recommended.", Environment.NewLine, Environment.NewLine, "Game ID: Tap the gear icon -> Copy your ID or send a settings screen screenshot.", Environment.NewLine, Environment.NewLine, "Payment Issues: Provide an official Google/Apple receipt (Note: Credit card/third-party records are not accepted).", Environment.NewLine, Environment.NewLine, Environment.NewLine, Environment.NewLine, "Please don't delete the information below", Environment.NewLine, "=========", Environment.NewLine, "GameName:", Application.productName, Environment.NewLine, "GameVersion:", Application.version, Environment.NewLine, "Platform:", text, Environment.NewLine, "UID:", text2, Environment.NewLine, "Region:", Application.systemLanguage.ToString(), Environment.NewLine, "=========", Environment.NewLine);
		string stringToEscape2 = $"{Application.productName}({Application.version})'s feedback";
		stringToEscape2 = Uri.EscapeDataString(stringToEscape2);
		stringToEscape = Uri.EscapeDataString(stringToEscape);
		Debug.Log(stringToEscape);
		Uri uri = new Uri($"mailto:{"tilemaster3d@outlook.com"}?subject={stringToEscape2}&body={stringToEscape}");
		Application.OpenURL(uri.AbsoluteUri);
	}
}
