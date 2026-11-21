using System;
using System.Collections.Generic;
using UnityEngine;

public static class MMath
{
	private static System.Random mRandom = new System.Random();

	public static int Random(int min, int max)
	{
		return mRandom.Next(min, max);
	}

	public static int Random(int max)
	{
		return mRandom.Next(max);
	}

	public static double Random()
	{
		return mRandom.NextDouble();
	}

	public static int RandomWeight(List<int> v_weight)
	{
		int num = Random(0, System.Linq.Enumerable.Sum(v_weight) + 1);
		for (int i = 0; i < v_weight.Count; i++)
		{
			if (num < v_weight[i])
			{
				return i;
			}
			num -= v_weight[i];
		}
		return 0;
	}

	public static int RandomWeight(int[] v_weight, int n_start = -1, int n_end = -1)
	{
		int num = ((n_start != -1) ? n_start : 0);
		if (n_end == -1)
		{
			n_end = v_weight.Length;
		}
		int num2 = 1;
		if (num < n_end)
		{
			num2 = 0;
			for (int i = num; i < n_end; i++)
			{
				num2 += v_weight[i];
			}
			num2++;
		}
		int num3 = Random(0, num2);
		for (int j = num; j < n_end; j++)
		{
			if (num3 < v_weight[j])
			{
				return j;
			}
			num3 -= v_weight[j];
		}
		return 0;
	}

	public static int ConditionIndex(List<int> v_condition, int value)
	{
		for (int i = 0; i < v_condition.Count; i++)
		{
			if (value <= v_condition[i])
			{
				return i;
			}
		}
		return v_condition.Count - 1;
	}

	public static void Swap<T>(ref T x, ref T y)
	{
		T val = x;
		x = y;
		y = val;
	}

	public static Vector2 Lerp(Vector2[] _Vertices, float _Phase)
	{
		if (_Vertices == null || _Vertices.Length == 0)
		{
			return Vector2.zero;
		}
		float num = _Vertices.Length;
		int num2 = Mathf.FloorToInt(num * _Phase);
		int num3 = (num2 + 1) % _Vertices.Length;
		float t = (_Phase - (float)num2 / num) / ((float)(num2 + 1) / num - (float)num2 / num);
		return Vector2.Lerp(_Vertices[num2], _Vertices[num3], t);
	}

	public static float Remap(float _Value, float _SourceMin, float _SourceMax, float _TargetMin, float _TargetMax)
	{
		return (_Value - _SourceMin) / (_SourceMax - _SourceMin) * (_TargetMax - _TargetMin) + _TargetMin;
	}

	public static float Remap01(float _Value, float _SourceMin, float _SourceMax)
	{
		return (_Value - _SourceMin) / (_SourceMax - _SourceMin);
	}

	public static void CircleAnglePosition(ref Vector2 p, Vector2 c, float angle)
	{
		float f = angle * ((float)Math.PI / 180f);
		float num = Mathf.Sin(f);
		float num2 = Mathf.Cos(f);
		float num3 = p.x - c.x;
		float num4 = p.y - c.y;
		p.x = c.x + (num3 * num2 - num4 * num);
		p.y = c.y + num4 * num2 + num3 * num;
	}
}
