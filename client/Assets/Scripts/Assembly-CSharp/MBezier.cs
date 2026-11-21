using System.Collections.Generic;
using UnityEngine;

public static class MBezier
{
	private static Queue<Vector3> vertexs = new Queue<Vector3>();

	public static Vector3[] Creator(Vector3 p0, Vector3 p1, Vector3 p2, int vertexCount = 10)
	{
		vertexs.Clear();
		vertexs.Enqueue(p0);
		for (int i = 1; i < vertexCount; i++)
		{
			float t = 1f / (float)vertexCount * (float)i;
			float u = 1f - t;
			vertexs.Enqueue(p0 * (u * u) + p1 * (t * (u + u)) + p2 * (t * t));
		}
		vertexs.Enqueue(p2);
		return vertexs.ToArray();
	}

	public static Vector3[] CreatorBy(Vector3 p0, Vector3 p1, Vector3 p2, int vertexCount = 10)
	{
		vertexs.Clear();
		vertexs.Enqueue(p0);
		for (int i = 1; i < vertexCount; i++)
		{
			float t = 1f / (float)vertexCount * (float)i;
			float u = 1f - t;
			vertexs.Enqueue(p0 * (u * u) + (p0 + p1) * (t * (u + u)) + p2 * (t * t));
		}
		vertexs.Enqueue(p2);
		return vertexs.ToArray();
	}

	public static Vector3[] Creator(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int vertexCount = 10)
	{
		vertexs.Clear();
		for (int i = 0; i <= vertexCount; i++)
		{
			vertexs.Enqueue(GetPoint(1f / (float)vertexCount * (float)i, p0, p1, p2, p3));
		}
		return vertexs.ToArray();
	}

	public static Vector3[] CreatorBy(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int vertexCount = 10)
	{
		vertexs.Clear();
		for (int i = 0; i <= vertexCount; i++)
		{
			vertexs.Enqueue(GetPoint(1f / (float)vertexCount * (float)i, p0, p0 + p1, p0 + p2, p3));
		}
		return vertexs.ToArray();
	}

	public static Vector3 GetPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		float u = 1f - t;
		return p0 * (u * u) + p1 * ((u + u) * t) + p2 * (t * t);
	}

	public static Vector3 GetPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float u = 1f - t;
		return p0 * (u * u * u) + p1 * (u * u * 3f * t) + p2 * (u * t * t * 3f) + p3 * (t * t * t);
	}
}
