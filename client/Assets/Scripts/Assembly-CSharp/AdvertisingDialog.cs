using System;
using AdvertisingCustom;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class AdvertisingDialog : MonoBehaviour
{
	public Transform tfBottom;

	public Transform tfTime;

	public VideoPlayer pVideo;

	public Text ttTime;

	public MButton btnClose;

	public MButton btnSound;

	private Action<string> funcRewardedCall;

	private AdvertData adsData;

	private int nVideoTimeMax = 15;

	public string sAdvName => adsData.appName;

	private void OnDestroy()
	{
		DOTween.Kill(ttTime);
	}

	public void Open(Action<string> callback, int videoTimeMAX)
	{
		funcRewardedCall = callback;
		nVideoTimeMax = videoTimeMAX;
		ttTime.text = "";
		tfTime.gameObject.SetActive(value: false);
		btnClose.gameObject.SetActive(value: false);
		adsData = AdvertisingMgr.I.Data;
		tfBottom.FindC<Image>("ImageIcon").sprite = LoadMgr.LoadSprite("AdvertisingCustom/Icon/" + adsData.backPlane);
		tfBottom.FindC<MButton>("BtnInstall").onClick.AddListener(OnInstallButtonClick);
		tfBottom.FindC<Text>("TextGame").text = adsData.appName;
		btnSound.interactable = DB.User.BSound;
		pVideo.source = VideoSource.Url;
		pVideo.url = adsData.url;
		pVideo.prepareCompleted += OnPrepareFinished;
		pVideo.Prepare();
	}

	private void OnPrepareFinished(VideoPlayer video)
	{
		tfTime.gameObject.SetActive(value: true);
		int num = Math.Min((int)((float)video.frameCount / video.frameRate), nVideoTimeMax);
		TweenMgr.NumberTo((float)num, num, 0, delegate(int v)
		{
			ttTime.text = string.Format("{0}s", v);
		}).SetTarget(ttTime).SetEase(Ease.Linear).OnComplete(delegate
		{
			btnClose.gameObject.SetActive(value: true);
			tfTime.gameObject.SetActive(value: false);
		});
		pVideo.Play();
	}

	public void ClickClose(MButton button)
	{
		button.interactable = false;
		_close("succ");
	}

	private void _close(string sResult)
	{
		if (funcRewardedCall != null)
		{
			funcRewardedCall(sResult);
		}
		AdvertisingMgr.I.LoadData();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void ClickBtnSound(MButton button)
	{
		button.interactable = false;
		pVideo.SetDirectAudioMute(0, !button.interactable);
	}

	public void ClickUnBtnSound(MButton button)
	{
		button.interactable = true;
		pVideo.SetDirectAudioMute(0, !button.interactable);
	}

	public void OnInstallButtonClick(MButton button)
	{
		Application.OpenURL(adsData.install);
	}
}
