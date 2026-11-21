using UnityEngine;

public static class Utils
{
	private static Vector3 vTemp3 = Vector3.zero;

	private static Vector3Int vTemp3Int = Vector3Int.zero;

	private static Vector2 vTemp2 = Vector2.zero;

	private static Vector2Int vTemp2Int = Vector2Int.zero;

	private static Color color = Color.white;

	public static Vector3 Vec3(float x, float y, float z = 0f)
	{
		vTemp3.x = x;
		vTemp3.y = y;
		vTemp3.z = z;
		return vTemp3;
	}

	public static Vector3 Vec3(float v)
	{
		vTemp3.x = v;
		vTemp3.y = v;
		vTemp3.z = v;
		return vTemp3;
	}

	public static Vector3Int Vec3Int(int x, int y, int z)
	{
		vTemp3Int.x = x;
		vTemp3Int.y = y;
		vTemp3Int.z = z;
		return vTemp3Int;
	}

	public static Vector3Int Vec3Int(int v)
	{
		vTemp3Int.x = v;
		vTemp3Int.y = v;
		vTemp3Int.z = v;
		return vTemp3Int;
	}

	public static Vector2 Vec2(float x, float y)
	{
		vTemp2.x = x;
		vTemp2.y = y;
		return vTemp2;
	}

	public static Vector2 Vec2(float v)
	{
		vTemp2.x = v;
		vTemp2.y = v;
		return vTemp2;
	}

	public static Vector2Int Vec2Int(int x, int y)
	{
		vTemp2Int.x = x;
		vTemp2Int.y = y;
		return vTemp2Int;
	}

	public static Vector2Int Vec2Int(int v)
	{
		vTemp2Int.x = v;
		vTemp2Int.y = v;
		return vTemp2Int;
	}

	public static Color ColorA(float a)
	{
		color.a = a;
		return color;
	}
}
