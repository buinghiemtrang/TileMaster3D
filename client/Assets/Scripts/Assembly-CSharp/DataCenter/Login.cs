using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Login : ILogin
	{
		public Login()
		{
			InitData();
		}
	}
}
