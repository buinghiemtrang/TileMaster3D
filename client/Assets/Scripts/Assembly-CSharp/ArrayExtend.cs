using System;
using System.Text;

public static class ArrayExtend
{
	public static bool IsNull(this Array self)
	{
		if (self != null)
		{
			return self.Length == 0;
		}
		return true;
	}

	public static bool IsNotNull(this Array self)
	{
		if (self != null)
		{
			return self.Length > 0;
		}
		return false;
	}

	public static void Shuffle<T>(this T[] self)
	{
		for (int num = self.Length - 1; num >= 0; num--)
		{
			int num2 = UnityEngine.Random.Range(0, num + 1);
			T val = self[num2];
			self[num2] = self[num];
			self[num] = val;
		}
	}

	public static T Random<T>(this T[] self)
	{
		return self[UnityEngine.Random.Range(0, self.Length)];
	}

	public static T First<T>(this T[] self)
	{
		if (self.Length == 0)
		{
			return default(T);
		}
		return self[0];
	}

	public static T Last<T>(this T[] self)
	{
		if (self.Length == 0)
		{
			return default(T);
		}
		return self[self.Length - 1];
	}

	public static bool TryGet<T>(this T[] self, int index, out T value)
	{
		value = default(T);
		if (index < 0 || index >= self.Length)
		{
			return false;
		}
		value = self[index];
		return true;
	}

	public static T Get<T>(this T[] self, int index, T defaultValue = default(T))
	{
		if (index >= 0 && index < self.Length)
		{
			return self[index];
		}
		return defaultValue;
	}

	public static int Weight(this int[] self)
	{
		int total = 0;
		self.ForEach(delegate(int v)
		{
			total += v;
		});
		int num = UnityEngine.Random.Range(0, total);
		int num2 = 0;
		for (int i = 0; i < self.Length; i++)
		{
			num2 += self[i];
			if (num <= num2)
			{
				return i;
			}
		}
		return 0;
	}

	public static string ToStr(this int[,] self, char one = ',', char two = '|')
	{
		StringBuilder stringBuilder = new StringBuilder();
		int length = self.GetLength(0);
		int length2 = self.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			if (i != 0)
			{
				stringBuilder.Append('|');
			}
			for (int j = 0; j < length2; j++)
			{
				if (j != 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(self[i, j]);
			}
		}
		return stringBuilder.ToString();
	}

	public static void ForEachReverse<T>(this T[] self, Action<T> action)
	{
		for (int num = self.Length - 1; num >= 0; num--)
		{
			action(self[num]);
		}
	}

	public static void ForEachReverse<T>(this T[] self, Action<int, T> action)
	{
		for (int num = self.Length - 1; num >= 0; num--)
		{
			action(num, self[num]);
		}
	}

	public static void ForEach<T>(this T[] self, Action<int, T> action)
	{
		for (int i = 0; i < self.Length; i++)
		{
			action(i, self[i]);
		}
	}

	public static void ForEach<T>(this T[] self, Action<T> action)
	{
		for (int i = 0; i < self.Length; i++)
		{
			action(self[i]);
		}
	}

	public static void ForEach<T>(this T[] self, Func<bool> condition, Action<T> action)
	{
		for (int i = 0; i < self.Length; i++)
		{
			if (condition())
			{
				action(self[i]);
			}
		}
	}

	public static int Counts<T>(this T[] self, Func<T, bool> condition)
	{
		int num = 0;
		for (int i = 0; i < self.Length; i++)
		{
			if (condition(self[i]))
			{
				num++;
			}
		}
		return num;
	}
}
