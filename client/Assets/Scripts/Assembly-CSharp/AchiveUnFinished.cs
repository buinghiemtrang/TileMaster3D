using System;
using UnityEngine;
using UnityEngine.UI;

public class AchiveUnFinished : DialogBase
{
	public Image ImageIcon;
	public Text TextDesc;
	public AchiveStarItem cellItem;
	public GridLayoutCenter pLayout;
	public MButton BtnFinish;
	public Action finish_callback;
	private int iCurLevel;

	public void InitInfo(cfg_achive cfg, AchiveStatus status, int curLevel)
	{
		iCurLevel = curLevel;
		ImageIcon.sprite = LoadMgr.LoadRes<Sprite>("AchiveIcon/" + cfg.Icon);
		ImageIcon.SetNativeSize();
		int[] array = cfg.Values.ToInt('|');
		int num = array.Length;
		int num2 = (iCurLevel < num) ? iCurLevel : (iCurLevel - 1);
		TextDesc.SetLKey(cfg.DescLangId, array[num2]);
		UpdateStar(num, iCurLevel);
	}

	protected void UpdateStar(int levelCount, int curLevel)
	{
		pLayout.Clear();
		for (int i = 1; i <= levelCount; i++)
		{
			AchiveStarItem achiveStarItem = UnityEngine.Object.Instantiate(cellItem);
			achiveStarItem.gameObject.SetActive(value: true);
			achiveStarItem.starIcon.SetActive(i <= curLevel);
			pLayout.AddItem(achiveStarItem.transform);
		}
	}

	public void OnClickClose(MButton button)
	{
		button.interactable = false;
		_Close();
	}

	public void OnClickFinish(MButton button)
	{
		button.interactable = false;
		_Close(delegate
		{
			finish_callback?.Invoke();
		});
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
