using UnityEngine;

public struct JVector2
{
	public float x;

	public float y;

	public JVector2(Vector3 v)
	{
		x = v.x;
		y = v.y;
	}
}
