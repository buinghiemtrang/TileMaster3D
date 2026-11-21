using GameScene;
using Pop_Gift;
using UnityEngine;

public class BannerCloseButton : MonoBehaviour
{
	[SerializeField]
	private GameObject _root;

	[SerializeField]
	private MButton _closeButton;

	private void Awake()
	{
		_closeButton.onClick.AddListener(OnClickCloseButton);
		DispatcherMgr.On<bool>("BannerAdShowState", OnBannerAdShowState);
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off<bool>("BannerAdShowState", OnBannerAdShowState);
	}

	private void OnClickCloseButton(MButton button)
	{
		if (GameMgr.I.GameStatus == GameStatus.Playing)
		{
			GiftNoAdsManager.Instance.PopGift(delegate
			{
			});
		}
	}

	private void OnBannerAdShowState(bool isDisplaying)
	{
		_root.SetActive(GiftNoAdsManager.Instance.GetPopGift() != null && isDisplaying);
	}
}
