using System.Collections.Generic;
using Newtonsoft.Json;

namespace FQDev.UserLabel
{
	public class UserGroup
	{
		[JsonProperty("required")]
		public List<string> FiltersRequired { get; set; }

		[JsonProperty("excluded")]
		public List<string> FiltersExcluded { get; set; }

		[JsonProperty("priority")]
		public int Priority { get; set; }

		[JsonProperty("rule")]
		public string RuleName { get; set; }
	}
}
