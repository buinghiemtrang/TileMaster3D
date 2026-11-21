using System;
using System.Collections.Generic;
using System.Linq;

public static class ListExtend
{
	public static void SortR<T>(this List<T> self)
	{
		int num = self.Count;
		if (num - 1 >= 0)
		{
			do
			{
				int num2 = MMath.Random(num);
				T value = self[num2];
				num--;
				self[num2] = self[num];
				self[num] = value;
			}
			while (num > 0);
		}
	}

	public static T Random<T>(this List<T> self)
	{
		return self[MMath.Random(self.Count)];
	}

	public static T PopRFast<T>(this List<T> self)
	{
		int count = self.Count;
		int index = MMath.Random(count);
		T result = self[index];
		self[index] = self[count - 1];
		self.RemoveAt(count - 1);
		return result;
	}

	public static T PopR<T>(this List<T> self)
	{
		int index = MMath.Random(self.Count);
		T result = self[index];
		self.RemoveAt(index);
		return result;
	}

	public static T Pop<T>(this List<T> self, int index = -1)
	{
		if (index == -1)
		{
			index = self.Count - 1;
		}
		T result = self[index];
		self.RemoveAt(index);
		return result;
	}

	public static T Last<T>(this List<T> self)
	{
		return self[self.Count - 1];
	}

	public static int Weight<T>(this List<T> self, Func<T, int> selector)
	{
		int num = self.Sum(selector);
		int num2 = MMath.Random(0, num);
		int num3 = 0;
		for (int i = 0; i < self.Count; i++)
		{
			num3 += selector(self[i]);
			if (num2 <= num3)
			{
				return i;
			}
		}
		return 0;
	}

	public static void _remove<T>(this List<T> self, List<T> target)
	{
		self.RemoveAll((T x) => target.Contains(x));
	}

	public static bool isEmpty(this Array self)
	{
		if (self != null)
		{
			return self.Length == 0;
		}
		return true;
	}

	public static bool isNotEmpty(this Array self)
	{
		if (self != null)
		{
			return self.Length > 0;
		}
		return false;
	}

	public static string ToLogString<T>(this List<T> self)
	{
		// UNVERIFIED: item separator literal (SL____ = 2 non-alphanumeric chars) cannot be recovered from the pseudo-C; loop = StringBuilder.Append(item?.ToString() + sep)
		return null;
	}
}
