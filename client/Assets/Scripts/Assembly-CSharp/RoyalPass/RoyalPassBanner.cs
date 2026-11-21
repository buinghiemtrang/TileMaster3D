using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoyalPass
{
	public class RoyalPassBanner : MonoBehaviour
	{
		[SerializeField]
		private Text textCurEnergyLimit;

		[SerializeField]
		private Text textNewEnergyLimit;

		[SerializeField]
		private Text textPrice;

		[SerializeField]
		private MButton btnBuy;

		public Action<bool> stopCloseCallback;

		private cfbt_Stores _storeCfg;

		private void Awake()
		{
			DispatcherMgr.On<GiftType>("BuySuccessCallBack", _IAPBuySuc);
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _IAPBuySuc);
		}

		private void Start()
		{
			textCurEnergyLimit.text = cf_const.maxEnergyRecover.ToString();
			Singleton<RoyalPassActivityMgr>.I.SetPaidRewardUI(null, textNewEnergyLimit, null, null);
			List<cfbt_Stores> cfgs = Global.pStores.GetCfgs(BuySpotType.BattleToken);
			if (cfgs != null && cfgs.Count > 0)
			{
				_storeCfg = cfgs[0];
			}
			if (_storeCfg != null)
			{
				if (_storeCfg.price == "")
				{
					textPrice.text = string.Format("${0}", _storeCfg.usd);
				}
				else
				{
					textPrice.text = _storeCfg.price;
				}
				btnBuy.interactable = true;
			}
			else
			{
				btnBuy.interactable = false;
			}
		}

		public void ClickBtnBuy(MButton btn)
		{
			btn.interactable = false;
			SceneMgr.OpenWaiting(30f, 0f, () => btn.interactable = true);
			Global.pStores.ProductId(_storeCfg.id);
			Singleton<RoyalPassActivityMgr>.I.GetGrade(out var grade);
			LogEvent.I.Track("Master_Pass_Click").Add("GradeNum", grade).Add("Type", "banner").CommonAttr(true).Send();
			if (IAPManager.instance.Buy(_storeCfg))
			{
				LogEvent.I.Track("Master_Pass_Buy").Add("GradeNum", grade).Add("Type", "banner").CommonAttr(true).Send();
			}
			else
			{
				SceneMgr.CloseWaiting();
				btn.interactable = true;
			}
		}

		private void _IAPBuySuc(GiftType type)
		{
			if (type == GiftType.BattleToken)
			{
				Singleton<RoyalPassActivityMgr>.I.ShowBuySuccessRewardsDialog(() => gameObject.SetActive(false));
				LogEvent.I.Track("Master_Pass_Callback")
					.Add("GradeNum", DB.RoyalPassActivity.CurGrade)
					.Add("Type", "Banner")
					.CommonAttr(true)
					.Send();
			}
		}
	}
}
