using System;
using Pop_Gift;
using UnityEngine;

public static class LevelInterAd
{
	public static Action OnPlaySuc;

	private static bool _isCanPlay;

	public static void Reset()
	{
		_isCanPlay = false;
	}

	public static void ShowPrepare()
	{
		if (DB.User.AdFree || _isCanPlay)
		{
			return;
		}
		_isCanPlay = true;
		UnityEngine.Debug.Log("LevelInterAd ShowPrepare");
	}

	public static void TryShow()
	{
		if (_isCanPlay && !DB.User.AdFree && Singleton<AdManager>.I.IsInterstitialAdOk(true))
		{
			UnityEngine.Debug.Log("LevelInterAd TryShow Success");
			_isCanPlay = false;
			DispatcherMgr.Emit("InGame_Interstitial_Start");
			Global.ShowInterstitial(delegate(string result)
			{
				OnPlaySuc.InvokeSafe();
				if (result == "succ")
				{
					GiftManager.Instance.TryAutoPopGift(GiftTriggerPosition.InGameInterstitialPlaySuccessAfter, (Action)delegate
					{
						DispatcherMgr.Emit("InGame_Interstitial_End");
					});
				}
				else
				{
					DispatcherMgr.Emit("InGame_Interstitial_End");
				}
			}, INTER_PLACE.AdGameInterstitial);
		}
	}
}
