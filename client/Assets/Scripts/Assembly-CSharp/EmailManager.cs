using System;
using System.Collections.Generic;
using BestHTTP;
using GameMail;
using Language;
using Newtonsoft.Json;
using OwnHttp;
using UnityEngine;

public class EmailManager : SingletonMono<EmailManager>
{
	private const string CMD_GET = "https://service.fuqiangame.com/mail/api/v1/app/appFindEmail";

	private const string CMD_READ = "https://service.fuqiangame.com/mail/api/v1/app/appGetEmail";

	private const string CMD_CLAIM = "https://service.fuqiangame.com/mail/api/v1/app/appClaimEmail";

	private DateTime LastEmailListReqTime;

	private int AutoReqEmailListIntervalTime;

	public List<MailData> CacheEmails;

	private float time;

	protected override void Init()
	{
		DispatcherMgr.On("Update_Account", OnAccountChanged);
		DispatcherMgr.On("AccountDataSynced", OnAccountChanged);
	}

	private void OnAccountChanged()
	{
		GetEmails();
	}

	public void GetEmails()
	{
		if (!IsLogin())
		{
			return;
		}
		OwnSingleHttp ownSingleHttp = RequestsMgr.I.Create(HTTPMethods.Post, "https://service.fuqiangame.com/mail/api/v1/app/appFindEmail", delegate(OwnHttp.ResultInfo resultInfo)
		{
			if (resultInfo.State == OwnHttp.RequestState.Success)
			{
				Debug.Log("https://service.fuqiangame.com/mail/api/v1/app/appFindEmail: " + resultInfo.Response.DataAsText);
				EmailServerDataRoot emailServerDataRoot = JsonConvert.DeserializeObject<EmailServerDataRoot>(resultInfo.Response.DataAsText);
				if (emailServerDataRoot.code == 1 && emailServerDataRoot.data != null)
				{
					CacheEmails = ParseServerEmailList(emailServerDataRoot.data.list);
					DispatcherMgr.Emit("GameMail_Get_Success");
				}
				else
				{
					CacheEmails.Clear();
				}
			}
			else
			{
				CacheEmails.Clear();
			}
		});
		Dictionary<string, string> headers = ServerCommonSign.GetCommonHeaders(true);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("pullReceived", 1);
		dictionary.Add("pullClaimed", 0);
		dictionary.Add("uid", DB.Account.Userid);
		dictionary.Add("lang", GetCurrentI18N());
		ServerCommonSign.Sign(ref headers, dictionary);
		ownSingleHttp.AddHeader(headers);
		ownSingleHttp.AddParameter(JsonConvert.SerializeObject(dictionary));
		ownSingleHttp.AddCustomTimeout(50f);
		ownSingleHttp.Send();
		LastEmailListReqTime = DateTime.Now;
	}

	private bool IsLogin()
	{
		return !string.IsNullOrEmpty(DB.Account.Token);
	}

	public void EmailTagRead(MailData info)
	{
		if (!IsLogin() || info.Status != 0)
		{
			return;
		}
		info.Status = 1;
		OwnSingleHttp ownSingleHttp = RequestsMgr.I.Create(HTTPMethods.Post, "https://service.fuqiangame.com/mail/api/v1/app/appGetEmail", delegate(OwnHttp.ResultInfo resultInfo)
		{
			if (resultInfo.State == OwnHttp.RequestState.Success && resultInfo.Response != null)
			{
				Debug.Log(resultInfo.Response.DataAsText);
				LogEvent.I.Mail_Click(info.Id);
				GetEmails();
			}
		});
		Dictionary<string, string> headers = ServerCommonSign.GetCommonHeaders(true);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("lang", GetCurrentI18N());
		dictionary.Add("emailId", info.Id);
		dictionary.Add("uid", DB.Account.Userid);
		ServerCommonSign.Sign(ref headers, dictionary);
		ownSingleHttp.AddHeader(headers);
		ownSingleHttp.AddParameter(JsonConvert.SerializeObject(dictionary));
		ownSingleHttp.AddCustomTimeout(50f);
		ownSingleHttp.Send();
	}

	public void EmailClaimReward(MailData info, Action<bool> callback)
	{
		if (!IsLogin() || string.IsNullOrEmpty(info.Rewards))
		{
			return;
		}
		OwnSingleHttp ownSingleHttp = RequestsMgr.I.Create(HTTPMethods.Post, "https://service.fuqiangame.com/mail/api/v1/app/appClaimEmail", delegate(OwnHttp.ResultInfo resultInfo)
		{
			if (resultInfo.State != OwnHttp.RequestState.Success)
			{
				callback?.Invoke(false);
				return;
			}
			Debug.Log("https://service.fuqiangame.com/mail/api/v1/app/appClaimEmail: " + resultInfo.Response.DataAsText);
			ServerDataRootBase serverDataRootBase = JsonConvert.DeserializeObject<ServerDataRootBase>(resultInfo.Response.DataAsText);
			if (serverDataRootBase == null || serverDataRootBase.code != 1)
			{
				callback?.Invoke(false);
				return;
			}
			callback?.Invoke(true);
			LogEvent.I.Mail_Receive(info.Id, info.Rewards);
			GetEmails();
		});
		Dictionary<string, string> headers = ServerCommonSign.GetCommonHeaders(true);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("lang", GetCurrentI18N());
		dictionary.Add("emailId", info.Id);
		dictionary.Add("uid", DB.Account.Userid);
		ServerCommonSign.Sign(ref headers, dictionary);
		ownSingleHttp.AddHeader(headers);
		ownSingleHttp.AddParameter(JsonConvert.SerializeObject(dictionary));
		ownSingleHttp.AddCustomTimeout(50f);
		ownSingleHttp.Send();
	}

	private List<MailData> ParseServerEmailList(List<EmailServerInfo> list)
	{
		List<MailData> list2 = new List<MailData>();
		if (list != null)
		{
			List<string> list3 = new List<string>();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].status != 2 && list[i].status >= 0 && !list3.Contains(list[i].emailId) && ServerDateTimeMgr.I.NetTimeUtc.TotalSeconds() <= list[i].expireAt)
				{
					list3.Add(list[i].emailId);
					list2.Add(new MailData(list[i]));
				}
			}
		}
		return list2;
	}

	public string GetCurrentI18N()
	{
		switch (Enum.Parse<SystemLanguage>(LanguageManager.Instance.Language))
		{
		case SystemLanguage.Chinese:
			return "zh-CN";
		case SystemLanguage.French:
			return "fr-FR";
		case SystemLanguage.German:
			return "de-DE";
		case SystemLanguage.Japanese:
			return "ja-JP";
		case SystemLanguage.Korean:
			return "ko-KR";
		case SystemLanguage.Portuguese:
			return "pt-PT";
		case SystemLanguage.Russian:
			return "ru-RU";
		case SystemLanguage.Spanish:
			return "es-ES";
		case SystemLanguage.ChineseSimplified:
			return "zh-CN";
		case SystemLanguage.ChineseTraditional:
			return "zh-TW";
		default:
			return "en-US";
		}
	}

	private void Update()
	{
		time += Time.deltaTime;
		if (time >= 1f)
		{
			time -= 1f;
			if ((DateTime.Now - LastEmailListReqTime).TotalSeconds > (double)AutoReqEmailListIntervalTime)
			{
				GetEmails();
			}
		}
	}

	public EmailManager()
	{
		AutoReqEmailListIntervalTime = 300;
		CacheEmails = new List<MailData>();
	}
}
