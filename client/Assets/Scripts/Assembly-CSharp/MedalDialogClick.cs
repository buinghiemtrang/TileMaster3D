using System;
using UnityEngine;

public class MedalDialogClick : MonoBehaviour
{
	public Action cback;

	public void OnClickClose(MButton button)
	{
		button.interactable = false;
		this.DestroySelf();
		if (cback != null)
		{
			cback();
		}
	}
}
