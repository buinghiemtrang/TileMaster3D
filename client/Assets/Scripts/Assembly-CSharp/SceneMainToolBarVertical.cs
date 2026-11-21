using System;
using UnityEngine;

public class SceneMainToolBarVertical : MonoBehaviour
{
	[SerializeField]
	private Transform tfLayoutShow;

	[SerializeField]
	private CanvasGroup canvasGroupPullDown;

	[SerializeField]
	private GameObject objCursorUp;

	[SerializeField]
	private Transform tfCurTopBtn;

	public const string EventSwitchTopBtn = "SceneMainToolBarVertical_EventSwitchTopBtn";

	private void Awake()
	{
		DispatcherMgr.On("SceneMainToolBarVertical_EventSwitchTopBtn", new Action<Transform>(SetTopBtn));
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off("SceneMainToolBarVertical_EventSwitchTopBtn", new Action<Transform>(SetTopBtn));
	}

	public void SetTopBtn(Transform tfBtn)
	{
		if (!(tfBtn == tfCurTopBtn))
		{
			tfCurTopBtn.SetParent(canvasGroupPullDown.transform, worldPositionStays: false);
			tfCurTopBtn.SetSiblingIndex(0);
			tfBtn.SetParent(tfLayoutShow, worldPositionStays: false);
			tfBtn.SetSiblingIndex(0);
			tfCurTopBtn = tfBtn;
		}
	}

	public void ClickCursorUp(MButton button)
	{
		canvasGroupPullDown.alpha = 255f;
		canvasGroupPullDown.blocksRaycasts = true;
		objCursorUp.gameObject.SetActive(value: false);
	}

	public void ClickCursorDown(MButton button)
	{
		canvasGroupPullDown.alpha = 0f;
		canvasGroupPullDown.blocksRaycasts = false;
		objCursorUp.gameObject.SetActive(value: true);
	}
}
