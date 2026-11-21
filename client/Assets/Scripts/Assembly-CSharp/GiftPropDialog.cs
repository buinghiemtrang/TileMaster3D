using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GiftPropDialog : MonoBehaviour
{
	[SerializeField]
	private GiftPropDialogItem[] items;

	[SerializeField]
	private Text textTimer;

	[SerializeField]
	private Text textTitle;

	private cfbt_Gift cfg;

	private GiftData data;

	private Tweener tw;

	public static GiftPropDialog ins;

	public Action<bool> func_close;

	public static void pop(cfbt_Gift _cfg, GiftData _data)
	{
		if (_data != null)
		{
			ins = SceneMgr.AddTopNode<GiftPropDialog>("Prefabs/Gift/GiftPropDialog", false);
			ins.cfg = _cfg;
			ins.data = _data;
		}
	}

	public void init(cfbt_Gift _cfg, GiftData _data)
	{
		cfg = _cfg;
		data = _data;
	}

	private void Start()
	{
		UpdateItems();
		if (cfg.continueTime == -1)
		{
			textTimer.transform.parent.gameObject.SetActive(false);
		}
		else
		{
			GiftData giftData = data;
			textTimer.transform.parent.gameObject.SetActive(giftData != null);
			if (giftData != null)
			{
				CDTimer();
			}
		}
		DispatcherMgr.On<GiftType>("BuySuccessCallBack", OnBuySuccess);
		DispatcherMgr.On("IAPInitFinishedCallBack", _IAPInitSuc);
		DialogShow component = GetComponent<DialogShow>();
		if ((bool)component)
		{
			component._Open();
		}
		int[] array = cfg.storeIndex.ToInt(',');
		if (array.Length != 0)
		{
			cfbt_Stores storeCfgByIndex = GiftDataMgr.ins.GetStoreCfgByIndex(array[0]);
			if (storeCfgByIndex != null)
			{
				LogEvent.I.Track("Gift_Show").Add("Commodity_ID", Global.pStores.ProductId(storeCfgByIndex.id)).CommonAttr(true).Send(false);
			}
		}
	}

	private void UpdateItems()
	{
		int num = items.Length;
		int[] array = cfg.storeIndex.ToInt(',');
		int num2 = array.Length;
		if (!cfg.isUnlockAll)
		{
			DB.GiftStalls.GetGiftUnlockData(cfg.id);
			for (int i = 0; i < num; i++)
			{
				items[i].gameObject.SetActive(i < num2);
				if (i < num2)
				{
					items[i].Init(cfg, array[i]);
				}
			}
		}
		else
		{
			for (int j = 0; j < num; j++)
			{
				items[j].gameObject.SetActive(j < num2);
				if (j < num2)
				{
					items[j].Init(cfg, array[j]);
				}
			}
		}
		if (array.Length != 0)
		{
			textTitle.text = GiftDataMgr.ins.GetStoreCfgByIndex(array[0]).name;
		}
	}

	private void CDTimer()
	{
		if (tw != null)
		{
			tw.Kill(false);
		}
		tw = GiftButtonsBox.CDTimer(textTimer, data.timer, delegate
		{
			if (GiftButtonsBox.ins != null)
			{
				GiftButtonsBox.ins.RefreshButton();
			}
			Close(true);
		});
	}

	private void Close(bool isNoBuy)
	{
		transform.GetComponent<DialogShow>()._Close(delegate
		{
			if (func_close != null)
			{
				func_close(isNoBuy);
			}
		});
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off<GiftType>("BuySuccessCallBack", OnBuySuccess);
		DispatcherMgr.Off("IAPInitFinishedCallBack", _IAPInitSuc);
		ins = null;
		if (tw != null)
		{
			tw.Kill(false);
		}
	}

	public void ClickClose(MButton button)
	{
		Close(true);
	}

	private void OnBuySuccess(GiftType type)
	{
		if (type != GiftType.GiftPop && type != GiftType.PropHint && type != GiftType.PropFreeze && type != GiftType.PropMagnet && type != GiftType.PropBack && type != GiftType.PropRefresh && type != GiftType.PropEnergyTime)
		{
			return;
		}
		int[] array = cfg.storeIndex.ToInt(',');
		if (!cfg.isUnlockAll)
		{
			if (array.Length <= DB.GiftStalls.GetGiftUnlockData(cfg.id))
			{
				Close(false);
				return;
			}
		}
		else
		{
			string key = cfg.id.ToString();
			if (DB.GiftStalls.GiftBuyData.ContainsKey(key) && array.Length <= DB.GiftStalls.GiftBuyData[key].ToInt(',').Length)
			{
				Close(false);
				return;
			}
		}
		UpdateItems();
	}

	private void _IAPInitSuc()
	{
		UpdateItems();
	}
}
