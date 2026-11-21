using System.Collections.Generic;
using GameScene;
using GameScene_Element;
using UnityEngine;
using UnityEngine.UI;

public class TeatherHand : MonoBehaviour
{
	public GameObject hand;

	public MButton btn;

	public Image uiMask;

	public static TeatherHand I;

	public List<Element> guideElements;

	private void Awake()
	{
		I = this;
		btn.onDown.AddListener(_down);
		btn.onUp.AddListener(_up);
		uiMask.enabled = false;
		hand.SetActive(value: false);
	}

	public void AddElements(List<Element> elements)
	{
		if (elements != null && elements.Count != 0)
		{
			guideElements = elements;
			uiMask.enabled = true;
			Show(guideElements[0].Self.position);
		}
	}

	private void _down(MButton btn)
	{
		if (GameMgr.I.GameStatus == GameStatus.Playing)
		{
			guideElements[0].Selected();
		}
	}

	private void _up(MButton btn)
	{
		if (GameMgr.I.GameStatus != GameStatus.Playing)
		{
			return;
		}
		AudioMgr.I.PlaySound("selected");
		hand.SetActive(value: false);
		guideElements[0].CancelSelected();
		PlatformMgr.I.Place(guideElements[0], isUp: false);
		guideElements.RemoveAt(0);
		if (guideElements.Count == 0)
		{
			uiMask.enabled = false;
		}
		else
		{
			Show(guideElements[0].Self.position);
		}
	}

	private void Show(Vector3 pos)
	{
		hand.transform.position = new Vector3(pos.x, pos.y, pos.z - 0.07f);
		hand.SetActive(value: true);
	}

	private void Hide(bool isHide)
	{
		hand.SetActive(!isHide);
	}
}
