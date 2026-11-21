public class InteractableSetSHowOrHide : InteractableSetBase
{
	public override void InitSet()
	{
	}

	public override void Interactable(bool b_interactable)
	{
		gameObject.SetActive(b_interactable);
	}
}
