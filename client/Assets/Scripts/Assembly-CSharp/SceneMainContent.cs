using System;
using System.Collections;
using System.Collections.Generic;
using FunctionEject;
using Pop_Gift;
using Scene_Entrance;
using UnityEngine;
using UnityEngine.UI;

public class SceneMainContent : MonoBehaviour
{
	public static SceneMainContent instance;

	[SerializeField]
	private GameObject adsBtnObj;

	[SerializeField]
	private RectTransform trfTopBottom;

	[SerializeField]
	private GameObject MedalObj;

	[SerializeField]
	private GameObject medalRedObj;

	[SerializeField]
	private Text ttLevel;

	[SerializeField]
	private GameObject _adReward;

	[SerializeField]
	private GameObject _adInterstitial;

	[SerializeField]
	private SceneMainAddAnimationControl _addAnimationControl;

	[SerializeField]
	private GameObject _animationMask;

	[SerializeField]
	private RectTransform _leftEnterRoot;

	[SerializeField]
	private RectTransform _rightEnterRoot;

	[SerializeField]
	private ModelChestInMain _modelChestInMain;

	private List<MainIconAnimation> _mainIconAnimations = new List<MainIconAnimation>();

	private void Awake()
	{
		instance = this;
		if ((bool)MedalObj)
		{
			MedalObj.SetActive(value: false);
		}
		DispatcherMgr.On("IAPInitFinishedCallBack", _iAPInitSuc);
		DispatcherMgr.On<GiftType>("BuySuccessCallBack", _iAPBuySuc);
		DispatcherMgr.On("Update_Account", _UpdateData);
		DispatcherMgr.On("Update_User_MedalRed", _UpdateMedalRed);
		DispatcherMgr.On("GotRewardEvent", _iAPInitSuc);
	}

	private void Start()
	{
		DispatcherMgr.Emit("Unload_Unused_Assets", true);
		trfTopBottom.SetAnchorPosY(trfTopBottom.anchoredPosition.y + (1f - SceneMgr.scale.min) * 132f);
		_UpdateData();
	}

	private void _UpdateData()
	{
		GiftManager.Instance.RefreshData();
		_UpdateUI();
		StartCoroutine(DoPlayMainIconAnim(delegate
		{
			if (EjectMgr.I != null)
			{
				EjectMgr.I.Refresh(SceneMain.ins.isCheckEject);
			}
			SceneMain.ins.isCheckEject = false;
		}));
	}

	private void _UpdateUI()
	{
		ttLevel.text = $"{DB.User.Maxlv + 1}";
		MedalMgr.I.CheckUpMedal();
		_showAdsBtn();
		_UpdateMedalRed();
		if (EjectMgr.I != null)
		{
			EjectMgr.I.Init();
		}
		GetMainIconMoves();
	}

	public void ClickBtnAds(MButton btn)
	{
		GiftNoAdsManager.Instance.PopGift(delegate
		{
		});
		LogEvent.I.Ads_NoAdsOpen("Icon");
		LogEvent.I.HomeIconClick("RemoveAD");
	}

	public void ClickBtnLogo(MButton btn)
	{
		if (MedalGuideMain.I != null)
		{
			MedalGuideMain.I.DestroySelf();
			MedalGuideMain.I = null;
		}
		SceneMgr.AddTopNode<SelectMedalDialog>("Prefabs/Medal/SelectMedalDialog");
	}

	private void _iAPInitSuc()
	{
		_showAdsBtn();
	}

	private void _iAPBuySuc(GiftType type)
	{
		_showAdsBtn();
	}

	private void _showAdsBtn()
	{
		Pop_Gift.GiftData popGift = GiftNoAdsManager.Instance.GetPopGift();
		adsBtnObj.SetActive(popGift != null);
	}

	public void ShowRewardAd()
	{
		Global.ShowRewardedVideo(null, REWARD_PLACE.None);
	}

	public void ShowInterstitialAd()
	{
		Singleton<AdManager>.I.ShowInterstitial(null, INTER_PLACE.AdInterstitial);
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off("IAPInitFinishedCallBack", _iAPInitSuc);
		DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _iAPBuySuc);
		DispatcherMgr.Off("Update_Account", _UpdateData);
		DispatcherMgr.Off("Update_User_MedalRed", _UpdateMedalRed);
		DispatcherMgr.Off("GotRewardEvent", _iAPInitSuc);
		instance = null;
	}

	private void _UpdateMedalRed()
	{
		if (!(medalRedObj == null))
		{
			List<MedalType> types = new List<MedalType>
			{
				MedalType.Normall,
				MedalType.Special
			};
			bool active = MedalMgr.I.JudgeHasNewGetMedals(types);
			medalRedObj.SetActive(active);
		}
	}

	private void GetMainIconMoves()
	{
		_mainIconAnimations.Clear();
		List<MainIconAnimation> mainIconAnimations = GetMainIconAnimations(_leftEnterRoot, _addAnimationControl);
		if (mainIconAnimations.Count > 0)
		{
			_mainIconAnimations.AddRange(mainIconAnimations);
		}
		List<MainIconAnimation> mainIconAnimations2 = GetMainIconAnimations(_rightEnterRoot, _addAnimationControl);
		if (mainIconAnimations2.Count > 0)
		{
			_mainIconAnimations.AddRange(mainIconAnimations2);
		}
	}

	private List<MainIconAnimation> GetMainIconAnimations(Transform root, SceneMainAddAnimationControl addAnimationControl)
	{
		List<MainIconAnimation> list = new List<MainIconAnimation>();
		MainIconAnimation[] componentsInChildren = root.GetComponentsInChildren<MainIconAnimation>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MainIconAnimation mainIconAnimation = componentsInChildren[i];
			if (mainIconAnimation.gameObject.activeSelf)
			{
				mainIconAnimation.Init(addAnimationControl);
				list.Add(mainIconAnimation);
			}
		}
		return list;
	}

	private IEnumerator DoPlayMainIconAnim(Action<bool> action)
	{
		_animationMask.SetActive(value: true);
		yield return _modelChestInMain.DoPlayProgressAnimation();
		if (_mainIconAnimations.Count > 0)
		{
			bool flag = false;
			foreach (MainIconAnimation mainIconAnimation in _mainIconAnimations)
			{
				flag |= mainIconAnimation.DoAnimation();
			}
			if (flag)
			{
				yield return new WaitForSeconds(2f);
			}
		}
		_animationMask.SetActive(value: false);
		action?.Invoke(obj: true);
	}
}
