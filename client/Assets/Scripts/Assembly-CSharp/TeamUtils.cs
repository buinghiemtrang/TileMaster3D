using System;
using UnityEngine;
using UnityEngine.UI;

public static class TeamUtils
{
	public static void ShowFlayItemAnim(Transform originTransform, Transform destTransform, PropID propID, Action cback)
	{
		Vector3 sPos = SceneMgr.tfTopRoot.InverseTransformPoint(originTransform.localPosition);
		Vector2 ePos = SceneMgr.tfTopRoot.InverseTransformPoint(destTransform.position);
		FlyItem item = LoadMgr.LoadRes<FlyItem>("Effect/Prop");
		item.transform.GetComponent<Image>().sprite = LoadMgr.LoadRes<Sprite>(string.Format("Props/prop{0}", (int)propID));
		item.Fly2D(SceneMgr.tfTopRoot, sPos, ePos, delegate
		{
			if (cback != null)
			{
				cback();
			}
			UnityEngine.Object.Destroy(item.gameObject);
		});
	}
}
