using System.Collections.Generic;
using UnityEngine;

namespace DG.Tweening
{
	public static class DOTweenModuleBezier
	{
		private static readonly Queue<Vector3> Vertexes = new Queue<Vector3>();

		public static Vector3[] Creator(Vector3 p0, Vector3 p1, Vector3 p2, int vertexCount = 10)
		{
			Vertexes.Clear();
			Vertexes.Enqueue(p0);
			if (vertexCount > 1)
			{
				for (int i = 1; i < vertexCount; i++)
				{
					float num = 1f / (float)vertexCount * (float)i;
					float num2 = 1f - num;
					float num3 = num2 * num2;
					float num4 = num * (num2 + num2);
					float num5 = num * num;
					Vertexes.Enqueue(new Vector3(p2.x * num5 + p0.x * num3 + p1.x * num4, p2.y * num5 + p0.y * num3 + p1.y * num4, p2.z * num5 + p0.z * num3 + p1.z * num4));
				}
			}
			Vertexes.Enqueue(p2);
			return Vertexes.ToArray();
		}

		public static Vector3[] CreatorBy(Vector3 p0, Vector3 p1, Vector3 p2, int vertexCount = 10)
		{
			Vertexes.Clear();
			Vertexes.Enqueue(p0);
			if (vertexCount > 1)
			{
				for (int i = 1; i < vertexCount; i++)
				{
					float num = 1f / (float)vertexCount * (float)i;
					float num2 = 1f - num;
					float num3 = num2 * num2;
					float num4 = num * (num2 + num2);
					float num5 = num * num;
					Vertexes.Enqueue(new Vector3(p2.x * num5 + p0.x * num3 + (p0.x + p1.x) * num4, p2.y * num5 + p0.y * num3 + (p0.y + p1.y) * num4, p2.z * num5 + p0.z * num3 + (p0.z + p1.z) * num4));
				}
			}
			Vertexes.Enqueue(p2);
			return Vertexes.ToArray();
		}

		public static Vector3[] Creator(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int vertexCount = 10)
		{
			Vertexes.Clear();
			if (vertexCount >= 0)
			{
				for (int i = 0; i <= vertexCount; i++)
				{
					Vertexes.Enqueue(GetPoint(1f / (float)vertexCount * (float)i, p0, p1, p2, p3));
				}
			}
			return Vertexes.ToArray();
		}

		public static Vector3[] CreatorBy(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int vertexCount = 10)
		{
			Vertexes.Clear();
			if (vertexCount >= 0)
			{
				for (int i = 0; i <= vertexCount; i++)
				{
					Vertexes.Enqueue(GetPoint(1f / (float)vertexCount * (float)i, p0, p0 + p1, p0 + p2, p3));
				}
			}
			return Vertexes.ToArray();
		}

		public static Vector3 GetPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
		{
			float num = 1f - t;
			float num2 = num * num;
			float num3 = (num + num) * t;
			float num4 = t * t;
			return new Vector3(p0.x * num2 + p1.x * num3 + num4 * p2.x, p0.y * num2 + p1.y * num3 + num4 * p2.y, p0.z * num2 + p1.z * num3 + num4 * p2.z);
		}

		public static Vector3 GetPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			float num = 1f - t;
			float num2 = num * num * num;
			float num3 = num * num * 3f * t;
			float num4 = num * t * t * 3f;
			float num5 = t * t * t;
			return new Vector3(num5 * p3.x + num4 * p2.x + p0.x * num2 + p1.x * num3, num5 * p3.y + num4 * p2.y + p0.y * num2 + p1.y * num3, num5 * p3.z + num4 * p2.z + p0.z * num2 + p1.z * num3);
		}

		public static Tweener DOLocalBezier(this Transform self, float delay, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			return DOTween.To((float v) => self.localPosition = GetPoint(v, p1, p2, p3), 0f, 1f, delay);
		}

		public static Tweener DOLocalBezier(this Transform self, float delay, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
		{
			return DOTween.To((float v) => self.localPosition = GetPoint(v, p1, p2, p3, p4), 0f, 1f, delay);
		}

		public static Tweener DOLocalBezier(this Transform self, float delay, Vector3[] ay_pos)
		{
			if (ay_pos.Length == 4)
			{
				return DOTween.To((float v) => self.localPosition = GetPoint(v, ay_pos[0], ay_pos[1], ay_pos[2], ay_pos[3]), 0f, 1f, delay);
			}
			return DOTween.To((float v) => self.localPosition = GetPoint(v, ay_pos[0], ay_pos[1], ay_pos[2]), 0f, 1f, delay);
		}

		public static Tweener DOBezier(this Transform self, float delay, Vector3 p1, Vector3 p2, Vector3 p3)
		{
			return DOTween.To((float v) => self.position = GetPoint(v, p1, p2, p3), 0f, 1f, delay);
		}

		public static Tweener DOBezier(this Transform self, float delay, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
		{
			return DOTween.To((float v) => self.position = GetPoint(v, p1, p2, p3, p4), 0f, 1f, delay);
		}

		public static Tweener DOBezier(this Transform self, float delay, Vector3[] ay_pos)
		{
			if (ay_pos.Length == 4)
			{
				return DOTween.To((float v) => self.position = GetPoint(v, ay_pos[0], ay_pos[1], ay_pos[2], ay_pos[3]), 0f, 1f, delay);
			}
			return DOTween.To((float v) => self.position = GetPoint(v, ay_pos[0], ay_pos[1], ay_pos[2]), 0f, 1f, delay);
		}
	}
}
