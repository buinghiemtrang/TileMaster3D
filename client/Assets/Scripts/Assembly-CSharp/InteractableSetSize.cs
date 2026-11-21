using UnityEngine;

public class InteractableSetSize : InteractableSetImageBase
{
	private Vector2 _size_old;

	public Vector2 size;

	private RectTransform myRectTransform;

	public override void InitSet()
	{
		myRectTransform = GetComponent<RectTransform>();
		_size_old = ((myRectTransform != null) ? myRectTransform.sizeDelta : Vector2.zero);
	}

	public override void Interactable(bool b_interactable)
	{
		if (myRectTransform == null)
		{
			return;
		}
		myRectTransform.sizeDelta = (b_interactable ? _size_old : size);
		if (onLoadCompleted != null)
		{
			onLoadCompleted.Invoke(this);
		}
	}
}
