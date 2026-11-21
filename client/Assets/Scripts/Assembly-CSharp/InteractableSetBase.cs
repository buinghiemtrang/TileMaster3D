using UnityEngine;

public abstract class InteractableSetBase : MonoBehaviour
{
	public abstract void Interactable(bool b_interactable);

	public abstract void InitSet();

	protected virtual void Awake()
	{
		InitSet();
	}

	protected virtual void Start()
	{
	}
}
