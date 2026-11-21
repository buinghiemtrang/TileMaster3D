using Newtonsoft.Json;

public class OrderDeliverResult
{
	[JsonProperty("dillId")]
	public string dillId;

	[JsonProperty("status")]
	public int status;
}
