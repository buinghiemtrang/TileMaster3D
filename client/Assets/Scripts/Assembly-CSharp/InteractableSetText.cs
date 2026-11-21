using UnityEngine;
using UnityEngine.UI;

public class InteractableSetText : InteractableSetBase
{
	private Text _text;

	private Color _text_color_old;

	public Color color;

	public InteractableSetText()
	{
		color = Color.gray;
	}

	public override void InitSet()
	{
		_text = GetComponent<Text>();
		_text_color_old = _text.color;
	}

	public override void Interactable(bool b_interactable)
	{
		_text.color = b_interactable ? _text_color_old : color;
	}
}
