using System;
using System.Collections;
using System.Collections.Generic;
using Authentication;
using DG.Tweening;
using Downloaded;
using Festival;
using FQDev.WordBlocker;
using Function_Reward_Video;
using Game.Activity.BakingMaster;
using GameMail;
using NativePlatform;
using Pop_Gift;
using UnityEngine;
using UnityEngine.UI;
using _FunctionsBlock.GameNotices;

public class SceneLoading : MonoBehaviour
{
	private readonly UpdateVersion pUVersion = new UpdateVersion();

	[SerializeField]
	private Image imgProgress;

	[SerializeField]
	private Text textProgress;

	private float fMax;

	private float fCur;

	private readonly List<IEnumerator> funcs_start = new List<IEnumerator>();

	private readonly List<IEnumerator> funcs_end = new List<IEnumerator>();

	private const float totalWaitTime = 5f;

	private float usedTime;

	private void Start()
	{
		bool isClearAccount = Util.Object.AsString(SceneMgr.GetData()).Equals("ClearAccount");
		DataManager.I.SetUrl("https://service.fuqiangame.com/data");
		imgProgress.fillAmount = 0f;
		textProgress.SetLBy("0%");
		if (!isClearAccount)
		{
			DB.Init();
			LogEvent.I.Init();
		}
		AddProgressMax();
		AddProgressMaxS(Sdk());
		AddProgressMaxS(CheckUserPrivacyBlock());
		AddProgressMaxS(PostProcessData());
		AddProgressMax();
		AddProgressMaxE(Log());
		if (!isClearAccount)
		{
			AddProgressMaxE(Word());
		}
		AddProgressMax(5f);
		Tweener ani = TweenMgr.NumberTo(3f, 0f, 3f, delegate(float value)
		{
			usedTime = value;
			fCur = value;
			_progrees();
		}).OnComplete(delegate
		{
			pUVersion.func_finish = null;
			StartCoroutine(IELoading());
		});
		pUVersion.func_finish = delegate
		{
			ani.Kill();
			pUVersion.func_finish = null;
			StartCoroutine(IELoading());
		};
		pUVersion.CheckVersion();
		Singleton<UserLabelGetCountry>.I.RequestCountry();
		IEnumerator Sdk()
		{
			Helper.Init();
			if (!isClearAccount)
			{
				FirebaseSDK.Initialize();
				bool umpResult = false;
				KGUtils.Instance.CheckUmp(delegate(bool isEnabled)
				{
					Debug.Log($"ump result: {isEnabled}");
					umpResult = true;
					if (isEnabled)
					{
						FirebaseSDK.MonitorConsentString();
					}
				});
				LogEvent.I.SetChannelStartTime();
				AppsflyerSDK.Initialize(SingletonMono<SDKAppsFlyerDelegate>.Instance, delegate(string uid)
				{
					SingletonMono<SDKAppsFlyerDelegate>.Instance.ManualGetConversionData(uid);
				});
				FacebookSDK.Initialize();
			}
			UserLabelManager.Instance.Initialized();
			Global.NeedReloadConfig = false;
			DataCfg.Init();
			Global.pStores.ResetStorePrice();
			if (!isClearAccount)
			{
				Global.pStores.Init();
			}
			yield return 0;
		}
		static IEnumerator PostProcessData()
		{
			InitData();
			yield return 0;
		}
		IEnumerator Log()
		{
			LogEventLogin.Init(delegate
			{
			});
			LogEvent.I.AF_FirstLogin();
			Singleton<LogEventValueOnceTrack>.I.InitData();
			Singleton<LogEventValueRepeatTrack>.I.InitData();
			if (!isClearAccount)
			{
				Singleton<AdManager>.I.Initialize();
			}
			yield return 0f;
		}
		static IEnumerator Word()
		{
			string[] keywords = Resources.Load<TextAsset>("WordsBlock").text.Replace("\r\n", "\n").Split('\n');
			TextFilterCenter.Default.LoadKeyWords(keywords);
			yield return 0;
		}
	}

	private IEnumerator CheckUserPrivacyBlock()
	{
		bool checkUserConsent = true;
		if (Helper.GetCountryCode().ToLower() == "br")
		{
			int @int = PlayerPrefs.GetInt("SetUserConsent", -1);
			if (@int == -1)
			{
				checkUserConsent = true;
				Debug.Log("Is BR Show CheckUserConsentDialog");
				LogEvent.I.Custom_Attpop_Show();
				AdvertisingProxy.SetHasUserConsent(isHas: true);
			}
			else
			{
				Debug.Log($"BR Have UseUserConsent: {@int}");
				if (@int == 1)
				{
					AdvertisingProxy.SetHasUserConsent(isHas: true);
				}
			}
		}
		while (!checkUserConsent)
		{
			yield return 0.4f;
		}
		yield return 0;
	}

	private IEnumerator IELoading()
	{
		DataCenterLoginMgr.I.Init();
		ServerDateTimeMgr.I.RequestRealTime(NoticeHttpMgr.I.RequestNotice);
		while (!DataCenterLoginMgr.I.bInitEnd)
		{
			yield return null;
		}
		AddProgressCur();
		foreach (IEnumerator item in funcs_start)
		{
			yield return item;
			AddProgressCur();
		}
		yield return null;
		AddProgressCur();
		float timer = 0f;
		bool isSuccess = false;
		if (DataManager.I.bLogined && DataCenterLoginMgr.I.GetPlatformID() == 0)
		{
			Singleton<FacebookLoginCheck>.I.Check(delegate(bool isDelete)
			{
				isSuccess = true;
				if (isDelete)
				{
					Debug.LogError("facebook is Delete ,clean data");
					DataCenterLoginMgr.I.Logout();
					DataCenterLoginMgr.I.SignOut(clear: false);
					DataManager.I.InitLocalDataAll();
					DataManager.I.WriteLocalDataAll();
				}
			});
			while (timer < 5f && !isSuccess)
			{
				timer += Time.deltaTime;
				yield return timer / 5f;
			}
			if (!isSuccess)
			{
				Singleton<FacebookLoginCheck>.I.AbortCheck();
			}
		}
		float surTime = 5f - usedTime;
		Action loginfinishAction = delegate
		{
			fCur += surTime;
			_progrees();
		};
		if (!DataManager.I.bLogined)
		{
			isSuccess = false;
			DataCenterLoginMgr.I.onLoginAndGetDataSuccess = delegate
			{
				isSuccess = true;
			};
			OwnServerLogin.Instance.Login(PlatformType.Guest, DataCenterLoginMgr.I.LoginFinish);
			while (surTime > 0f && !isSuccess)
			{
				fCur += Time.deltaTime;
				surTime -= Time.deltaTime;
				_progrees();
				yield return null;
			}
			if (!isSuccess)
			{
				OwnServerLogin.Instance.AbortGuestSignIn();
			}
			else
			{
				loginfinishAction();
			}
		}
		else
		{
			loginfinishAction();
		}
		foreach (IEnumerator item2 in funcs_end)
		{
			yield return item2;
			AddProgressCur();
		}
		_jumpScene();
	}

	private void _jumpScene()
	{
		Global.AddNoviceProps();
		if (DB.User.Maxlv != 0)
		{
			SceneMgr.LoadScene("MainScene", null, delegate
			{
				LogEvent.I.Track("Start_Begin_Game").IsNew().Send();
				RankingUtils.ReqMonthIssue();
			});
		}
		else
		{
			BlackCutDialog.EnterGame(delegate
			{
				LogEvent.I.Track("Start_Begin_Game").IsNew().Send();
				RankingUtils.ReqMonthIssue();
			});
		}
	}

	private void AddProgressMax(float value = 1f)
	{
		fMax += value;
	}

	private void AddProgressMaxS(IEnumerator callback)
	{
		AddProgressMax();
		if (callback != null)
		{
			funcs_start.Add(callback);
		}
	}

	private void AddProgressMaxE(IEnumerator callback)
	{
		AddProgressMax();
		funcs_end.Add(callback);
	}

	private void AddProgressCur()
	{
		fCur += 1f;
		_progrees();
	}

	private void _progrees(float va = 0f)
	{
		imgProgress.fillAmount = (fCur + va) / fMax;
		textProgress.SetLBy($"{Mathf.FloorToInt(imgProgress.fillAmount * 100f)}%");
	}

	public static void InitData()
	{
		GiftManager.Instance.Init();
		Singleton<FestivalMgr>.I.Initialize();
		Singleton<CardSystemManager>.I.InitData();
		Singleton<ActivityMgr>.I.Initialized();
		AudioMgr.I.Init();
		AdvertisingMgr.I.Init();
		Singleton<StarRankingActivityMgr>.I.Init();
		MailMgr.I.Init();
		Singleton<UserLevelRewardManager>.I.InitDate();
		Singleton<SpecialBlockRewardManager>.I.InitData();
		Singleton<NetworkInspectionManager>.I.InitData();
		Singleton<TurntableManager>.I.InitData();
		Singleton<RewardVideoManager>.I.InitData();
	}
}
