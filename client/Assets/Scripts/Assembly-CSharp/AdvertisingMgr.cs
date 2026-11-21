using System;
using System.IO;
using AdvertisingCustom;
using BestHTTP;
using UnityEngine;

public class AdvertisingMgr
{
	private static readonly AdvertisingMgr _instance = new AdvertisingMgr();

	private AdvertCfg _cfgs;

	private AdvertData _data;

	private bool _waiting;

	private bool _bInit;

	private bool _bVideoReady;

	private AdvertisingDialog _pAdvertDialog;

	private DateTime _rewardAdCd = DateTime.Now;

	private DateTime _unfocusTime;

	private string ServerUrl => "https://match3d.oss-accelerate.aliyuncs.com/tilemaster3d/advertvideo/";

	public static AdvertisingMgr I => _instance;

	public AdvertData Data
	{
		get
		{
			if (!_bVideoReady)
			{
				return _cfgs.infos[0];
			}
			return _data;
		}
	}

	public static void CreateDialog(Action<string> callback, REWARD_PLACE place, bool bRewardAd = true)
	{
		if (!cf_const.isOpenCustomAdsVideo || !_instance._bInit || !_instance._cfgs.active || _instance._waiting || (bool)_instance._pAdvertDialog || DateTime.Now < _instance._rewardAdCd)
		{
			callback("fail");
			return;
		}
		_instance.UpdateAdvertisingCd();
		bool bBannerShow = Singleton<AdManager>.I.isBannerShow;
		Singleton<AdManager>.I.HideBanner();
		AudioMgr.I.PauseMusic();
		AudioMgr.I.StopSoundAll();
		AdvertisingDialog dialog = SceneMgr.AddSuperNode<AdvertisingDialog>("AdvertisingCustom/AdvertisingDialog");
		dialog.Open(delegate(string sResult)
		{
			if (sResult.Equals("succ"))
			{
				LogEvent.I.Ads_Return(place, dialog.sAdvName);
			}
			AudioMgr.I.ResumeMusic();
			callback(sResult);
			if (bBannerShow)
			{
				Singleton<AdManager>.I.ShowBanner();
			}
			_instance._pAdvertDialog = null;
		}, cf_const.customAdsVideoTimeMax);
		_instance._pAdvertDialog = dialog;
		LogEvent.I.Track("Ads_Suc").Add("AdsType", place.ToString()).Add("AdsContent", dialog.sAdvName)
			.Add("OpenUi", SceneMgr.sCurSceneName)
			.IsNew()
			.Send();
	}

	public void UpdateAdvertisingCd()
	{
		_rewardAdCd = DateTime.Now.AddSeconds(cf_const.unfocusTimeCD);
	}

	public void Init()
	{
		_cfgs = LoadMgr.LoadJson<AdvertCfg>("Json/cfgAdvertising");
		for (int i = 0; i < _cfgs.infos.Count; i++)
		{
			AdvertData advertData = _cfgs.infos[i];
			advertData.url = Path.Combine((advertData.id == 1) ? Application.streamingAssetsPath : Application.persistentDataPath, advertData.file);
		}
		LoadData();
		_bInit = true;
	}

	private AdvertData GetDataByWeight()
	{
		int total = 0;
		_cfgs.infos.ForEach(delegate(AdvertData data)
		{
			total += data.weight;
		});
		int num = MMath.Random(total);
		int num2 = 0;
		for (int i = 0; i < _cfgs.infos.Count; i++)
		{
			num2 += _cfgs.infos[i].weight;
			if (num <= num2)
			{
				return _cfgs.infos[i];
			}
		}
		return _cfgs.infos[0];
	}

	public void LoadData()
	{
		if (!_cfgs.active)
		{
			return;
		}
		_data = GetDataByWeight();
		if (_data.id == 1)
		{
			_bVideoReady = true;
			return;
		}
		_bVideoReady = false;
		if (File.Exists(_data.url))
		{
			_bVideoReady = true;
			return;
		}
		HTTPMgr.Request(ServerUrl + _data.file, delegate(HTTPRequest req, HTTPResponse resp)
		{
			if (resp != null)
			{
				int num = _data.url.LastIndexOf('/');
				if (num > 0)
				{
					Directory.CreateDirectory(_data.url.Substring(0, num));
				}
				File.WriteAllBytes(_data.url, resp.Data);
				_bVideoReady = true;
			}
		}).Send();
	}

	public void DisableWaiting()
	{
		_waiting = false;
		UpdateUnfocusTimeCd();
	}

	public void EnableWaiting()
	{
		_waiting = true;
	}

	public void UpdateUnfocusTimeCd()
	{
		_unfocusTime = DateTime.Now;
	}

	public void UnfocusBackPlay(bool pause)
	{
		if (pause)
		{
			UpdateUnfocusTimeCd();
		}
		else if (_bInit && !DB.User.AdFree && DB.User.Maxlv + 1 >= cf_const.customAdsVidelUnlockLv && cf_const.unfocusTime != 0 && cf_const.unfocusTimeCD != 0)
		{
			DateTime curTime = DateTime.Now;
			if ((curTime - _unfocusTime).TotalSeconds > (double)cf_const.unfocusTime)
			{
				CreateDialog(delegate
				{
					_unfocusTime = curTime;
				}, REWARD_PLACE.AdGoBack, bRewardAd: false);
			}
		}
	}
}
