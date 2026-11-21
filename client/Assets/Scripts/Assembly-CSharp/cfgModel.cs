using UnityEngine;

public class cfgModel
{
	public int id;

	public int type;

	public string prefab;

	public float pointScale = 0.8f;

	public Vector3 pointPos = new Vector3(-2.55f, 3f, -4.5f);

	public Vector3 pointAngle = Vector3.zero;

	public float unlockScale = 1f;

	public Vector3 unlockAngle = Vector3.zero;

	public string sFile => "Sketch/" + prefab;
}
