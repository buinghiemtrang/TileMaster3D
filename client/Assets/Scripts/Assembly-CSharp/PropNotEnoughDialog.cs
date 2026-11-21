using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropNotEnoughDialog : DialogBase
{
	[SerializeField]
	private Image iconImg;

	[SerializeField]
	private Text numTex;

	private PropID propId;

	private cfbt_Stores cfg;

	private bool isPauseGame;

	public Action<bool> cback;

	private void OnEnable()
	{
		DispatcherMgr.On<GiftType>("BuySuccessCallBack", _iAPBuySuc);
	}

	private void OnDisable()
	{
		DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _iAPBuySuc);
	}

	public void Show(PropID propId, bool isPause = true)
	{
		this.propId = propId;
		isPauseGame = isPause;
		List<cfbt_Stores> cfgsByTypeAndPropID = Global.pStores.GetCfgsByTypeAndPropID(BuySpotType.BuyPropTips, propId);
		cfg = cfgsByTypeAndPropID[0];
		if (isPause)
		{
			DispatcherMgr.Emit("GamePause", true, "PropNotEnough");
		}
		iconImg.sprite = LoadMgr.LoadRes<Sprite>(string.Format("Props/prop{0}", (int)propId));
		iconImg.SetNativeSize();
		numTex.text = string.Format("x{0}", cfg.item.ToInt('|')[1]);
		_Open();
	}

	private void _iAPBuySuc(GiftType type)
	{
		CloseSelf(isBuySuccess: true);
	}

	public void ClickFull(MButton btn)
	{
		SceneMgr.OpenWaiting(30f, 0f);
		string value = Global.pStores.ProductId(cfg.id);
		LogEvent.I.Track("Items_Shop_Click").Add("Commodity_ID", value).CommonAttr().Send();
		if (!IAPManager.instance.Buy(cfg))
		{
			SceneMgr.CloseWaiting();
		}
	}

	public void ClickClose(MButton btn)
	{
		CloseSelf(isBuySuccess: false);
	}

	private void CloseSelf(bool isBuySuccess)
	{
		if (isPauseGame)
		{
			DispatcherMgr.Emit("GamePause", false, "PropNotEnough");
		}
		_Close(delegate
		{
			if (cback != null)
			{
				cback(isBuySuccess);
			}
		});
	}
}
