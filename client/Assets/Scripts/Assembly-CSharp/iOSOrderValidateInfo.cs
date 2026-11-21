using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class iOSOrderValidateInfo
{
	[JsonProperty]
	public int status;

	[JsonProperty]
	public string environment;

	public List<object> receipt;
}
