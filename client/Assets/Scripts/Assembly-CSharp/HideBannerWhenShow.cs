using UnityEngine;

public class HideBannerWhenShow : MonoBehaviour
{
	private bool _isTempHideBanner;

	private void Awake()
	{
		if (Singleton<AdManager>.I.isBannerShow)
		{
			Global.HideBanner();
			_isTempHideBanner = true;
		}
	}

	private void OnDestroy()
	{
		if (!_isTempHideBanner)
		{
			return;
		}
		Singleton<AdManager>.I.ShowBanner();
	}
}
