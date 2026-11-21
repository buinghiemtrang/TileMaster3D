namespace Util
{
	public static class Object
	{
		public static bool AsBool(object data)
		{
			if (data != null)
			{
				return (bool)data;
			}
			return false;
		}

		public static string AsString(object data)
		{
			if (data != null)
			{
				return data as string;
			}
			return string.Empty;
		}
	}
}
