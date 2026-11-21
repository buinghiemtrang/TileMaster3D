using System.Collections.Generic;
using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;

namespace _FunctionsBlock.GameNotices
{
	public class NoticeHttpMgr : HttpMgrBase
	{
		private static NoticeHttpMgr _instance;

		public List<NoticeInfo> ayGameLED;

		public NoticeInfo pNoticeInfo;

		public const string GET_NOTICES = "Http_GetNotices";

		private const string TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiVGlsZSBNYXN0ZXIgM0QiLCJpZCI6NDIsInNob3dfaWQiOiJ0aWxlbWFzdGVyM2QiLCJleHAiOjE5NTc5NDUyNjQsImlzcyI6ImZ1cWlhbmdhbWVhdXRoIiwibmJmIjoxNjQ2OTA0MjY0fQ.EfSsj963Z0PhMfIWpSpxYRz_pqhTXlBbRYf-b4stY_I";

		public static NoticeHttpMgr I
		{
			get
			{
				if (!_instance)
				{
					GameObject gameObject = new GameObject("NoticeHttpMgr");
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
					_instance = gameObject.AddComponent<NoticeHttpMgr>();
				}
				return _instance;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			ayGameLED = new List<NoticeInfo>();
			pNoticeInfo = null;
		}

		public void RequestNotice()
		{
			if (ServerDateTimeMgr.I.IsServerTimeSynced)
			{
				headers.Clear();
				headers.Add("Authorization", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiVGlsZSBNYXN0ZXIgM0QiLCJpZCI6NDIsInNob3dfaWQiOiJ0aWxlbWFzdGVyM2QiLCJleHAiOjE5NTc5NDUyNjQsImlzcyI6ImZ1cWlhbmdhbWVhdXRoIiwibmJmIjoxNjQ2OTA0MjY0fQ.EfSsj963Z0PhMfIWpSpxYRz_pqhTXlBbRYf-b4stY_I");
				headers.Add("Content-Type", "application/json");
				long fromTime = ServerDateTimeMgr.I.NetTime.TotalSeconds();
				string body = JsonConvert.SerializeObject(new
				{
					fromTime = fromTime,
					system = "android",
					ver = Application.version
				});
				OwnHttpMgr.I.SendPost("https://service.fuqiangame.com/notice/api/v1/app/appGetNotice", headers, body, delegate(HTTPResponse resp)
				{
					SetNotices(SendListInfo<NoticeInfo>("Http_GetNotices", resp));
				}, "Http_GetNotices");
			}
		}

		private void SetNotices(ServerDataListRoot<NoticeInfo> root)
		{
			if (root.code != 0 || root.data == null || root.data.Count == 0)
			{
				return;
			}
			List<NoticeInfo> list = root.data.FindAll((NoticeInfo info) => info.category == "Login" && (pNoticeInfo == null || pNoticeInfo.id < info.id));
			if (list.Count > 1)
			{
				list.Sort((NoticeInfo a, NoticeInfo b) => b.id - a.id);
			}
			if (list.Count > 0)
			{
				pNoticeInfo = list[0];
			}
			List<NoticeInfo> list2 = root.data.FindAll((NoticeInfo info) => info.category == "GameLED" && !ayGameLED.Exists((NoticeInfo led) => led.id == info.id));
			if (list2.Count > 0)
			{
				ayGameLED.AddRange(list2);
				for (int num = ayGameLED.Count - 1; num >= 0; num--)
				{
					if (ayGameLED[num].isUsed)
					{
						ayGameLED.RemoveAt(num);
					}
				}
				if (ayGameLED.Count >= 2)
				{
					ayGameLED.Sort((NoticeInfo a, NoticeInfo b) => b.id - a.id);
				}
			}
		}
	}
}
