using System;
using UnityEngine;

public class GameParkBanner : MonoBehaviour
{
	public Action ONGo;

	public bool Show()
	{
		bool value = DB.User.Maxlv + 1 == cf_const.showGameParkBannerLv;
		gameObject.SetActive(value);
		return value;
	}

	public void ClickGo(MButton btn)
	{
		btn.interactable = false;
		SceneMgr.SetData("Open" + FunctionType.GamePark.ToString());
		if (ONGo != null)
		{
			ONGo();
		}
	}
}
