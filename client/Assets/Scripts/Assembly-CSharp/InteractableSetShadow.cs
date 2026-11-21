using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableSetShadow : InteractableSetBase
{
	private List<Shadow> _shadows = new List<Shadow>();

	private List<Color> _colors_old = new List<Color>();

	public Color color = Color.gray;

	public override void InitSet()
	{
		_shadows.AddRange(GetComponents<Shadow>());
		_colors_old.Clear();
		for (int i = 0; i < _shadows.Count; i++)
		{
			_colors_old.Add(_shadows[i].effectColor);
		}
	}

	public override void Interactable(bool b_interactable)
	{
		for (int i = 0; i < _shadows.Count; i++)
		{
			Color effectColor = (b_interactable ? _colors_old[i] : color);
			_shadows[i].effectColor = effectColor;
		}
	}
}
