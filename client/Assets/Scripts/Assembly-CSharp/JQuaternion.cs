using UnityEngine;

public struct JQuaternion
{
	public float x;

	public float y;

	public float z;

	public float w;

	public JQuaternion(Quaternion v)
	{
		x = v.x;
		y = v.y;
		z = v.z;
		w = v.w;
	}
}
