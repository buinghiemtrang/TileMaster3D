using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class GiftButtonsBox : MonoBehaviour
{
	public Transform tfContent;

	public GiftButton cellGift;

	public static GiftButtonsBox ins;

	private void Awake()
	{
		IAPManager.instance.initSuccEndCall += OnIAPInItSuccEnd;
		DispatcherMgr.On("Update_Account", RefreshButton);
		if (cellGift != null && cellGift.gameObject != null)
		{
			cellGift.gameObject.SetActive(false);
			ins = this;
			if (ins != null)
			{
				RefreshButton();
			}
		}
	}

	private void OnDestroy()
	{
		IAPManager.instance.initSuccEndCall -= OnIAPInItSuccEnd;
		DispatcherMgr.Off("Update_Account", RefreshButton);
		ins = null;
	}

	public static void Create(Transform tfRoot)
	{
		if (ins != null)
		{
			return;
		}
		GiftButtonsBox giftButtonsBox = LoadMgr.LoadRes<GiftButtonsBox>("Prefabs/Gift/ButtonsBox");
		giftButtonsBox.transform.SetParent(tfRoot, false);
	}

	private void OnIAPInItSuccEnd()
	{
		if (SceneMgr.showingDialogs.Count == 0 && GiftPop.ins == null)
		{
			RefreshButton();
			if (DB.GiftStalls.OfflineDays < 3 && !RecallMgr.I.JudgeAutoEject())
			{
				GiftDataMgr.ins.TryPopGift();
			}
		}
	}

	public void RefreshButton()
	{
		GiftDataMgr.ins.GetTriggerMain();
	}

	public static void BuyCallBack(cfbt_Stores storesCfg)
	{
		if (storesCfg.sType == 3 && ins != null)
		{
			ins.refreshButton(storesCfg);
		}
	}

	private void _DestroyGiftsBtn(GiftButton[] btns = null)
	{
		if (btns == null)
		{
			btns = tfContent.GetComponentsInChildren<GiftButton>();
		}
		for (int i = 0; i < btns.Length; i++)
		{
			UnityEngine.Object.Destroy(btns[i].gameObject);
		}
	}

	private void refreshButton(cfbt_Stores storeCfg)
	{
		GiftButton[] componentsInChildren = tfContent.GetComponentsInChildren<GiftButton>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			GiftButton giftButton = componentsInChildren[i];
			cfbt_Gift cfg = giftButton.cfg;
			int[] array = cfg.storeIndex.ToInt(',');
			if (Array.IndexOf(array, storeCfg.index) != -1)
			{
				if (array.Length == 1)
				{
					UnityEngine.Object.Destroy(giftButton.gameObject);
					return;
				}
				if (cfg.isUnlockAll)
				{
					string text = cfg.id.ToString();
					if (DB.GiftStalls.GiftBuyData.ContainsKey(text))
					{
						if (DB.GiftStalls.GiftBuyData[text].ToInt(',').Length == array.Length)
						{
							UnityEngine.Object.Destroy(giftButton.gameObject);
							return;
						}
					}
				}
				else
				{
					if (DB.GiftStalls.GetGiftUnlockData(cfg.id) >= array.Length)
					{
						UnityEngine.Object.Destroy(giftButton.gameObject);
						return;
					}
				}
			}
		}
	}

	public static Tweener CDTimer(Text timerTxt, DateTime time, TweenCallback action)
	{
		TimeSpan timeSpan = time - DateTime.Now;
		double totalSeconds = timeSpan.TotalSeconds;
		if (totalSeconds >= 0.0)
		{
			int num = Convert.ToInt32(totalSeconds);
			if (num != 0)
			{
				timerTxt.gameObject.SetActive(true);
				return TweenMgr.NumberTo((float)num, num, 0, delegate(int va)
				{
					TimeSpan timeSpan2 = new TimeSpan(0, 0, va);
					timerTxt.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan2.Hours + timeSpan2.Days * 24, timeSpan2.Minutes, timeSpan2.Seconds);
				}).SetEase(Ease.Linear).OnComplete(action);
			}
		}
		timerTxt.gameObject.SetActive(false);
		if (action != null)
		{
			action();
		}
		return null;
	}
}
