using System.Collections.Generic;

namespace FQDev.WordBlocker
{
	public static class TextFilterCenter
	{
		public static TextFilter Default = new TextFilter();

		private static Dictionary<string, TextFilter> _filters = new Dictionary<string, TextFilter>();

		public static TextFilter GetFilter(string key)
		{
			if (!_filters.TryGetValue(key, out var value))
			{
				value = new TextFilter();
				_filters.Add(key, value);
			}
			return value;
		}
	}
}
