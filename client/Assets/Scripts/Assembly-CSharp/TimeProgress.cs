using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TimeProgress : MonoBehaviour
{
	[SerializeField]
	private bool _isGameScene;

	public GameObject parentNode;

	public Text textTime;

	public Image bar;

	public Transform tfClockAni;

	public Transform tfColorNor;

	private TweenCallback func_callback;

	private Action func_freeze_end;

	private Tween pTweener;

	private Tween pTweenFreeze;

	private float tTimeMax;

	private float fTimeResidue;

	private float timer;

	private int nShowTime;

	private List<FlyItem> stars;

	private string startIconPath;

	public bool IsGameScene => _isGameScene;

	public bool IsFreeze => pTweenFreeze != null;

	public int TimeResidue => (int)fTimeResidue;

	public int TimeMax => (int)tTimeMax;

	private void Start()
	{
		LevelInterAd.OnPlaySuc = delegate
		{
			timer = 0f;
		};
	}

	private void OnEnable()
	{
		DispatcherMgr.On<bool, string>("GamePause", SetGamePause);
		DispatcherMgr.On("InGame_Interstitial_Start", _InterstitialPause);
		DispatcherMgr.On("InGame_Interstitial_End", _InterstitialPlay);
	}

	private void OnDisable()
	{
		DispatcherMgr.Off<bool, string>("GamePause", SetGamePause);
		DispatcherMgr.Off("InGame_Interstitial_Start", _InterstitialPause);
		DispatcherMgr.Off("InGame_Interstitial_End", _InterstitialPlay);
	}

	private void _InterstitialPause()
	{
		if (pTweenFreeze != null)
		{
			pTweenFreeze.Pause();
		}
		else if (pTweener != null)
		{
			pTweener.Pause();
		}
	}

	private void _InterstitialPlay()
	{
		if (pTweenFreeze != null)
		{
			pTweenFreeze.Play();
		}
		else if (pTweener != null)
		{
			pTweener.Play();
		}
	}

	public void SetGamePause(bool bPause, string param = "")
	{
		if (!bPause)
		{
			_InterstitialPlay();
		}
		else
		{
			_InterstitialPause();
		}
	}

	public TimeProgress InitTime(int nTimeMAX, int nTimeResidue, TweenCallback callback = null, bool isInit = true)
	{
		float num = nTimeResidue;
		tTimeMax = nTimeMAX;
		fTimeResidue = num;
		timer = (isInit ? 0f : 45f);
		if (isInit)
		{
			Kill();
		}
		else
		{
			if (pTweener != null)
			{
				pTweener.Kill();
				pTweener = null;
			}
			LevelInterAd.Reset();
		}
		if (nTimeMAX < 1)
		{
			parentNode.SetActive(value: false);
		}
		else
		{
			parentNode.SetActive(value: true);
			func_callback = callback;
			SetTime(num);
			pTweener = DOTween.To(SetTime, num, 0f, num).SetEase(Ease.Linear).OnComplete(func_callback);
			pTweener.Pause();
		}
		return this;
	}

	public void AddTime(int nTime, bool bPlay)
	{
		InitTime((int)tTimeMax, (int)fTimeResidue + nTime, func_callback, isInit: false);
		if (bPlay)
		{
			Play();
		}
	}

	public void DelTime(int nTime, bool bPlay)
	{
		if (fTimeResidue - (float)nTime <= 0f)
		{
			if (func_callback != null)
			{
				func_callback();
			}
			return;
		}
		InitTime((int)tTimeMax, (int)fTimeResidue - nTime, func_callback, isInit: false);
		if (bPlay)
		{
			Play();
		}
	}

	private void UpdateTime(float fTime)
	{
		// UNVERIFIED: (int)fTime may be Mathf.FloorToInt (System.Math class init is referenced); same result for non-negative values
		if ((bool)textTime)
		{
			int num = (int)fTime;
			if (nShowTime == -1 || nShowTime != num)
			{
				nShowTime = num;
				textTime.text = Util.Helper.FormatTimer(num, "mm:ss");
			}
		}
		if ((bool)bar)
		{
			bar.fillAmount = Mathf.Clamp01(fTime / tTimeMax);
		}
	}

	private void SetTime(float fTime)
	{
		if (!DB.User.AdFree && DB.User.Curlv >= cf_const.gameInterstitialUnlockLv)
		{
			timer += fTimeResidue - fTime;
			if (timer >= cf_const.gameInterstitialIntervalTime)
			{
				LevelInterAd.ShowPrepare();
			}
		}
		if ((bool)GameScene.OperationTip.I && GameScene.OperationTip.I.JudgeFreezeTip())
		{
			GameScene.OperationTip.I.FreezeTip(fTime);
		}
		if ((bool)tfClockAni)
		{
			bool activeSelf = tfClockAni.gameObject.activeSelf;
			if (fTime < 30f && !activeSelf)
			{
				tfClockAni.gameObject.SetActive(value: true);
				if ((bool)tfColorNor)
				{
					tfColorNor.gameObject.SetActive(value: false);
				}
			}
		}
		fTimeResidue = fTime;
		UpdateTime(fTime);
	}

	public void Pause()
	{
		if (pTweener != null)
		{
			pTweener.Pause();
		}
	}

	public void Play()
	{
		if (pTweener != null)
		{
			pTweener.Play();
		}
	}

	public void Kill()
	{
		if ((bool)tfClockAni)
		{
			tfClockAni.gameObject.SetActive(value: false);
		}
		if ((bool)tfColorNor)
		{
			tfColorNor.gameObject.SetActive(value: true);
		}
		if (pTweener != null)
		{
			pTweener.Kill();
			pTweener = null;
		}
		if (pTweenFreeze != null)
		{
			pTweenFreeze.Kill();
			pTweenFreeze = null;
		}
	}

	public void ClearTime(Action cback)
	{
		if (fTimeResidue == 0f)
		{
			if (cback != null)
			{
				cback();
			}
			return;
		}
		StartCoroutine(IETimeStar(cback));
	}

	private IEnumerator IETimeStar(Action cback)
	{
		int num = Mathf.Max(1, (int)(fTimeResidue / 25f));
		int animNum = num;
		Vector2 sPos = base.transform.localPosition;
		Vector2 ePos = base.transform.InverseTransformPoint(GameScene.GameMgr.I.chestControl.tfStar.position);
		Action flyEnd = null;
		while (num > 0)
		{
			FlyItem flyItem = stars.FirstOrDefault((FlyItem fd) => fd.isFree);
			if (!flyItem)
			{
				flyItem = LoadMgr.LoadRes<GameObject>(startIconPath).GetComponent<FlyItem>();
				stars.Add(flyItem);
			}
			fTimeResidue = Mathf.Max(0f, fTimeResidue - 25f);
			UpdateTime(fTimeResidue);
			if (flyEnd == null)
			{
				flyEnd = delegate
				{
					GameScene.GameMgr.I.chestControl.Add(1);
					if (animNum < 2)
					{
						cback?.Invoke();
					}
					else
					{
						animNum--;
					}
				};
			}
			flyItem.Fly2D(base.transform, sPos, ePos, flyEnd);
			num--;
			yield return new WaitForSeconds(0.05f);
		}
	}

	public void RecoveryTime()
	{
		if (pTweener == null)
		{
			// UNVERIFIED: pseudo-C only shows the inlined DOTween "Null Tween" warning path (Debugger.LogNullTween); calling Play() on the null tween reproduces it
			pTweener.Play();
		}
	}

	public void Freeze(int time, Action callback = null)
	{
		AudioMgr.I.PlaySound("prop_reeze");
		func_freeze_end = callback;
		Pause();
		pTweenFreeze = TweenMgr.NumberTo(time, time, 0).OnComplete(delegate
		{
			pTweenFreeze = null;
			Play();
			if (func_freeze_end != null)
			{
				func_freeze_end();
			}
		});
	}

	private void OnDestroy()
	{
		if (pTweener != null)
		{
			pTweener.Kill();
			pTweener = null;
		}
		if (pTweenFreeze != null)
		{
			pTweenFreeze.Kill();
			pTweenFreeze = null;
		}
	}
}
