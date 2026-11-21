using UnityEngine;
using UnityEngine.UI;

public class BuyPropItem : MonoBehaviour
{
	[SerializeField]
	private MButton buyBtn;

	[SerializeField]
	private Text TextNum;

	[SerializeField]
	private Image ImageIcon;

	[SerializeField]
	private GameObject _offTagObj;

	[SerializeField]
	private Text _offTagText;

	private cfbt_Stores cfg;

	public void Init(cfbt_Stores cfg)
	{
		this.cfg = cfg;
		int[] array = cfg.item.ToInt('|');
		Sprite sprite = LoadMgr.LoadRes<Sprite>(string.Format("Props/prop{0}", array[0]));
		ImageIcon.sprite = sprite;
		ImageIcon.SetNativeSize();
		if (array[0] == 21)
		{
			TextNum.text = string.Format("{0}h", array[1]);
		}
		else
		{
			TextNum.text = string.Format("x{0}", array[1]);
		}
		SetOffTag(cfg.discount);
	}

	public void Buy(MButton btn)
	{
		SceneMgr.OpenWaiting(30f, 0f);
		string value = Global.pStores.ProductId(cfg.id);
		LogEvent.I.Track("Items_Shop_Click").Add("Commodity_ID", value).CommonAttr().Send();
		if (!IAPManager.instance.Buy(cfg))
		{
			SceneMgr.CloseWaiting();
		}
	}

	private void SetOffTag(int discount)
	{
		if (!(_offTagObj == null))
		{
			_offTagObj.SetActive(discount > 0);
			if (discount > 0)
			{
				_offTagText.text = string.Format("{0}%", discount);
			}
		}
	}
}
