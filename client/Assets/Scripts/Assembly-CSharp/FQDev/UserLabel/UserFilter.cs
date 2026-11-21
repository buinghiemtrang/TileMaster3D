using System.Collections.Generic;
using Newtonsoft.Json;

namespace FQDev.UserLabel
{
	public class UserFilter
	{
		[JsonProperty("equation")]
		public EquationType Equation;

		[JsonProperty("emit")]
		public bool Emit;

		[JsonProperty("param")]
		public string Param;

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("field")]
		public string Field { get; set; }

		[JsonProperty("values")]
		public List<string> Values { get; set; }
	}
}
