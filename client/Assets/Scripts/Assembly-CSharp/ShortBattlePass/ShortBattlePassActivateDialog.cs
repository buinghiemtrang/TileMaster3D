using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShortBattlePass
{
	public class ShortBattlePassActivateDialog : DialogBase
	{
		[SerializeField]
		private MButton closeButton;

		[SerializeField]
		private MButton buyButton;

		[SerializeField]
		private Text priceTex;

		public Action CloseCallback;

		private cfbt_Stores _storeConfig;

		protected override void Awake()
		{
			base.Awake();
			closeButton.onClick.AddListener(ClickClose);
			buyButton.onClick.AddListener(_ClickBuy);
		}

		public void Show()
		{
			List<cfbt_Stores> cfgs = Global.pStores.GetCfgs(BuySpotType.ShortBattlePass, GiftType.ShortBattlePass);
			if (cfgs.Count > 0)
			{
				_storeConfig = cfgs[0];
				if (_storeConfig.price.Equals(""))
				{
					priceTex.text = string.Format("${0}", _storeConfig.usd);
				}
				else
				{
					priceTex.text = _storeConfig.price;
				}
			}
			_Open();
		}

		public void ClickClose(MButton button)
		{
			if (closeButton.interactable)
			{
				closeButton.interactable = false;
				_Close(CloseCallback);
			}
		}

		private void _ClickBuy(MButton button)
		{
			LogEvent.I.Track("Liitle_Master_Pass_Click").Add("GradeNum", DB.ShortBattlePass.Data.Grade).Add("Type", "Activity").CommonAttr(true)
				.Send(false);
			if (_storeConfig == null)
			{
				return;
			}
			SceneMgr.OpenWaiting(30f);
			if (IAPManager.instance.Buy(_storeConfig))
			{
				LogEvent.I.Track("Little_Master_Pass_Buy").Add("GradeNum", DB.ShortBattlePass.Data.Grade).Add("Type", "Activity").CommonAttr(true)
					.Send(false);
			}
			else
			{
				SceneMgr.CloseWaiting();
			}
		}

		public static ShortBattlePassActivateDialog Create()
		{
			ShortBattlePassActivateDialog shortBattlePassActivateDialog = SceneMgr.AddTopNode<ShortBattlePassActivateDialog>("Prefabs/Activity/ShortBattlePass/ShortBattlePassActivateDialog");
			shortBattlePassActivateDialog.Show();
			return shortBattlePassActivateDialog;
		}
	}
}
