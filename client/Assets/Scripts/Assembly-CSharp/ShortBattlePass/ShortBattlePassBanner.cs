using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShortBattlePass
{
	public class ShortBattlePassBanner : MonoBehaviour
	{
		[SerializeField]
		private Text priceTex;

		[SerializeField]
		private MButton buyButton;

		private cfbt_Stores _storeConfig;

		private void Awake()
		{
			buyButton.onClick.AddListener(_ClickBtnBuy);
			DispatcherMgr.On<GiftType>("BuySuccessCallBack", _IapBuySuccess);
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _IapBuySuccess);
		}

		private void Start()
		{
			List<cfbt_Stores> cfgs = Global.pStores.GetCfgs(BuySpotType.ShortBattlePass);
			if (cfgs.Count > 0)
			{
				_storeConfig = cfgs[0];
				if (_storeConfig.price == "")
				{
					priceTex.text = string.Format("${0}", _storeConfig.usd);
				}
				else
				{
					priceTex.text = _storeConfig.price;
				}
			}
		}

		private void _ClickBtnBuy(MButton btn)
		{
			if (_storeConfig != null)
			{
				btn.interactable = false;
				SceneMgr.OpenWaiting(30f, 0f, () => btn.interactable = true);
				LogEvent.I.Track("Liitle_Master_Pass_Click")
					.Add("GradeNum", DB.ShortBattlePass.Data.Grade)
					.Add("Type", "banner")
					.CommonAttr(true)
					.Send();
				if (IAPManager.instance.Buy(_storeConfig))
				{
					LogEvent.I.Track("Little_Master_Pass_Buy")
						.Add("GradeNum", DB.ShortBattlePass.Data.Grade)
						.Add("Type", "banner")
						.CommonAttr(true)
						.Send();
				}
				else
				{
					SceneMgr.CloseWaiting();
					btn.interactable = true;
				}
			}
		}

		private void _IapBuySuccess(GiftType type)
		{
			if (type == GiftType.ShortBattlePass)
			{
				Singleton<ShortBattlePassMgr>.I.ShowBuySuccessRewardsDialog(() => gameObject.SetActive(false));
				LogEvent.I.Track("Little_Master_Pass_Callback")
					.Add("GradeNum", DB.ShortBattlePass.Data.Grade)
					.Add("Type", "Banner")
					.CommonAttr(true)
					.Send();
			}
		}
	}
}
