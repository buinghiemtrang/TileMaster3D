using System;
using UnityEngine;

public class InteractableSwitch : MonoBehaviour
{
	private event Action<bool> eventInteractable;

	private void Awake()
	{
		InteractableSetBase[] componentsInChildren = GetComponentsInChildren<InteractableSetBase>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			eventInteractable += componentsInChildren[i].Interactable;
		}
	}

	public void Switch(bool b_sure)
	{
		eventInteractable?.Invoke(b_sure);
	}
}
