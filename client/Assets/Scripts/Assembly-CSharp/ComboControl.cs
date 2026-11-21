using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameScene;
using GameScene_Element;
using UnityEngine;
using UnityEngine.UI;

public class ComboControl : MonoBehaviour
{
	[SerializeField]
	private Text comboTex;

	[SerializeField]
	private Image barImg;

	[HideInInspector]
	public int comboNumber;

	[HideInInspector]
	public int animNumber;

	private List<FlyItem> _stars;

	private const string StartIconPath = "Effect/star_icon";

	private int _starNumber;

	private int _pauseNumber;

	private bool _isPlayingInterstitial;

	private void Awake()
	{
		_stars = new List<FlyItem>();
		DispatcherMgr.On<GameStatus>("GameStatus_Change", _ChangeStatus);
		// ghép key/handler theo asm 0x11deae8: thanh ghi x26/x27 (Freeze_Start/_Pause), x24/x25 (Freeze_End/_Play), x23 (Interstitial_End và StartFuse đều dùng _InterstitialPlay)
		DispatcherMgr.On("GameScene_Freeze_Start", _Pause);
		DispatcherMgr.On("GameScene_Freeze_End", _Play);
		DispatcherMgr.On("InGame_Interstitial_Start", _InterstitialPause);
		DispatcherMgr.On("InGame_Interstitial_End", _InterstitialPlay);
		DispatcherMgr.On("GameScene_Platform_StartFuse", _InterstitialPlay);
	}

	private void OnDestroy()
	{
		barImg.DOKill();
		DOTween.Kill("ComboAni_TextCombo");
		DispatcherMgr.Off<GameStatus>("GameStatus_Change", _ChangeStatus);
		// ghép như Awake (asm 0x11dedc0)
		DispatcherMgr.Off("GameScene_Freeze_Start", _Pause);
		DispatcherMgr.Off("GameScene_Freeze_End", _Play);
		DispatcherMgr.Off("InGame_Interstitial_Start", _InterstitialPause);
		DispatcherMgr.Off("InGame_Interstitial_End", _InterstitialPlay);
		DispatcherMgr.Off("GameScene_Platform_StartFuse", _InterstitialPlay);
	}

	public void Init()
	{
		_HideUI();
		barImg.DOKill();
		DOTween.Kill("ComboAni_TextCombo");
	}

	private void _ChangeStatus(GameStatus status)
	{
		if (status != GameStatus.None)
		{
			if (status == GameStatus.Playing)
			{
				_Play();
			}
			else
			{
				_Pause();
			}
		}
	}

	private void _HideUI()
	{
		comboNumber = -1;
		comboTex.text = "";
		barImg.fillAmount = 0f;
	}

	public void Combo()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		comboNumber++;
		barImg.DOKill();
		_starNumber = 1;
		if (comboNumber > 0)
		{
			_Countdown();
			int num = comboNumber;
			if (num > 7)
			{
				num = 8;
			}
			AudioMgr.I.PlaySound(string.Format("combo{0}", num));
			comboTex.SetLBy(comboNumber);
			DOTween.Kill("ComboAni_TextCombo");
			comboTex.transform.ScaleMaxToMin(1.2f, 1f, 0.2f).SetId("ComboAni_textCombo");
			_starNumber = Mathf.Min(10, comboNumber) + 1;
		}
	}

	private void _Countdown()
	{
		barImg.DOKill();
		barImg.fillAmount = 1f;
		float duration = Mathf.Clamp(cf_const.comboMaxTime - cf_const.comboReduceTime * (float)comboNumber, cf_const.comboMinTime, cf_const.comboMaxTime);
		// OnComplete = _HideUI (asm 0x11df6bc: TweenCallback..ctor(this, MD_ComboControl$$_HideUI))
		barImg.DOCounter(duration, 0f, duration, delegate(float value)
		{
			barImg.fillAmount = Mathf.Clamp01(value / duration);
		}).SetEase(Ease.Linear).OnComplete(_HideUI);
		if (_pauseNumber > 0)
		{
			DOTween.Pause(barImg);
		}
	}

	private void _InterstitialPause()
	{
		_isPlayingInterstitial = true;
		_pauseNumber++;
		DOTween.Pause(barImg);
	}

	private void _InterstitialPlay()
	{
		if (_isPlayingInterstitial)
		{
			_isPlayingInterstitial = false;
			int num = _pauseNumber - 1;
			_pauseNumber = Mathf.Max(num, 0);
			if (num <= 0)
			{
				DOTween.Play(barImg);
			}
		}
	}

	private void _Pause()
	{
		_pauseNumber++;
		DOTween.Pause(barImg);
	}

	private void _Play()
	{
		int num = _pauseNumber - 1;
		_pauseNumber = Mathf.Max(num, 0);
		if (num <= 0)
		{
			DOTween.Play(barImg);
		}
	}

	public void FlyStart(Vector3 startPosition, bool isMoreStart = false)
	{
		int starNumber = _starNumber;
		if (starNumber != 0)
		{
			animNumber += starNumber;
			StartCoroutine(IEStar(starNumber, startPosition, isMoreStart));
		}
	}

	private IEnumerator IEStar(int number, Vector3 startPosition, bool isMoreStart = false)
	{
		while (number > 0)
		{
			FlyItem flyItem = _stars.FirstOrDefault((FlyItem fd) => fd.isFree);
			if (!flyItem)
			{
				flyItem = LoadMgr.LoadRes<GameObject>("Effect/star_icon").GetComponent<FlyItem>();
				_stars.Add(flyItem);
			}
			int costNumber = ((!isMoreStart) ? number : 1);
			flyItem.Fly(ElementsMgr.I.starNode, startPosition, GameMgr.I.chestControl.tfStar.position, delegate
			{
				GameMgr.I.chestControl.Add(costNumber);
				animNumber--;
			});
			number -= costNumber;
			yield return new WaitForSeconds(0.05f);
		}
	}
}
