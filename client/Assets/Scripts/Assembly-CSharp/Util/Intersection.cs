using UnityEngine;

namespace Util
{
	public static class Intersection
	{
		public static bool lineLine(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
		{
			float num = b2.x - b1.x;
			float num2 = b2.y - b1.y;
			float num3 = a2.x - a1.x;
			float num4 = a2.y - a1.y;
			float num5 = num3 * num2 - num4 * num;
			if (num5 != 0f)
			{
				float num6 = a1.y - b1.y;
				float num7 = a1.x - b1.x;
				float num8 = (num3 * num6 - num4 * num7) / num5;
				if (num8 <= 1f && num8 >= 0f)
				{
					float num9 = (num6 * num - num7 * num2) / num5;
					if (num9 >= 0f && num9 <= 1f)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool linePolygon(Vector2 a1, Vector2 a2, Vector2[] b)
		{
			for (int i = 0; i < b.Length; i++)
			{
				int num = (i + 1) % b.Length;
				if (lineLine(a1, a2, b[i], b[num]))
				{
					return true;
				}
			}
			return false;
		}

		public static bool pointInPolygon(Vector2 point, Vector2[] polygon)
		{
			int num = polygon.Length - 1;
			bool flag = false;
			for (int i = 0; i < polygon.Length; i++)
			{
				if (point.y < polygon[i].y == polygon[num].y <= point.y && point.x < (polygon[num].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[num].y - polygon[i].y) + polygon[i].x)
				{
					flag = !flag;
				}
				num = i;
			}
			return flag;
		}

		public static bool RectPolygon(Rect a, Vector2[] b)
		{
			Vector2 vector = new Vector2(a.x, a.y);
			Vector2 vector2 = new Vector2(a.x, a.y + a.height);
			Vector2 vector3 = new Vector2(a.x + a.width, a.y + a.height);
			Vector2 vector4 = new Vector2(a.x + a.width, a.y);
			if (linePolygon(vector, vector2, b))
			{
				return true;
			}
			if (linePolygon(vector2, vector3, b))
			{
				return true;
			}
			if (linePolygon(vector3, vector4, b))
			{
				return true;
			}
			if (linePolygon(vector4, vector, b))
			{
				return true;
			}
			Vector2[] polygon = new Vector2[4] { vector, vector2, vector3, vector4 };
			for (int i = 0; i < b.Length; i++)
			{
				if (pointInPolygon(b[i], polygon))
				{
					return true;
				}
			}
			if (pointInPolygon(vector, b))
			{
				return true;
			}
			if (pointInPolygon(vector2, b))
			{
				return true;
			}
			if (pointInPolygon(vector3, b))
			{
				return true;
			}
			return pointInPolygon(vector4, b);
		}
	}
}
