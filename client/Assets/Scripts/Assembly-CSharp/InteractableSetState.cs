using System.Collections.Generic;
using UnityEngine;

public class InteractableSetState : MonoBehaviour
{
	[SerializeField]
	private List<MButton> btns;

	private void Awake()
	{
		if (btns == null)
		{
			return;
		}
		btns.ForEach(delegate(MButton fe)
		{
			if ((bool)fe)
			{
				fe.eventInteractableState += State;
			}
		});
	}

	private void State(bool isInteractable)
	{
		if (btns == null)
		{
			return;
		}
		btns.ForEach(delegate(MButton fe)
		{
			if ((bool)fe)
			{
				fe.SetInteractable(isInteractable);
			}
		});
	}
}
