using System;
using UnityEngine;
using UnityEngine.UI;

public class AchiveDetails : DialogBase
{
	public GiftPopItem cellItem;

	public Image ImageIcon;

	public Image ItemBg;

	public Text TextDesc;

	public Text TextNext;

	public RectTransform rtBigBg;

	public RectTransform rtSmallBg;

	public MButton BtnReceive;

	public GridLayoutCenter pGridLayout;

	public Action receive_callback;

	private cfg_achive _cfg;

	private AchiveStatus _status;

	private int iCurLevel;

	private int iLevelCount;

	public void InitInfo(cfg_achive cfg, AchiveStatus status, int curLevel)
	{
		_cfg = cfg;
		_status = status;
		iCurLevel = curLevel;
		initUI();
	}

	public void initUI()
	{
		BtnReceive.gameObject.SetActive(_status == AchiveStatus.Finished);
		ImageIcon.sprite = LoadMgr.LoadRes<Sprite>("AchiveIcon/" + _cfg.Icon);
		ImageIcon.SetNativeSize();
		int[] array = _cfg.Values.ToInt('|');
		iLevelCount = array.Length;
		TextNext.gameObject.SetActive(iCurLevel < iLevelCount - 1);
		int num = (iCurLevel < iLevelCount) ? iCurLevel : (iCurLevel - 1);
		TextDesc.SetLKey(_cfg.DescLangId, array[num]);
		if (iCurLevel < iLevelCount - 1)
		{
			TextNext.SetLBy(array[iCurLevel + 1]);
		}
		UpdateReward();
	}

	protected void UpdateReward()
	{
		string[] array = _cfg.RewardTypesAndroid.Split('|');
		string[] array2 = _cfg.RewardAndroid.Split('|');
		int num = iCurLevel - ((iLevelCount <= iCurLevel) ? 1 : 0);
		int[] array3 = array[num].ToInt(',');
		int[] array4 = array2[num].ToInt(',');
		int num2 = array3[0];
		ItemBg.gameObject.SetActive(num2 != -1);
		UpdateBgHeight(num2 != -1);
		if (num2 != -1)
		{
			pGridLayout.Clear();
			for (int i = 0; i < array3.Length; i++)
			{
				GiftPopItem giftPopItem = UnityEngine.Object.Instantiate(cellItem);
				giftPopItem.gameObject.SetActive(value: true);
				giftPopItem.Init(array3[i], array4[i]);
				pGridLayout.AddItem(giftPopItem.transform);
			}
		}
	}

	public void UpdateBgHeight(bool bShowReward)
	{
		Vector2 sizeDelta = rtBigBg.sizeDelta;
		Vector2 sizeDelta2 = rtSmallBg.sizeDelta;
		RectTransform component = ItemBg.transform.GetComponent<RectTransform>();
		Vector2 sizeDelta3 = component.sizeDelta;
		float num = bShowReward ? 0f : sizeDelta3.y;
		rtSmallBg.sizeDelta = new Vector2(sizeDelta2.x, sizeDelta2.y - num);
		rtBigBg.sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y - num);
	}

	public void OnClickClose(MButton button)
	{
		button.interactable = false;
		_Close();
	}

	public void OnClickReceive(MButton button)
	{
		button.interactable = false;
		_Close(delegate
		{
			receive_callback?.Invoke();
		});
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
