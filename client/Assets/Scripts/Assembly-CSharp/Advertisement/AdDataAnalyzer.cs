using Newtonsoft.Json;

namespace Advertisement
{
	public class AdDataAnalyzer
	{
		public static T DeserializeJson<T>(string json) where T : IAdData, new()
		{
			return JsonConvert.DeserializeObject<T>(json);
		}
	}
}
