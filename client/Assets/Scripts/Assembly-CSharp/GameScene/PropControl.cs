using System;
using System.Collections.Generic;
using DG.Tweening;
using GameScene_Function;
using GameScene_Level;
using UnityEngine;

namespace GameScene
{
	public class PropControl : MonoBehaviour
	{
		public UpdatePropExpend pPropHint;

		public UpdatePropExpend pPropFreeze;

		public UpdatePropExpend pPropBack;

		public UpdatePropExpend pPropRefresh;

		private Dictionary<PropID, UpdatePropExpend> updatePropExpendDic;

		private bool isCanUseProp;

		private void Awake()
		{
			DispatcherMgr.On("GameScene_Guide_SpecialElement_Start", NotCanUseProp);
			DispatcherMgr.On("GameScene_Guide_SpecialElement_End", CanUseProp);
		}

		private void CanUseProp()
		{
			isCanUseProp = true;
		}

		private void NotCanUseProp()
		{
			isCanUseProp = false;
		}

		public void Init()
		{
			isCanUseProp = true;
			if (LevelMgr.Data.Type == LevelType.Ordinary)
			{
				int num = DB.User.Curlv + 1;
				pPropHint.Init(num < cf_const.unlockHintLv, cf_const.unlockHintLv);
				pPropBack.Init(num < cf_const.unlockBackLv, cf_const.unlockBackLv);
				pPropRefresh.Init(num < cf_const.unlockRefreshLv, cf_const.unlockRefreshLv);
				pPropFreeze.Init(num < cf_const.unlockFreezeLv, cf_const.unlockFreezeLv);
			}
			else
			{
				pPropHint.Init(false, 0);
				pPropBack.Init(false, 0);
				pPropRefresh.Init(false, 0);
				pPropFreeze.Init(false, 0);
			}
			updatePropExpendDic = new Dictionary<PropID, UpdatePropExpend>();
			updatePropExpendDic.Add(PropID.Hint, pPropHint);
			updatePropExpendDic.Add(PropID.Freeze, pPropFreeze);
			updatePropExpendDic.Add(PropID.Back, pPropBack);
			updatePropExpendDic.Add(PropID.Refresh, pPropRefresh);
		}

		private void _Use(PropID tp)
		{
			switch (tp)
			{
			case PropID.Hint:
				GameMgr.I.RandomHintControl.Use(false);
				LogEvent.I.AF_PropHintUse();
				break;
			case PropID.Freeze:
				GameMgr.I.FreezeControl.Use(cf_const.nFreezeTime);
				break;
			case PropID.Back:
				BackMgr.I.Back();
				break;
			case PropID.Refresh:
				GameMgr.I.RefreshControl.Use(false);
				break;
			}
		}

		private void _checkUse(PropID tp, REWARD_PLACE place)
		{
			if (!isCanUseProp || GameMgr.I.GameStatus != GameStatus.Playing)
			{
				return;
			}
			UpdatePropExpend updatePropExpend = updatePropExpendDic[tp];
			if (updatePropExpend.isLock)
			{
				return;
			}
			if (!updatePropExpend.isFree && DB.Bag.GetProp(tp) < 1)
			{
				BuyDialog(tp, place).func_callback_adv = delegate
				{
					bool flag;
					switch (tp)
					{
					case PropID.Hint:
						flag = GameMgr.I.RandomHintControl.JudgeCanUse();
						break;
					case PropID.Freeze:
						flag = GameMgr.I.FreezeControl.JudgeCanUse();
						break;
					case PropID.Back:
						flag = BackMgr.I.Judge();
						break;
					case PropID.Refresh:
						flag = GameMgr.I.RefreshControl.JudgeCanUse();
						break;
					default:
						flag = true;
						break;
					}
					if (!flag)
					{
						_modify(tp, 1);
					}
					else
					{
						_Use(tp);
					}
				};
				return;
			}
			bool flag2;
			switch (tp)
			{
			case PropID.Hint:
				flag2 = GameMgr.I.RandomHintControl.JudgeCanUse();
				break;
			case PropID.Freeze:
				flag2 = GameMgr.I.FreezeControl.JudgeCanUse();
				break;
			case PropID.Back:
				flag2 = BackMgr.I.Judge();
				break;
			case PropID.Refresh:
				flag2 = GameMgr.I.RefreshControl.JudgeCanUse();
				break;
			default:
				flag2 = true;
				break;
			}
			if (flag2)
			{
				_modify(tp, -1);
				_Use(tp);
			}
		}

		public void ClickBtnHint(MButton btn)
		{
			_checkUse(PropID.Hint, REWARD_PLACE.AdHint);
		}

		public void ClickBtnFreeze(MButton btn)
		{
			_checkUse(PropID.Freeze, REWARD_PLACE.AdFreeze);
		}

		public void ClickBtnBack(MButton btn)
		{
			_checkUse(PropID.Back, REWARD_PLACE.AdBack);
		}

		public void ClickBtnRefresh(MButton btn)
		{
			_checkUse(PropID.Refresh, REWARD_PLACE.AdRefresh);
		}

		public void ShowBackHand(bool isShow)
		{
			if (isShow)
			{
				pPropBack.isFree = true;
			}
			else
			{
				if (!pPropBack.isFree)
				{
					return;
				}
				pPropBack.isFree = false;
			}
			pPropBack.Refresh();
		}

		public void ShowHand()
		{
			// pseudo-C (RVA 0x10cd6e0): chỉ đọc isFree của pPropHint/pPropRefresh rồi return, không có tác dụng phụ
		}

		private void _modify(PropID pId, int num)
		{
			UpdatePropExpend updatePropExpend;
			switch (pId)
			{
			case PropID.Hint:
				updatePropExpend = pPropHint;
				break;
			case PropID.Freeze:
				updatePropExpend = pPropFreeze;
				break;
			case PropID.Back:
				updatePropExpend = pPropBack;
				break;
			case PropID.Refresh:
				updatePropExpend = pPropRefresh;
				break;
			default:
				return;
			}
			if (updatePropExpend.isFree)
			{
				updatePropExpend.isFree = false;
				ShowHand();
				num = 0;
			}
			DB.Bag.Modify(pId, num, "Game");
			if (num != 0 && DB.Bag.GetProp(pId) == 0 && DB.User.Curlv + 1 >= cf_const.lvBuyProp && IAPManager.instance.isInitialized)
			{
				SceneMgr.AddTopNode<PropNotEnoughDialog>("Prefabs/PropNotEnoughDialog").Show(pId);
			}
		}

		private BuyPropDialog BuyDialog(PropID kpropid, REWARD_PLACE kPlace)
		{
			GiftTriggerType giftTriggerType = GetGiftTriggerType(kpropid);
			BuyPropDialog buyPropDialog = SceneMgr.AddTopNode<BuyPropDialog>("Prefabs/BuyPropDialog");
			buyPropDialog.func_callback_close = delegate(bool isTip)
			{
				bool flag = GiftDataMgr.ins.TryPopGiftImmediately(giftTriggerType, null, true);
				if (isTip && !flag)
				{
					DispatcherMgr.Emit("GamePause", true, "PropControl");
					Global.CreateMsgBoxDialog("Language53", delegate
					{
						DispatcherMgr.Emit("GamePause", false, "PropControl");
					}, false, "");
				}
			};
			buyPropDialog.Show(kpropid, kPlace);
			return buyPropDialog;
		}

		private void OnDestroy()
		{
			DOTween.Kill("Prop_Freeze_Fade");
			DispatcherMgr.Off("GameScene_Guide_SpecialElement_Start", NotCanUseProp);
			DispatcherMgr.Off("GameScene_Guide_SpecialElement_End", CanUseProp);
		}

		public GiftTriggerType GetGiftTriggerType(PropID kpropid)
		{
			switch (kpropid)
			{
			case PropID.Hint:
				return GiftTriggerType.HintProp;
			case PropID.Freeze:
				return GiftTriggerType.FreezeProp;
			case PropID.Back:
				return GiftTriggerType.BackProp;
			case PropID.Refresh:
				return GiftTriggerType.RefreshProp;
			default:
				return GiftTriggerType.CommonProp;
			}
		}
	}
}
