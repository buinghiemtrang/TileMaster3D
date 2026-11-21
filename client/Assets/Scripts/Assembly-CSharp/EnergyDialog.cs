using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyDialog : MonoBehaviour
{
	[SerializeField]
	private DialogShow pDialog;

	[SerializeField]
	private BuyPropItem[] items;

	[SerializeField]
	private Text goldTex;

	[SerializeField]
	private Text goldEnergyNumTex;

	[SerializeField]
	private Text adEnergyNumTex;

	public Action cback;

	public Action goldInsufficientCall;

	private void Awake()
	{
		DispatcherMgr.On<GiftType>("BuySuccessCallBack", _BuySucc);
	}

	private void Start()
	{
		goldTex.text = cf_const.buyEnergyNeedGold.ToString();
		goldEnergyNumTex.text = string.Format("+{0}", cf_const.buyOnecGetEnergyNum);
		adEnergyNumTex.text = string.Format("+{0}", cf_const.watchOnecAdGetEnergyNum);
		List<cfbt_Stores> cfgsByTypeAndPropID = Global.pStores.GetCfgsByTypeAndPropID(BuySpotType.BuyProp, PropID.EnergyTime);
		if (items == null || cfgsByTypeAndPropID == null)
		{
			return;
		}
		int count = cfgsByTypeAndPropID.Count;
		int length = items.Length;
		if (length < count)
		{
			Debug.LogError(string.Format("number error {0} {1}", length, count));
		}
		for (int i = 0; i < length; i++)
		{
			items[i].gameObject.SetActive(i < count);
			if (i < count)
			{
				items[i].Init(cfgsByTypeAndPropID[i]);
			}
		}
	}

	public void ClickBtnAdv(MButton button)
	{
		Global.ShowRewardedVideo(delegate(string info)
		{
			if (info == "succ")
			{
				DB.Bag.ModifyEnergy(cf_const.watchOnecAdGetEnergyNum, true, "Ads");
				pDialog._Close();
				cback();
			}
		}, REWARD_PLACE.AdEnergy);
	}

	public void ClickBuy(MButton btn)
	{
		pDialog._Close();
		int prop = DB.Bag.GetProp(PropID.Money);
		int buyEnergyNeedGold = cf_const.buyEnergyNeedGold;
		if (prop < buyEnergyNeedGold)
		{
			if (goldInsufficientCall != null)
			{
				goldInsufficientCall();
			}
			return;
		}
		DB.Bag.Modify(PropID.Money, -buyEnergyNeedGold, true, "BuyEnergy");
		DB.Bag.ModifyEnergy(cf_const.buyOnecGetEnergyNum, true, "Gold");
		if (cback != null)
		{
			cback();
		}
	}

	public void ClickClose(MButton btn)
	{
		pDialog._Close();
		GiftDataMgr.ins.TryPopGiftImmediately(GiftTriggerType.EnergyProp);
	}

	private void _BuySucc(GiftType type)
	{
		if (type == (GiftType)21)
		{
			pDialog._Close();
			if (cback != null)
			{
				cback();
			}
		}
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _BuySucc);
	}

	public static EnergyDialog Create(Action cback, Action goldInsufficientCall)
	{
		EnergyDialog energyDialog = SceneMgr.AddTopNode<EnergyDialog>("Prefabs/EnergyDialog");
		if (energyDialog != null)
		{
			energyDialog.cback = cback;
			energyDialog.goldInsufficientCall = goldInsufficientCall;
		}
		return energyDialog;
	}
}
