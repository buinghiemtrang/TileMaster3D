using System.Collections.Generic;
using Newtonsoft.Json;

namespace FQDev.UserLabel
{
	public class UserLabelConfig
	{
		[JsonProperty("filters")]
		public Dictionary<string, UserFilter> Filters { get; set; }

		[JsonProperty("rules")]
		public Dictionary<string, UserRule> Rules { get; set; }

		[JsonProperty("groups")]
		public List<UserGroup> Groups { get; set; }
	}
}
