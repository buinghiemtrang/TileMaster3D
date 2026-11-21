using UnityEngine;

public struct JVector3
{
	public float x;

	public float y;

	public float z;

	public JVector3(Vector3 v)
	{
		x = v.x;
		y = v.y;
		z = v.z;
	}
}
