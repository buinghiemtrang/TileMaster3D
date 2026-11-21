using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableSetOutLine : InteractableSetBase
{
	private List<Outline> _outlines = new List<Outline>();

	private List<Color> _colors_old = new List<Color>();

	public Color color = Color.gray;

	public override void InitSet()
	{
		_outlines.AddRange(GetComponents<Outline>());
		_colors_old.Clear();
		for (int i = 0; i < _outlines.Count; i++)
		{
			_colors_old.Add(_outlines[i].effectColor);
		}
	}

	public override void Interactable(bool b_interactable)
	{
		for (int i = 0; i < _outlines.Count; i++)
		{
			Color effectColor = (b_interactable ? _colors_old[i] : color);
			_outlines[i].effectColor = effectColor;
		}
	}
}
