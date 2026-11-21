using UnityEngine;
using UnityEngine.UI;

public class ChangeOutlineOrShadowColor : MonoBehaviour
{
	[SerializeField]
	private Color[] color;

	[SerializeField]
	private Outline outline;

	[SerializeField]
	private Shadow shadow;

	public void ChangeColor(int index)
	{
		if (index < color.Length)
		{
			if (outline != null)
			{
				outline.effectColor = color[index];
			}
			if (shadow != null)
			{
				shadow.effectColor = color[index];
			}
		}
	}
}
