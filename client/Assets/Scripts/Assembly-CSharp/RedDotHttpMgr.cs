using BestHTTP;
using DG.Tweening;
using Newtonsoft.Json;

public class RedDotHttpMgr : HttpMgrBase
{
	private class ServerInfoData
	{
		public int code;

		public string msg;

		public bool? data;
	}

	public static RedDotHttpMgr I;

	private Sequence delaySeq;

	private HTTPRequest checkRedRequest;

	private bool bIsStopRefresh;

	private string urlCheckNewTeamBoard = "https://service.fuqiangame.com/team/CheckNewTeamBoard";

	private const string CheckNewTeamBoardKey = "CHECK_NEW_TEAMBOARD";

	protected override void Awake()
	{
		base.Awake();
		I = this;
	}

	private void Start()
	{
		CheckNewTeamBoard();
		delaySeq = DOTween.Sequence().AppendInterval(10f).AppendCallback(delegate
		{
			CheckNewTeamBoard();
		})
			.SetLoops(-1)
			.SetId("CheckNewTeamBoard_Delay");
	}

	private void OnDestroy()
	{
		if (delaySeq != null)
		{
			delaySeq.Kill();
			delaySeq = null;
		}
	}

	public void CheckChangeState(int tag)
	{
		if (tag == 4)
		{
			if (delaySeq != null)
			{
				delaySeq.Pause();
			}
			bIsStopRefresh = true;
		}
		else if (bIsStopRefresh)
		{
			if (delaySeq != null)
			{
				delaySeq.Play();
			}
			bIsStopRefresh = false;
		}
	}

	private void CheckNewTeamBoard()
	{
		headers.Clear();
		headers.Add("Authorization", "Bearer " + DataManager.I.Token);
		checkRedRequest = OwnHttpMgr.I.SendGet(urlCheckNewTeamBoard, headers, delegate(HTTPResponse resp)
		{
			checkRedRequest = null;
			if (resp != null)
			{
				ServerInfoData serverInfoData = JsonConvert.DeserializeObject<ServerInfoData>(resp.DataAsText);
				if (serverInfoData.code == 1)
				{
					DispatcherMgr.Emit("CHECK_NEW_TEAMBOARD", serverInfoData.data);
				}
			}
		}, "CHECK_NEW_TEAMBOARD");
	}
}
