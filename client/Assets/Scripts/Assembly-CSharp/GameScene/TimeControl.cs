using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameScene_Level;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene
{
	public class TimeControl : MonoBehaviour
	{
		[SerializeField]
		private Text timeTex;

		[SerializeField]
		private GameObject rootNodeObj;

		[SerializeField]
		private GameObject hardNodeObj;

		[SerializeField]
		private GameObject veryHardNodeObj;

		[SerializeField]
		private Image levelTypeIconImg;

		[SerializeField]
		private Sprite[] levelTypeIconSprites;

		private int _pauseNumber;

		private int _adTime;

		private List<FlyItem> _stars;

		private string _startIconPath;

		private bool _isPlayingInterstitial;

		public int ResidueTime { get; private set; }

		public int TotalTime { get; private set; }

		private void Awake()
		{
			_stars = new List<FlyItem>();
			_startIconPath = "Effect/Star";
			DispatcherMgr.On<GameStatus>("GameStatus_Change", _ChangeStatus);
			DispatcherMgr.On("GameScene_Freeze_Start", _Pause);
			DispatcherMgr.On("GameScene_Freeze_End", _Play);
			DispatcherMgr.On("GameScene_Guide_SpecialElement_Start", _Pause);
			DispatcherMgr.On("GameScene_Guide_SpecialElement_End", _Play);
			DispatcherMgr.On("InGame_Interstitial_Start", _InterstitialPause);
			DispatcherMgr.On("InGame_Interstitial_End", _InterstitialPlay);
			DispatcherMgr.On("GameScene_Platform_StartFuse", _InterstitialPlay);
			DispatcherMgr.On("GameScene_User_Level_Reward_Play_Interstitial_Success", OnUserLevelRewardPlayInterstitialSuccess);
		}

		private void OnDestroy()
		{
			timeTex.DOKill();
			DispatcherMgr.Off<GameStatus>("GameStatus_Change", _ChangeStatus);
			DispatcherMgr.Off("GameScene_Freeze_Start", _Pause);
			DispatcherMgr.Off("GameScene_Freeze_End", _Play);
			DispatcherMgr.Off("GameScene_Guide_SpecialElement_Start", _Pause);
			DispatcherMgr.Off("GameScene_Guide_SpecialElement_End", _Play);
			DispatcherMgr.Off("InGame_Interstitial_Start", _InterstitialPause);
			DispatcherMgr.Off("InGame_Interstitial_End", _InterstitialPlay);
			DispatcherMgr.Off("GameScene_Platform_StartFuse", _InterstitialPlay);
			DispatcherMgr.Off("GameScene_User_Level_Reward_Play_Interstitial_Success", OnUserLevelRewardPlayInterstitialSuccess);
		}

		public void Init(bool isReset, int totalTime, int residueTime)
		{
			int adTime = (isReset ? 45 : 0);
			ResidueTime = residueTime;
			TotalTime = totalTime;
			_pauseNumber = 0;
			_adTime = adTime;
			_isPlayingInterstitial = false;
			timeTex.DOKill();
			hardNodeObj.SetActive(false);
			veryHardNodeObj.SetActive(false);
			Debug.LogError("GameScene: " + totalTime);
			rootNodeObj.SetActive(totalTime > 0);
			if (totalTime == 0)
			{
				return;
			}
			if (LevelMgr.Data.Type == LevelType.BakingMaster)
			{
				levelTypeIconImg.gameObject.SetActive(false);
			}
			else
			{
				int levelHardLevel = LevelMgr.GetLevelHardLevel();
				if (levelHardLevel == 2)
				{
					veryHardNodeObj.SetActive(true);
				}
				else if (levelHardLevel == 1)
				{
					hardNodeObj.SetActive(true);
				}
				LevelMgr.GetLevelStatus(out var isRapidly, out var isRegenerate);
				Sprite sprite;
				if (isRapidly)
				{
					sprite = levelTypeIconSprites[0];
				}
				else if (isRegenerate)
				{
					sprite = levelTypeIconSprites[1];
				}
				else
				{
					sprite = null;
				}
				levelTypeIconImg.gameObject.SetActive(sprite);
				if ((bool)sprite)
				{
					levelTypeIconImg.sprite = sprite;
				}
			}
			_RefreshUI(ResidueTime);
			_Countdown();
			_pauseNumber++;
			timeTex.DOPause();
		}

		public void AddTimeByAnim(int time, Action onCompleted = null)
		{
			timeTex.DOKill();
			int residueTime = ResidueTime;
			int num = residueTime + time;
			ResidueTime = num;
			timeTex.DOCounter(residueTime, num, 0.2f, _RefreshUI).OnComplete(delegate
			{
				_Countdown();
				if (_pauseNumber == 0)
				{
					timeTex.DOPlay();
				}
				else
				{
					timeTex.DOPause();
				}
				onCompleted?.Invoke();
			});
		}

		public void ClearTimeByStar(Vector3 position, Action callback)
		{
			if (ResidueTime == 0)
			{
				callback?.Invoke();
				return;
			}
			LevelMgr.Data.Difficulty.SetRemainingTime(ResidueTime);
			StartCoroutine(IETimeStar(position, callback));
		}

		private IEnumerator IETimeStar(Vector3 position, Action callback)
		{
			int animNumber = 0;
			Action starCallback = null;
			animNumber = ResidueTime / 25;
			if (ResidueTime % 25 > 0)
			{
				animNumber++;
			}
			int number = animNumber;
			Vector2 startPosition = base.transform.localPosition;
			Vector2 endPosition = base.transform.InverseTransformPoint(position);
			while (number > 0)
			{
				FlyItem flyItem = _stars.FirstOrDefault((FlyItem fd) => fd.isFree);
				if (!flyItem)
				{
					flyItem = LoadMgr.LoadRes<GameObject>(_startIconPath).GetComponent<FlyItem>();
					_stars.Add(flyItem);
				}
				ResidueTime = Mathf.Max(ResidueTime - 25, 0);
				_RefreshUI(ResidueTime);
				if (starCallback == null)
				{
					starCallback = delegate
					{
						GameMgr.I.chestControl.Add(1);
						if (animNumber > 1)
						{
							animNumber--;
						}
						else
						{
							callback?.Invoke();
						}
					};
				}
				flyItem.Fly2D(base.transform, startPosition, endPosition, starCallback);
				number--;
				yield return new WaitForSeconds(0.05f);
			}
		}

		private void _Countdown()
		{
			int residueTime = ResidueTime;
			timeTex.DOCounter(residueTime, 0, residueTime, _UpdateTime).OnComplete(delegate
			{
				DispatcherMgr.Emit("GameTime_End");
			});
		}

		private void _ChangeStatus(GameStatus status)
		{
			if (status == GameStatus.None)
			{
				return;
			}
			if (status == GameStatus.Playing)
			{
				_pauseNumber = Mathf.Max(_pauseNumber - 1, 0);
				if (_pauseNumber <= 0)
				{
					timeTex.DOPlay();
				}
			}
			else
			{
				_pauseNumber++;
				timeTex.DOPause();
			}
		}

		private void _InterstitialPause()
		{
			_isPlayingInterstitial = true;
			_pauseNumber++;
			timeTex.DOPause();
		}

		private void _InterstitialPlay()
		{
			if (_isPlayingInterstitial)
			{
				_isPlayingInterstitial = false;
				_pauseNumber = Mathf.Max(_pauseNumber - 1, 0);
				_adTime = 0;
				if (_pauseNumber <= 0)
				{
					timeTex.DOPlay();
				}
			}
		}

		private void _Pause()
		{
			_pauseNumber++;
			timeTex.DOPause();
		}

		private void _Play()
		{
			_pauseNumber = Mathf.Max(_pauseNumber - 1, 0);
			if (_pauseNumber <= 0)
			{
				timeTex.DOPlay();
			}
		}

		private void _UpdateTime(int time)
		{
			DispatcherMgr.Emit("GameTime_Change", ResidueTime - time);
			if (!DB.User.AdFree && DB.User.Curlv >= cf_const.gameInterstitialUnlockLv)
			{
				_adTime += ResidueTime - time;
				if (cf_const.gameInterstitialIntervalTime <= (float)_adTime)
				{
					LevelInterAd.ShowPrepare();
				}
			}
			ResidueTime = time;
			_RefreshUI(time);
		}

		private void _RefreshUI(int time)
		{
			timeTex.text = Util.Helper.FormatTimer(time, "mm:ss");
		}

		private void OnUserLevelRewardPlayInterstitialSuccess()
		{
			_adTime = 0;
		}
	}
}
