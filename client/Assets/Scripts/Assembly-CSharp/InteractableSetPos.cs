using UnityEngine;

public class InteractableSetPos : InteractableSetBase
{
	public Vector2 pos;

	private Vector2 oldPos;

	public override void InitSet()
	{
		RectTransform rt = transform as RectTransform;
		oldPos = rt.anchoredPosition;
	}

	public override void Interactable(bool b_interactable)
	{
		RectTransform rt = transform as RectTransform;
		rt.anchoredPosition = b_interactable ? oldPos : pos;
	}
}
