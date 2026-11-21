using System;
using DataCenter;
using UGUI_Selected;
using UnityEngine;
using UnityEngine.UI;

public class InitUserInfoComparisonSet : MonoBehaviour
{
	[HideInInspector]
	public DialogShow pDialog;

	public Action<bool> callback;

	public Transform tfOnline;

	public Transform tfLocal;

	public Transform tfOnlineSel;

	public Transform tfLocalSel;

	public MButton btnSure;

	public Sprite[] platformIcons;

	private void Awake()
	{
		pDialog = GetComponent<DialogShow>();
		tfOnlineSel.gameObject.SetActive(value: false);
		_RefreshUI(tfOnline, isSelected: false);
		tfLocalSel.gameObject.SetActive(value: false);
		_RefreshUI(tfLocal, isSelected: false);
	}

	private void Start()
	{
		btnSure.interactable = false;
	}

	public void Init(Account online_account, User online_user, Bag online_bag, Account local_account, User local_user, Bag local_bag)
	{
		SetUserInfo(tfOnline, online_account, online_user, online_bag, b_online: false);
		SetUserInfo(tfLocal, local_account, local_user, local_bag, b_online: false);
		_SetPlatformImage(tfOnline, local_account);
	}

	private void SetUserInfo(Transform tf, Account account, User user, Bag bag, bool b_online)
	{
		tf.FindC<Text>("Level").SetLBy(user.Maxlv + 1);
		tf.FindC<Text>("NameNode/ImageName/TextName").text = account.GetName();
		cfg_medal cfgById = MedalMgr.I.GetCfgById(user.Medal);
		if (cfgById != null)
		{
			tf.FindC<Image>("NameNode/ImageName/Image").sprite = LoadMgr.LoadSprite("MedalIcon/" + cfgById.Icon);
		}
		tf.FindC<Text>("ImageGold/Text").text = "x" + bag.Prop[PropID.Money.ToString()];
		tf.FindC<Text>("ImageStar/Text").text = "x" + user.SumStar;
		tf.FindC<Text>("Image1/Text").text = "x" + bag.Prop[PropID.Back.ToString()];
		tf.FindC<Text>("Image2/Text").text = "x" + bag.Prop[PropID.Refresh.ToString()];
		tf.FindC<Text>("Image3/Text").text = "x" + bag.Prop[PropID.Freeze.ToString()];
		tf.FindC<Text>("Image4/Text").text = "x" + bag.Prop[PropID.Hint.ToString()];
	}

	private void _SetPlatformImage(Transform tf, Account account)
	{
		int platformID = DataCenterLoginMgr.I.GetPlatformID(account);
		Image image = tf.FindC<Image>("LoginPlatform");
		bool active = platformID != -1 || account.LoginPlatform.Equals(Authentication.PlatformType.Guest.ToString());
		image.gameObject.SetActive(active);
		if (image.gameObject.activeSelf)
		{
			image.sprite = platformIcons[platformID + 1];
		}
	}

	private void _RefreshUI(Component node, bool isSelected)
	{
		SelectBase[] componentsInChildren = node.GetComponentsInChildren<SelectBase>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Select(isSelected);
		}
	}

	public void ClickBtnSel(MButton button)
	{
		tfOnlineSel.gameObject.SetActive(button.nTag == 0);
		_RefreshUI(tfOnline, button.nTag == 0);
		tfLocalSel.gameObject.SetActive(button.nTag != 0);
		_RefreshUI(tfLocal, button.nTag != 0);
		btnSure.interactable = true;
	}

	public void ClickBtnSure(MButton button)
	{
		pDialog._Close();
		callback?.Invoke(tfOnlineSel.gameObject.activeSelf);
	}
}
