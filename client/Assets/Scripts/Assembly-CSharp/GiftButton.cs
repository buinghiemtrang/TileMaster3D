using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GiftButton : MonoBehaviour
{
	public Text timerTxt;

	public MButton openBtn;

	public Image icon;

	public cfbt_Gift cfg;

	private GiftData data;

	private cfbt_Stores storeCfg;

	private Tweener tw;

	public void init(cfbt_Gift _cfg, int index = 0)
	{
		cfg = _cfg;
		data = DB.GiftStalls.GetGiftDataById(cfg.id);
		int[] array = cfg.storeIndex.ToInt(',');
		storeCfg = GiftDataMgr.ins.GetStoreCfgByIndex(array[0]);
		string iconPath = string.Format("GiftIcon_{0}", (index >= 1) ? 1 : 2);
		if (array.Length != 1)
		{
			iconPath = "GiftIcon_3";
		}
		Sprite giftIcon = GiftPop.GetGiftIcon(iconPath);
		if (giftIcon != null)
		{
			icon.sprite = giftIcon;
			icon.SetNativeSize();
		}
		CDTimer();
	}

	private void Start()
	{
		openBtn.onClick.AddListener(openClick);
	}

	private void openClick(MButton m)
	{
		int[] array = cfg.storeIndex.ToInt(',');
		if (array.Length == 1)
		{
			GiftPop.pop(cfg, data, storeCfg);
			if (storeCfg != null)
			{
				LogEvent.I.HomeIconClick(storeCfg.name);
			}
			return;
		}
		GiftPropDialog.pop(cfg, data);
		string text = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			cfbt_Stores storeCfgByIndex = GiftDataMgr.ins.GetStoreCfgByIndex(array[i]);
			if (storeCfgByIndex != null)
			{
				text = text + storeCfgByIndex.name + ((i < array.Length - 1) ? "|" : "");
			}
		}
		LogEvent.I.HomeIconClick(text);
	}

	private void CDTimer()
	{
		if (tw != null)
		{
			tw.Kill();
		}
		tw = GiftButtonsBox.CDTimer(timerTxt, data.timer, delegate
		{
			if (GiftButtonsBox.ins != null)
			{
				GiftButtonsBox.ins.RefreshButton();
			}
		});
	}

	private void OnDestroy()
	{
		if (tw != null)
		{
			tw.Kill();
		}
	}
}
