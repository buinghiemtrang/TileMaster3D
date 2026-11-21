using UnityEngine.UI;

public class MagnetDialog : DialogBase
{
	public MButton btnAds;

	public MButton btnGold;

	public MButton closeBtn;

	public Text ttGoldNum;

	public Text getGoldNumTex;

	public MagnetDialog()
	{
		fMaskAlpha = 0.8f;
	}

	public void Show()
	{
		btnAds.gameObject.SetActive(cf_const.magnetAdNum != 0);
		btnGold.gameObject.SetActive(0 < cf_const.magnetNeedGold);
		int surMagnetAdNum = DB.User.SurMagnetAdNum;
		btnAds.interactable = surMagnetAdNum != 0;
		ttGoldNum.text = cf_const.magnetNeedGold.ToString();
		getGoldNumTex.text = string.Format("x{0}", cf_const.magnetBuyOnceNum);
		float height = ((btnAds.gameObject.activeSelf && btnGold.gameObject.activeSelf) ? 780f : 663f);
		tfRoot.SetHeight(height);
		LogEvent.I.Track("Game_Magnet_Open").CommonAttr().Send();
		_Open();
	}

	public void ClickAd(MButton btn)
	{
		Global.ShowRewardedVideo(delegate(string info)
		{
			if (info == "succ")
			{
				if (DB.User.SurMagnetAdNum > 0)
				{
					DB.User.SurMagnetAdNum--;
				}
				DB.Bag.Modify(PropID.Magnet, cf_const.magnetBuyOnceNum, true, "BuyMagnet");
				_Close();
			}
		}, REWARD_PLACE.AdGameBooster);
	}

	public void ClickGold(MButton btn)
	{
		if (DB.Bag.GetProp(PropID.Money) < cf_const.magnetNeedGold)
		{
			Global.pStores.PopStoreDialog(GiftTriggerType.Gold, false);
			return;
		}
		btn.interactable = false;
		closeBtn.interactable = false;
		btnAds.interactable = false;
		_Close();
		DB.Bag.Modify(PropID.Money, -cf_const.magnetNeedGold, true, "BuyMagnet");
		DB.Bag.Modify(PropID.Magnet, cf_const.magnetBuyOnceNum, true, "BuyMagnet");
	}

	public void ClickClose(MButton btn)
	{
		_Close();
	}
}
