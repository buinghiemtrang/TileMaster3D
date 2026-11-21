using UnityEngine;
using UnityEngine.UI;

public class CircleText : BaseMeshEffect
{
	public int radius = 100;

	public float spaceCoff = 1f;

	public override void ModifyMesh(VertexHelper vh)
	{
		if (!IsActive() || radius == 0)
		{
			return;
		}
		UIVertex vertex = default(UIVertex);
		UIVertex vertex2 = default(UIVertex);
		UIVertex vertex3 = default(UIVertex);
		UIVertex vertex4 = default(UIVertex);
		if (vh.currentVertCount <= 3)
		{
			return;
		}
		int num = 0;
		int num2 = 3;
		do
		{
			vh.PopulateUIVertex(ref vertex, num2 - 3);
			vh.PopulateUIVertex(ref vertex2, num2 - 2);
			vh.PopulateUIVertex(ref vertex3, num2 - 1);
			vh.PopulateUIVertex(ref vertex4, num2);
			Vector3 vector = Vector3.Lerp(vertex.position, vertex3.position, 0.5f);
			Matrix4x4 matrix4x = Matrix4x4.TRS(-vector, Quaternion.identity, Vector3.one);
			float num3 = 1.5707964f - spaceCoff * vector.x / (float)radius;
			Quaternion q = Quaternion.Euler(0f, 0f, num3 * 180f / Mathf.PI - 90f);
			Matrix4x4 matrix4x2 = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
			Matrix4x4 matrix4x3 = Matrix4x4.TRS(new Vector3(Mathf.Cos(num3) * (float)radius, Mathf.Sin(num3) * (float)radius, (float)radius * 0f), Quaternion.identity, Vector3.one);
			Matrix4x4 matrix4x4 = matrix4x3 * matrix4x2 * matrix4x;
			Vector3 vector2 = matrix4x4.MultiplyPoint(vertex.position);
			vertex.position = new Vector3(vector2.x, vector.y + (vector2.y - (float)radius), vector2.z);
			vector2 = matrix4x4.MultiplyPoint(vertex2.position);
			vertex2.position = new Vector3(vector2.x, vector.y + (vector2.y - (float)radius), vector2.z);
			vector2 = matrix4x4.MultiplyPoint(vertex3.position);
			vertex3.position = new Vector3(vector2.x, vector.y + (vector2.y - (float)radius), vector2.z);
			vector2 = matrix4x4.MultiplyPoint(vertex4.position);
			vertex4.position = new Vector3(vector2.x, vector.y + (vector2.y - (float)radius), vector2.z);
			vh.SetUIVertex(vertex, num2 - 3);
			vh.SetUIVertex(vertex2, num2 - 2);
			vh.SetUIVertex(vertex3, num2 - 1);
			vh.SetUIVertex(vertex4, num2);
			num++;
			num2 += 4;
		}
		while (num < vh.currentVertCount / 4);
	}
}
