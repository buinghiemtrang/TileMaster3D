using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ValueOnceTrack : IValueOnceTrack
	{
	}
}
