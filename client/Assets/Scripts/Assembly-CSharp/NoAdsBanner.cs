using Pop_Gift;
using UnityEngine;
using UnityEngine.UI;

public class NoAdsBanner : MonoBehaviour
{
	public GameObject noAdsBannerObj;

	public Text noAdsGoldTex;

	public Text noAdsPriceTex;

	private cfbt_Stores _noAdsCfg;

	private void Awake()
	{
		DispatcherMgr.On<GiftType>("BuySuccessCallBack", _iAPBuySuc);
	}

	public void ClickBuyNoAds(MButton btn)
	{
		if (_noAdsCfg == null)
		{
			return;
		}
		LogEvent.I.Ads_NoAdsOpen("Banner");
		SceneMgr.OpenWaiting(30f, 0f);
		if (!IAPManager.instance.Buy(_noAdsCfg))
		{
			SceneMgr.CloseWaiting();
		}
	}

	public bool JudgeShow()
	{
		Pop_Gift.GiftData popGift = GiftNoAdsManager.Instance.GetPopGift(true);
		_noAdsCfg = ((popGift != null) ? popGift.StoresConfig : null);
		return _noAdsCfg != null;
	}

	public void Show()
	{
		noAdsBannerObj.SetActive(_noAdsCfg != null);
		if (_noAdsCfg == null)
		{
			return;
		}
		if (_noAdsCfg.price == "")
		{
			noAdsPriceTex.text = string.Format("${0}", _noAdsCfg.usd);
		}
		else
		{
			noAdsPriceTex.text = _noAdsCfg.price;
		}
		int[,] array = _noAdsCfg.item.ToInt(2, ',', '|');
		for (int i = 0; i < array.GetLength(0); i++)
		{
			if (array[i, 0] == 0)
			{
				noAdsGoldTex.text = string.Format("x{0}", array[i, 1]);
				break;
			}
		}
	}

	private void _iAPBuySuc(GiftType type)
	{
		if (Global.pStores.AdFree)
		{
			_noAdsCfg = null;
			Show();
			LogEvent.I.NoAds_Banner_Buy();
		}
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _iAPBuySuc);
	}
}
