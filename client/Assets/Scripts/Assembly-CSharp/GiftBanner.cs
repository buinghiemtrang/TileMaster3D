using UnityEngine;
using UnityEngine.UI;

public class GiftBanner : MonoBehaviour
{
	[SerializeField]
	private GameObject bannerObj;

	[SerializeField]
	private MButton buyBtn;

	[SerializeField]
	private Transform rewardItemPrefab;

	[SerializeField]
	private GridLayoutCenter layout;

	[SerializeField]
	private Text priceTex;

	private cfbt_Stores _storeCfg;

	private void Awake()
	{
		buyBtn.onClick.AddListener(_ClickBuy);
		DispatcherMgr.On<GiftType>("BuySuccessCallBack", _BuySuc);
	}

	public void Show(cfbt_Stores cfg)
	{
		_storeCfg = cfg;
		bannerObj.SetActive(cfg != null);
		if (cfg == null)
		{
			return;
		}
		Text text = priceTex;
		string text2 = ((!cfg.price.Equals("")) ? cfg.price : string.Format("${0}", cfg.usd));
		text.text = text2;
		int[,] array = cfg.item.ToInt(2, ',', '|');
		int length = array.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			Transform transform = UnityEngine.Object.Instantiate(rewardItemPrefab);
			transform.gameObject.SetActive(value: true);
			layout.AddItem(transform);
			transform.GetComponent<OrdinaryRewardItem>().Init((PropID)array[i, 0], array[i, 1]);
		}
		layout.SetWidth((length == 5) ? 3 : 4);
	}

	private void _ClickBuy(MButton btn)
	{
		if (_storeCfg != null)
		{
			string value = Global.pStores.ProductId(_storeCfg.id);
			LogEvent.I.Track("Gift_Click").Add("Commodity_ID", value).Add("Type", "Banner")
				.CommonAttr()
				.Send();
			SceneMgr.OpenWaiting(30f);
			if (!IAPManager.instance.Buy(_storeCfg))
			{
				SceneMgr.CloseWaiting();
			}
		}
	}

	private void _BuySuc(GiftType type)
	{
		_storeCfg = null;
		Show(_storeCfg);
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _BuySuc);
	}
}
