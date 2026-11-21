using System;
using System.Collections.Generic;

public static class DictionaryExtend
{
	public static V Get<K, V>(this Dictionary<K, V> self, K key)
	{
		self.TryGetValue(key, out var value);
		return value;
	}

	public static bool isEmpty<TKey, TValue>(this IDictionary<TKey, TValue> it)
	{
		return it.Count == 0;
	}

	public static bool isNotEmpty<TKey, TValue>(this IDictionary<TKey, TValue> it)
	{
		return it.Count != 0;
	}

	public static V Find<K, V>(this Dictionary<K, V> self, Func<V, bool> condition)
	{
		foreach (KeyValuePair<K, V> item in self)
		{
			if (condition(item.Value))
			{
				return item.Value;
			}
		}
		return default(V);
	}
}
