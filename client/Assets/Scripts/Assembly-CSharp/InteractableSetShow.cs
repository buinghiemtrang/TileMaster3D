using UnityEngine;

public class InteractableSetShow : InteractableSetBase
{
	public GameObject objOn;

	public GameObject objOff;

	public override void InitSet()
	{
	}

	public override void Interactable(bool b_interactable)
	{
		objOn.SetActive(b_interactable);
		objOff.SetActive(!b_interactable);
	}
}
