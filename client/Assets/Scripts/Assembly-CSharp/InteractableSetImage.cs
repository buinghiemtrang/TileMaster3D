using UnityEngine;
using UnityEngine.UI;

public class InteractableSetImage : InteractableSetImageBase
{
	private Sprite _sprite_old;

	public Sprite sprite;

	public override void InitSet()
	{
		_image = GetComponent<Image>();
		_sprite_old = ((_image != null) ? _image.sprite : null);
	}

	public override void Interactable(bool b_interactable)
	{
		if (_image == null)
		{
			return;
		}
		_image.sprite = (b_interactable ? _sprite_old : sprite);
		if (onLoadCompleted != null)
		{
			onLoadCompleted.Invoke(this);
		}
	}
}
