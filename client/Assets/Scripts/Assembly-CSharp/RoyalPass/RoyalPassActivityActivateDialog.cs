using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoyalPass
{
	public class RoyalPassActivityActivateDialog : DialogBase
	{
		public static RoyalPassActivityActivateDialog I;

		[SerializeField]
		private Text coinNumTex;

		[SerializeField]
		private Text propNumTex;

		[SerializeField]
		private Text energyUpLimitText;

		[SerializeField]
		private Image medalImg;

		[SerializeField]
		private Image rewardBoxImg;

		[SerializeField]
		private MButton closeBtn;

		[SerializeField]
		private MButton buyBtn;

		[SerializeField]
		private Text priceTex;

		public Action closedCallback;

		private int coinNum;

		private int propNum;

		private cfbt_Stores storeCfg;

		protected override void Awake()
		{
			base.Awake();
			I = this;
			closeBtn.onClick.AddListener(ClickClose);
			buyBtn.onClick.AddListener(_ClickBuy);
		}

		public void Show()
		{
			if (Singleton<RoyalPassActivityMgr>.I != null)
			{
				if (DataCfg.cfgRoyalPassActivityRewards != null)
				{
					for (int i = 0; i < DataCfg.cfgRoyalPassActivityRewards.Count; i++)
					{
						cfgRoyalPassActivityReward cfgRoyalPassActivityReward = DataCfg.cfgRoyalPassActivityRewards[i];
						_Compute(cfgRoyalPassActivityReward.freeReward);
						_Compute(cfgRoyalPassActivityReward.paidReward);
					}
				}
				coinNumTex.text = string.Format("x{0}", coinNum);
				propNumTex.text = string.Format("x{0}", propNum);
				Singleton<RoyalPassActivityMgr>.I.SetPaidRewardUI(medalImg, energyUpLimitText, rewardBoxImg);
				if (Global.pStores != null)
				{
					List<cfbt_Stores> cfgs = Global.pStores.GetCfgs((BuySpotType)5, (GiftType)6);
					if (cfgs != null && cfgs.Count > 0)
					{
						storeCfg = cfgs[0];
						if (storeCfg != null)
						{
							if (storeCfg.price.Equals(""))
							{
								priceTex.text = string.Format("${0}", storeCfg.usd);
							}
							else
							{
								priceTex.text = storeCfg.price;
							}
						}
					}
				}
				_Open();
			}
		}

		public void ClickClose(MButton btn)
		{
			if (!closeBtn.interactable)
			{
				return;
			}
			closeBtn.interactable = false;
			_Close(delegate
			{
				closedCallback?.Invoke();
			});
		}

		private void _ClickBuy(MButton btn)
		{
			int grade = 0;
			if (Singleton<RoyalPassActivityMgr>.I != null)
			{
				Singleton<RoyalPassActivityMgr>.I.GetGrade(out grade);
				LogEvent.I.Track("Master_Pass_Click").Add("GradeNum", grade).Add("Type", "Activity").CommonAttr().Send(false);
				if (storeCfg == null)
				{
					return;
				}
				SceneMgr.OpenWaiting(30f);
				if (IAPManager.instance.Buy(storeCfg))
				{
					LogEvent.I.Track("Master_Pass_Buy").Add("GradeNum", grade).Add("Type", "Activity").CommonAttr().Send(false);
				}
				else
				{
					SceneMgr.CloseWaiting();
				}
			}
		}

		private void _Compute(int[,] reward)
		{
			if (reward != null)
			{
				int length = reward.GetLength(0);
				for (int i = 0; i < length; i++)
				{
					int num = reward[i, 0];
					if (num < 16)
					{
						if (((1 << num) & 0xCC00) != 0)
						{
							propNum += reward[i, 1];
						}
						else if (num == 0)
						{
							coinNum += reward[i, 1];
						}
					}
				}
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			I = null;
		}
	}
}
