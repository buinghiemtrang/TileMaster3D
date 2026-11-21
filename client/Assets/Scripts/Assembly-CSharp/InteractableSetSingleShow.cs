using UnityEngine;

public class InteractableSetSingleShow : InteractableSetBase
{
	public GameObject obj;

	public override void InitSet()
	{
	}

	public override void Interactable(bool b_interactable)
	{
		obj.SetActive(b_interactable);
	}
}
