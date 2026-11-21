using UnityEngine;

public class UnlockNewItem : MonoBehaviour
{
	private static Vector3 RotateSpeed = new Vector3(0f, 0f, 30f);

	private void Update()
	{
		base.transform.Rotate(RotateSpeed * Time.deltaTime, Space.World);
	}
}
