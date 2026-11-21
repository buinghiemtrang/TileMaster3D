using System.Collections.Generic;
using Newtonsoft.Json;

namespace FQDev.UserLabel
{
	public class UserRule
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("replacements")]
		public Dictionary<string, string> Replacements { get; set; }
	}
}
