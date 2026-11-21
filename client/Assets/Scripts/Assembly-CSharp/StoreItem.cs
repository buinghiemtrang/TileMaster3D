using System;
using DG.Tweening;
using Festival;
using RoyalPass;
using UnityEngine;
using UnityEngine.UI;

public class StoreItem : MonoBehaviour
{
	[Serializable]
	public struct PropInfo
	{
		public GameObject obj;

		public Image icon;

		public Text numTex;
	}

	[SerializeField]
	private GameObject discountObj;

	[SerializeField]
	private GameObject vipObj;

	[SerializeField]
	private GameObject cdObj;

	[SerializeField]
	private Text nameTx;

	[SerializeField]
	private Text discountValue;

	[SerializeField]
	private Text vipTx;

	[SerializeField]
	private GameObject ads;

	[SerializeField]
	private GameObject adsObj;

	[SerializeField]
	private MButton buyBtn;

	[SerializeField]
	private Text priceTex;

	[SerializeField]
	private Text originalPriceTex;

	[SerializeField]
	private PropInfo[] pInfos;

	[SerializeField]
	private PropInfo coinInfo;

	[SerializeField]
	private PropInfo strengthInfo;

	[SerializeField]
	private Transform itemsTran;

	[SerializeField]
	private Text cdTex;

	[SerializeField]
	private Transform royalPassActivityNodeTran;

	[SerializeField]
	private Transform shortBattlePassTran;

	[SerializeField]
	private Image itemIcon;

	[SerializeField]
	private GameObject saleObj;

	[SerializeField]
	private Text saleText;

	private bool isCanMultipleCtrl;

	public Action<bool> stopCloseCallback;

	public Action refreshCallback;

	private cfbt_Stores cfg;

	private cfbt_Gift giftCfg;

	private Tweener tw;

	public void Init(cfbt_Stores cfg, cfbt_Gift giftCfg, float vipMultiple = 0f)
	{
		this.cfg = cfg;
		this.giftCfg = giftCfg;
		if (originalPriceTex)
		{
			if (cfg != null && cfg.gType == 4)
			{
				if (cfg.discount != 0 && cfg.price != "")
				{
					originalPriceTex.text = GiftDataMgr.ins.MoneyStrSale(cfg.price, cfg.discount);
				}
				else
				{
					originalPriceTex.transform.parent.gameObject.SetActive(false);
				}
			}
		}
		_CDTimer();
		if (discountObj)
		{
			if (cfg == null || cfg.discount == 0)
			{
				discountObj.SetActive(false);
			}
			else
			{
				discountValue.text = string.Format("{0}%", cfg.discount);
			}
		}
		if (vipObj)
		{
			if (vipMultiple == 0f)
			{
				vipObj.SetActive(false);
			}
			else
			{
				vipObj.SetActive(true);
				vipTx.text = string.Format("X{0}", vipMultiple + 1f);
			}
		}
		if (cfg != null)
		{
			if (cfg.gType == 6)
			{
				royalPassActivityNodeTran.DestroysInChildren();
				RoyalPassActivityStoreItem royalPassActivityStoreItem = Singleton<FestivalMgr>.I.LoadAndInstantiate<RoyalPassActivityStoreItem>("Prefabs/Activity/RoyalPass/{0}_RoyalPassActivityStoreItem");
				royalPassActivityStoreItem.transform.SetParent(royalPassActivityNodeTran, false);
				if (royalPassActivityStoreItem)
				{
					royalPassActivityStoreItem.Init();
				}
			}
			if (cfg.price != null)
			{
				if (cfg.price == "")
				{
					buyBtn.interactable = false;
					priceTex.text = string.Format("${0}", cfg.usd);
				}
				else
				{
					buyBtn.interactable = true;
					priceTex.text = cfg.price;
				}
				if (nameTx)
				{
					nameTx.text = cfg.name;
				}
				if (cfg.sType != 5 && cfg.sType != 8)
				{
					int[,] array = cfg.item.ToInt(2, ',', '|');
					int length = array.GetLength(0);
					if (ads)
					{
						if (length < 1)
						{
							ads.SetActive(false);
							adsObj.SetActive(false);
							itemsTran.SetLocPosY(45f);
						}
						else
						{
							bool flag = false;
							for (int i = 0; i < length; i++)
							{
								if (array[i, 0] == 20)
								{
									ads.SetActive(!Global.pStores.AdFree);
									adsObj.SetActive(!Global.pStores.AdFree);
									itemsTran.SetLocPosY(Global.pStores.AdFree ? 45f : 13f);
									flag = true;
								}
							}
							if (!flag)
							{
								ads.SetActive(false);
								adsObj.SetActive(false);
								itemsTran.SetLocPosY(45f);
							}
						}
					}
					if (cfg.sType == 6)
					{
						if (coinInfo.numTex)
						{
							if (array[0, 0] == 21)
							{
								coinInfo.numTex.text = string.Format("{0}h", (int)((float)array[0, 1] * vipMultiple + (float)array[0, 1]));
							}
							else
							{
								coinInfo.numTex.text = string.Format("x{0}", (int)((float)array[0, 1] * vipMultiple + (float)array[0, 1]));
							}
						}
					}
					else
					{
						bool hasEnergy = false;
						int num = 0;
						for (int j = 0; j < length; j++)
						{
							int propId = array[j, 0];
							if (propId == 21)
							{
								if (strengthInfo.numTex)
								{
									strengthInfo.numTex.text = string.Format("{0}h", (int)((float)array[j, 1] * vipMultiple + (float)array[j, 1]));
									hasEnergy = true;
								}
							}
							else if (propId != 20)
							{
								if (propId == 0)
								{
									if (coinInfo.numTex)
									{
										coinInfo.numTex.text = string.Format("{0}", (int)((float)array[j, 1] * vipMultiple + (float)array[j, 1]));
									}
								}
								else if (pInfos != null && num < pInfos.Length)
								{
									pInfos[num].numTex.text = string.Format("x{0}", (int)((float)array[j, 1] * vipMultiple + (float)array[j, 1]));
									pInfos[num].icon.sprite = Resources.Load<Sprite>(string.Format("Props/prop{0}", propId));
									num++;
								}
							}
						}
						if (strengthInfo.obj)
						{
							strengthInfo.obj.SetActive(hasEnergy);
						}
						if (pInfos != null && num < pInfos.Length)
						{
							if (num < 3)
							{
								pInfos[num].obj.transform.parent.gameObject.SetActive(false);
							}
							for (int k = num; k < pInfos.Length; k++)
							{
								pInfos[k].obj.SetActive(false);
							}
						}
					}
					isCanMultipleCtrl = false;
					if (saleObj != null)
					{
						if (Global.pStores != null && Global.pStores.storeMultipleCtrl != null)
						{
							isCanMultipleCtrl = Global.pStores.storeMultipleCtrl.HasBuyCount(cfg);
							if (isCanMultipleCtrl)
							{
								saleText.text = "Reward X\ufe0f " + (cf_const.storeMultiple + 1).ToString();
							}
							saleObj.SetActive(isCanMultipleCtrl);
						}
					}
				}
			}
		}
	}

	public void Buy(MButton btn)
	{
		SceneMgr.OpenWaiting(30f, 0f, () => stopCloseCallback?.Invoke(false));
		if (Global.pStores != null && Global.pStores.storeMultipleCtrl != null)
		{
			Global.pStores.storeMultipleCtrl.buyPlace = (BuyPlace)(isCanMultipleCtrl ? 0 : 1);
			stopCloseCallback?.Invoke(true);
			if (cfg != null)
			{
				string productId = Global.pStores.ProductId(cfg.id);
				LogEvent.I.Track("Shoping_Commodity_Click")
					.Add("Commodity_ID", productId)
					.Add("Commodity_State", isCanMultipleCtrl ? "Doubled" : "Normal")
					.IsNew()
					.Send(false);
				if (cfg.gType == 6)
				{
					Singleton<RoyalPassActivityMgr>.I.GetGrade(out var grade);
					LogEvent.I.Track("Master_Pass_Click")
						.Add("GradeNum", grade)
						.Add("Type", "shop")
						.CommonAttr(true)
						.Send(false);
				}
				if (cfg.gType == 22)
				{
					LogEvent.I.Track("Liitle_Master_Pass_Click")
						.Add("GradeNum", DB.ShortBattlePass.Data.Grade)
						.Add("Type", "shop")
						.CommonAttr(true)
						.Send(false);
				}
				if (IAPManager.instance.Buy(cfg))
				{
					LogEvent.I.Track("Shoping_Commodity_Buy")
						.Add("Commodity_ID", productId)
						.Add("Commodity_State", isCanMultipleCtrl ? "Doubled" : "Normal")
						.IsNew()
						.Send(false);
					if (cfg.gType == 22)
					{
						LogEvent.I.Track("Little_Master_Pass_Buy")
							.Add("GradeNum", DB.ShortBattlePass.Data.Grade)
							.Add("Type", "shop")
							.CommonAttr(true)
							.Send(false);
					}
					else if (cfg.gType == 6)
					{
						Singleton<RoyalPassActivityMgr>.I.GetGrade(out var grade2);
						LogEvent.I.Track("Master_Pass_Buy")
							.Add("GradeNum", grade2)
							.Add("Type", "shop")
							.CommonAttr(true)
							.Send(false);
					}
					else if (cfg.gType == 2)
					{
						LogEvent.I.Ads_NoAdsOpen("Shop");
					}
				}
				else
				{
					SceneMgr.CloseWaiting();
					stopCloseCallback?.Invoke(false);
				}
			}
		}
	}

	public void LoadItemIcon(string file)
	{
		itemIcon.sprite = LoadMgr.LoadSprite(file);
	}

	public void LoadItemIcon(Sprite sprite)
	{
		itemIcon.sprite = sprite;
		itemIcon.SetNativeSize();
	}

	private void _CDTimer()
	{
		if (!cdObj)
		{
			return;
		}
		if (cdTex && giftCfg != null)
		{
			GiftData giftDataById = DB.GiftStalls.GetGiftDataById(giftCfg.id);
			if (giftDataById == null)
			{
				return;
			}
			if (tw != null)
			{
				tw.Kill(false);
				tw = null;
			}
			tw = GiftButtonsBox.CDTimer(cdTex, giftDataById.timer, delegate
			{
				refreshCallback?.Invoke();
			});
		}
		else
		{
			cdObj.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		if (tw != null)
		{
			tw.Kill(false);
			tw = null;
		}
	}
}
