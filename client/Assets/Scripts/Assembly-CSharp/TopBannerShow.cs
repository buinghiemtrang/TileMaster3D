using FQDev.Time;
using UnityEngine;

public class TopBannerShow : MonoBehaviour
{
	private static int _showCount;

	private void Awake()
	{
		if (DB.User.Maxlv + 1 >= cf_banner_const.UnlockBannerLv && cf_banner_const.IsOpenTopBanner != 0)
		{
			_showCount = Mathf.Max(0, _showCount) + 1;
			Singleton<AdManager>.I.ShowBannerTop();
			Debug.LogError(string.Format("TopBanner Show: {0}", _showCount));
		}
	}

	private void OnDestroy()
	{
		if (DB.User.Maxlv + 1 >= cf_banner_const.UnlockBannerLv && cf_banner_const.IsOpenTopBanner != 0)
		{
			TimeManager.SetupInvoke(0.1f, delegate
			{
				_showCount--;
				if (_showCount < 1)
				{
					Singleton<AdManager>.I.HideBannerTop();
				}
				Debug.LogError(string.Format("TopBanner Hide: {0}", _showCount));
			});
		}
	}
}
