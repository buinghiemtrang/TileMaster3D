using System;
using DG.Tweening;
using Festival;
using UnityEngine;
using UnityEngine.UI;

public class GiftPop : MonoBehaviour
{
	[SerializeField]
	private GiftPopItem cellItem;

	[SerializeField]
	private Text textTitle;

	[SerializeField]
	private GridLayoutCenter pGridLayout;

	[SerializeField]
	private MButton btnClose;

	[SerializeField]
	private MButton btnBuy;

	[SerializeField]
	private Text textTimer;

	[SerializeField]
	private Text textPrice;

	[SerializeField]
	private Text textOldPrice;

	[SerializeField]
	private Transform tfBg;

	[SerializeField]
	private Image imageIcon;

	[SerializeField]
	private GameObject _offTagObj;

	[SerializeField]
	private Text _offTagText;

	public Action<bool> func_close;

	private cfbt_Gift cfg;

	private GiftData data;

	private cfbt_Stores storeCfg;

	private Tweener tw;

	public static GiftPop ins;

	public static void pop(cfbt_Gift _cfg, GiftData _data, cfbt_Stores _storeCfg)
	{
		if (_data == null)
		{
			return;
		}
		ins = SceneMgr.AddTopNode<GiftPop>("Prefabs/Gift/" + _cfg.popPrefab, false);
		ins.cfg = _cfg;
		ins.data = _data;
		ins.storeCfg = _storeCfg;
	}

	public void init(cfbt_Gift _cfg, GiftData _data, cfbt_Stores _storeCfg)
	{
		cfg = _cfg;
		data = _data;
		storeCfg = _storeCfg;
	}

	private void Start()
	{
		textTitle.text = storeCfg.name;
		int[,] array = storeCfg.item.ToInt(2, ',', '|');
		int length = array.GetLength(0);
		if (tfBg != null && length > 1)
		{
			pGridLayout.transform.parent.SetHeight((length < 5) ? 150f : 260f);
		}
		for (int i = 0; i < length; i++)
		{
			GiftPopItem giftPopItem = UnityEngine.Object.Instantiate<GiftPopItem>(cellItem);
			giftPopItem.gameObject.SetActive(true);
			giftPopItem.Init(array[i, 0], array[i, 1]);
			pGridLayout.AddItem(giftPopItem.transform);
		}
		pGridLayout.SetWidth(4);
		if (imageIcon != null)
		{
			Sprite giftIcon = GetGiftIcon(cfg.icon);
			if (giftIcon != null)
			{
				imageIcon.sprite = giftIcon;
				imageIcon.SetNativeSize();
			}
		}
		CDTimer();
		SetOffTag(storeCfg.discount);
		_RefreshPrice();
		LogEvent.I.Track("Gift_Show").Add("Commodity_ID", Global.pStores.ProductId(storeCfg.id)).CommonAttr(true).Send(false);
	}

	private void OnClickClose(MButton button)
	{
		Close(true);
	}

	private void OnClickBuy(MButton button)
	{
		string text = Global.pStores.ProductId(storeCfg.id);
		LogEvent.I.Track("Gift_Click").Add("Commodity_ID", text).Add("Type", "Activity").CommonAttr(true).Send(false);
		Close(false);
		SceneMgr.OpenWaiting(30f, 0f, delegate
		{
		});
		if (IAPManager.instance.Buy(storeCfg))
		{
			LogEvent.I.Track("Gift_Buy").Add("Commodity_ID", text).CommonAttr(true).Send(false);
		}
		else
		{
			SceneMgr.CloseWaiting();
		}
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

	private void Awake()
	{
		cellItem.gameObject.SetActive(false);
		btnClose.onClick.AddListener(OnClickClose);
		btnBuy.onClick.AddListener(OnClickBuy);
		DispatcherMgr.On("IAPInitFinishedCallBack", _IAPInitSuc);
	}

	private void OnDestroy()
	{
		ins = null;
		if (tw != null)
		{
			tw.Kill(false);
		}
		DispatcherMgr.Off("IAPInitFinishedCallBack", _IAPInitSuc);
	}

	private void _RefreshPrice()
	{
		if (storeCfg != null && storeCfg.price != null)
		{
			if (storeCfg.price == "-")
			{
				btnBuy.interactable = false;
				textPrice.text = string.Format("${0}", storeCfg.usd);
				if (textOldPrice != null)
				{
					textOldPrice.transform.parent.gameObject.SetActive(false);
				}
			}
			else
			{
				btnBuy.interactable = true;
				textPrice.text = storeCfg.price;
				if (textOldPrice != null)
				{
					textOldPrice.transform.parent.gameObject.SetActive(storeCfg.discount != 0);
					if (storeCfg.discount != 0)
					{
						textOldPrice.text = GiftDataMgr.ins.MoneyStrSale(storeCfg.price, (float)storeCfg.discount);
					}
				}
			}
		}
	}

	private void _IAPInitSuc()
	{
		_RefreshPrice();
	}

	private void SetOffTag(int discount)
	{
		if (_offTagObj != null)
		{
			_offTagObj.SetActive(discount > 0);
			if (discount > 0)
			{
				_offTagText.text = string.Format("{0}%", discount);
			}
		}
	}

	public static Sprite GetGiftIcon(string iconPath)
	{
		return Singleton<FestivalMgr>.I.LoadAsset<Sprite>("Festival/Gift/{0}/" + iconPath);
	}
}
