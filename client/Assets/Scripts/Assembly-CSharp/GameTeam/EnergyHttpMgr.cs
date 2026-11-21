using BestHTTP;
using Newtonsoft.Json;

namespace GameTeam
{
	public class EnergyHttpMgr : ShareHttpMgr
	{
		public static EnergyHttpMgr I;

		public const string RECEIVEENERGYKEY = "TeamHttp_ReceiveEnergy";

		protected override void Awake()
		{
			base.Awake();
			I = this;
			OwnHttpMgr.I.Timeout = 5.0;
			OwnHttpMgr.I.ConnectTimeout = 2.0;
		}

		public void ReceiveEnergy(string id)
		{
			headers.Clear();
			headers.Add("Authorization", "Bearer " + DataManager.I.Token);
			headers.Add("Content-Type", "application/json");
			string body = JsonConvert.SerializeObject(new
			{
				GUID = id
			});
			OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/team/ReceiveLive", headers, body, delegate(HTTPResponse resp)
			{
				SendInfo("TeamHttp_ReceiveEnergy", resp);
			}, "TeamHttp_ReceiveEnergy");
		}
	}
}
