using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class InteractableSetImageBase : InteractableSetBase
{
	[Serializable]
	public class VoidDelegate : UnityEvent<InteractableSetImageBase>
	{
	}

	protected Image _image;

	[HideInInspector]
	public VoidDelegate onLoadCompleted;

	public Graphic GetGraphic()
	{
		return _image;
	}
}
