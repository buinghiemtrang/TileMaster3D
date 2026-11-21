using System;
using UnityEngine;

public class VictoriesActivityBanner : MonoBehaviour
{
	public Action<bool> onGo;

	public bool Show()
	{
		uint num = (uint)((DB.User.Maxlv + 1) % 10);
		bool value = false;
		if (num < 10 && (0x248 & (1 << (int)num)) != 0)
		{
			value = VictoriesActivityMgr.I.JudgeActivityDuring();
		}
		gameObject.SetActive(value);
		return value;
	}

	public void ClickGo(MButton btn)
	{
		btn.interactable = false;
		SceneMgr.SetData("Open" + FunctionType.Victories.ToString());
		if (onGo != null)
		{
			onGo(cf_const.victoriesActivityBannerJumpHomeLookAd);
		}
	}
}
