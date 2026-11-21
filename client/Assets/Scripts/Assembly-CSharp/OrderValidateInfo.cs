using Newtonsoft.Json;

public class OrderValidateInfo
{
	[JsonProperty("dillId")]
	public string dillId;

	[JsonProperty("status")]
	public int status;

	[JsonProperty("isSandbox")]
	public bool isSandbox;
}
