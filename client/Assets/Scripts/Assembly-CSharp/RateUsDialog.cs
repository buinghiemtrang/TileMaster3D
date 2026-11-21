using System;
using System.Collections.Generic;
using UnityEngine;

public class RateUsDialog : DialogBase
{
	public Sprite normalStar;

	public Sprite shineStar;

	public Transform stars;

	public MButton closeButton;

	public MButton rateButton;

	public Action closeCall;

	private List<StarButton> starButtons;

	private int starIndex;

	private string packName = "com.higgs.tilemaster3d";

	private static int nextShowTimes = 11;

	public static bool CanShow()
	{
		return !StorageMgr.GetBool("RateUs_IsRate", b: false) && DB.User.Curlv == cf_const.unlockRateUsLv;
	}

	public static void ShowRateUs(Action closeCall = null)
	{
		if (!CanShow())
		{
			closeCall?.Invoke();
			return;
		}
		RateUsDialog rateUsDialog = SceneMgr.AddTopNode<RateUsDialog>("Prefabs/RateUsDialog");
		rateUsDialog.closeCall = closeCall;
		rateUsDialog._Open();
	}

	private void Start()
	{
		starButtons = new List<StarButton>();
		for (int i = 0; i < stars.childCount; i++)
		{
			StarButton component = stars.GetChild(i).GetComponent<StarButton>();
			component.index = i;
			component.callback = UpdateShowStar;
			starButtons.Add(component);
		}
		closeButton.onClick.AddListener(OnCloseClick);
		rateButton.onClick.AddListener(OnRateClick);
		rateButton.interactable = false;
		LogEvent.I.Track("Rate_Us_Open").CommonAttr(b_max: false).Send(b_apps_flyer_analytics: false);
	}

	private void OnCloseClick(MButton mButton)
	{
		Close();
	}

	private void UpdateShowStar(int index)
	{
		rateButton.interactable = true;
		rateButton.gameObject.SetActive(value: true);
		if (index < 0 || index >= starButtons.Count)
		{
			return;
		}
		starIndex = index;
		for (int i = 0; i < starButtons.Count; i++)
		{
			starButtons[i].ShowStar((index < i) ? normalStar : shineStar);
		}
	}

	private void OnRateClick(MButton mButton)
	{
		StorageMgr.GetBool("RateUs_IsRate", b: true);
		if (starIndex > 2)
		{
			DoGoStore(isRateScore: false);
		}
		Close();
	}

	public void Close()
	{
		LogEvent.I.Track("Rate_Us_Close").Add("StarNum", starIndex).CommonAttr(b_max: false).Send(b_apps_flyer_analytics: false);
		_Close(closeCall);
	}

	public void DoGoStore(bool isRateScore)
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("android.content.Intent");
		AndroidJavaClass androidJavaClass3 = new AndroidJavaClass("android.net.Uri");
		AndroidJavaObject @static = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		AndroidJavaObject androidJavaObject = new AndroidJavaObject("java.lang.String", "https://play.google.com/store/apps/details?id=" + packName);
		AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("android.content.Intent", androidJavaClass2.GetStatic<AndroidJavaObject>("ACTION_VIEW"), androidJavaClass3.CallStatic<AndroidJavaObject>("parse", androidJavaObject));
		@static.Call("startActivity", androidJavaObject2);
	}
}
