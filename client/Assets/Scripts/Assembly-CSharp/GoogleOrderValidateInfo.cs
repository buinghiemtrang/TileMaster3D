using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class GoogleOrderValidateInfo
{
	public int acknowledgementState;

	public int consumptionState;

	public string developerPayload;

	public string kind;

	public string obfuscatedExternalAccountId;

	public string obfuscatedExternalProfileId;

	public string orderId;

	public string productId;

	[JsonProperty]
	public int purchaseState;

	public string purchaseTimeMillis;

	public string purchaseToken;

	[JsonProperty]
	public object purchaseType;

	public int quantity;

	public string regionCode;
}
