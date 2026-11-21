using UnityEngine;

namespace frame8.Logic.Misc.Other.Extensions
{
	public static class VectorExtensions
	{
		public static bool HasComponentsWithin01(this Vector2 v)
		{
			float num = Mathf.Min(v.x, v.y);
			float num2 = ((v.x <= v.y) ? v.y : v.x);
			if (num >= 0f)
			{
				return num2 <= 1f;
			}
			return false;
		}

		public static Vector2 Rotate(this Vector2 v, float degreesCounterClockwise)
		{
			float f = degreesCounterClockwise * ((float)System.Math.PI / 180f);
			float num = Mathf.Sin(f);
			float num2 = Mathf.Cos(f);
			Vector2 result = default(Vector2);
			result.x = v.x * num2 - v.y * num;
			result.y = v.x * num + v.y * num2;
			return result;
		}
	}
}
