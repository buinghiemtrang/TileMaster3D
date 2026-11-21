using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectMedalGroupItem : MonoBehaviour
{
	[SerializeField]
	private SelectMedalCelltem[] items;

	[SerializeField]
	private Transform tfTips;

	[SerializeField]
	private Image imageArrow;

	[SerializeField]
	private Text textDesc;

	private int groupIndex;

	private int curCellIndex;

	private bool bShowTips;

	private MedalGroupData medalGroupData;

	private void Awake()
	{
		if (tfTips != null)
		{
			tfTips.gameObject.SetActive(value: false);
		}
	}

	public void Init(MedalGroupData data)
	{
		medalGroupData = data;
		for (int i = 0; i < 3; i++)
		{
			bool flag = i < medalGroupData.medalDatas.Count;
			items[i].gameObject.SetActive(flag);
			if (flag)
			{
				items[i].Init(medalGroupData.groupIndex, medalGroupData.medalDatas[i]);
			}
		}
		// UNVERIFIED: the first argument of ShowTips is not loaded in the original asm (ignored by ShowTips); groupIndex is assumed
		ShowTips(medalGroupData.groupIndex, medalGroupData.cellIndexOfShowTips);
	}

	private void ShowTips(int groupIndex, int cellIndex = -1)
	{
		if (tfTips != null)
		{
			tfTips.gameObject.SetActive(cellIndex != -1);
		}
		if (cellIndex == -1)
		{
			return;
		}
		if (imageArrow != null)
		{
			Vector3 localPosition = imageArrow.transform.localPosition;
			localPosition.x = cellIndex switch
			{
				0 => -150f,
				1 => 0f,
				_ => 150f,
			};
			imageArrow.transform.localPosition = localPosition;
		}
		cfg_medal cfg = medalGroupData.medalDatas[cellIndex].cfg;
		if (cfg != null)
		{
			textDesc.SetLKey(cfg.Desc, (cfg.Type == 1) ? new object[1] { cfg.Count } : Array.Empty<object>());
		}
	}
}
