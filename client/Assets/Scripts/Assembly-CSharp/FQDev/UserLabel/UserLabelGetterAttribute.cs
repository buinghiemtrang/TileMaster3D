using System;
using System.Runtime.InteropServices;

namespace FQDev.UserLabel
{
	[AttributeUsage(AttributeTargets.Method)]
	[ComVisible(true)]
	public sealed class UserLabelGetterAttribute : Attribute
	{
		public string Field { get; }

		public UserLabelGetterAttribute(string field)
		{
			Field = field;
		}
	}
}
