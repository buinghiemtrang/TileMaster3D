using System;
using UnityEngine;
using UnityEngine.UI;

public class GiftPropDialogItem : MonoBehaviour
{
	[SerializeField]
	private Image ImageLock;

	[SerializeField]
	private Image ImageBuy;

	[SerializeField]
	private Text priceTxt;

	[SerializeField]
	private GiftPopItem cellItem;

	[SerializeField]
	private Transform tfItemsRoot;

	[SerializeField]
	private MButton BtnBuy;

	[SerializeField]
	private GameObject _offTagObj;

	[SerializeField]
	private Text _offTagText;

	private cfbt_Gift giftCfg;

	private cfbt_Stores storeCfg;

	public Action<bool> stopCloseCallback;

	private bool bIsUnlock = true;

	public void Init(cfbt_Gift cfg, int stroreIndex)
	{
		giftCfg = cfg;
		storeCfg = GiftDataMgr.ins.GetStoreCfgByIndex(stroreIndex);
		cellItem.gameObject.SetActive(false);
		if (storeCfg.price.Equals("-"))
		{
			priceTxt.text = string.Format("${0}", storeCfg.usd);
		}
		else
		{
			priceTxt.text = storeCfg.price;
		}
		BtnBuy.interactable = false;
		SetOffTag(storeCfg.discount);
		UpdateItems();
		UpdateStatus();
	}

	private void ClearItems()
	{
		for (int num = tfItemsRoot.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(tfItemsRoot.GetChild(num).gameObject);
		}
	}

	private void UpdateItems()
	{
		ClearItems();
		int[,] array = storeCfg.item.ToInt(2, ',', '|');
		int length = array.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			GiftPopItem giftPopItem = UnityEngine.Object.Instantiate<GiftPopItem>(cellItem, tfItemsRoot);
			giftPopItem.gameObject.SetActive(true);
			giftPopItem.Init(array[i, 0], array[i, 1]);
		}
	}

	private void UpdateStatus()
	{
		bool flag;
		if (!giftCfg.isUnlockAll)
		{
			ImageBuy.gameObject.SetActive(false);
			BtnBuy.gameObject.SetActive(true);
			int giftUnlockData = DB.GiftStalls.GetGiftUnlockData(giftCfg.id);
			int num = Array.IndexOf(giftCfg.storeIndex.ToInt(','), storeCfg.index);
			flag = num < giftUnlockData;
			ImageLock.gameObject.SetActive(giftUnlockData < num);
			bIsUnlock = num <= giftUnlockData;
		}
		else
		{
			ImageLock.gameObject.SetActive(false);
			string key = giftCfg.id.ToString();
			if (!DB.GiftStalls.GiftBuyData.ContainsKey(key))
			{
				flag = false;
			}
			else
			{
				flag = Array.IndexOf(DB.GiftStalls.GiftBuyData[key].ToInt(','), storeCfg.index) != -1;
			}
		}
		ImageBuy.gameObject.SetActive(flag);
		BtnBuy.gameObject.SetActive(!flag);
	}

	public void Buy(MButton btn)
	{
		if (!bIsUnlock)
		{
			SceneMgr.AddTopNode<GiftTipsDialog>("Prefabs/Gift/GiftTipsDialog", false).Show(btn.transform);
			return;
		}
		SceneMgr.OpenWaiting(30f, 0f, delegate
		{
			if (stopCloseCallback != null)
			{
				stopCloseCallback(false);
			}
		});
		if (stopCloseCallback != null)
		{
			stopCloseCallback(true);
		}
		string text = Global.pStores.ProductId(storeCfg.id);
		LogEvent.I.Track("Gift_Click").Add("Commodity_ID", text).Add("Type", "Activity").CommonAttr(true).Send(false);
		if (IAPManager.instance.Buy(storeCfg))
		{
			LogEvent.I.Track("Gift_Buy").Add("Commodity_ID", text).CommonAttr(true).Send(false);
			return;
		}
		SceneMgr.CloseWaiting();
		if (stopCloseCallback != null)
		{
			stopCloseCallback(false);
		}
	}

	private void SetOffTag(int discount)
	{
		if ((bool)_offTagObj)
		{
			_offTagObj.SetActive(0 < discount);
			if (0 < discount)
			{
				_offTagText.text = string.Format("{0}%", discount);
			}
		}
	}
}
