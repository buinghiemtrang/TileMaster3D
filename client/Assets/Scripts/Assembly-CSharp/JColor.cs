using UnityEngine;

public struct JColor
{
	public float r;

	public float g;

	public float b;

	public float a;

	public JColor(Color color)
	{
		r = color.r;
		g = color.g;
		b = color.b;
		a = color.a;
	}
}
