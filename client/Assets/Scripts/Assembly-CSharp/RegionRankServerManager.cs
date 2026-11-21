using System;
using System.Collections.Generic;
using System.Diagnostics;
using BestHTTP;
using Newtonsoft.Json;
using OwnHttp;

public class RegionRankServerManager : Singleton<RegionRankServerManager>
{
	private const string UploadScoreUrl = "https://service.fuqiangame.com/rank/api/v1/app/upCustomRank";

	private const string GetRankUrl = "https://service.fuqiangame.com/rank/api/v1/app/findCustomRankList";

	private RegionRankClientData _personalCacheData;

	private RegionRankClientData _nationalCacheData;

	public string RegionCode => GSingleton<RegionRankActivityManager>.ins.GetRegionCode();

	public int Issue => GSingleton<RegionRankActivityManager>.ins.GetSelfData().Issue;

	public long EndTime => GSingleton<RegionRankActivityManager>.ins.GetSelfData().EndTime;

	private long NowTimestamp => GSingleton<RegionRankActivityManager>.ins.GetTimeStamp(ServerDateTimeMgr.I.NetTimeUtc);

	private OwnSingleHttp _currnthHttp { get; set; }

	public string GetPersonalRankType()
	{
		return string.Format("region_rank_personal_{0}_{1}", RegionCode, Issue);
	}

	private string GetPersonalExtraData()
	{
		RegionRankExtraData regionRankExtraData = new RegionRankExtraData();
		regionRankExtraData.name = DB.Account.GetName();
		regionRankExtraData.medal = DB.User.Medal;
		return JsonConvert.SerializeObject(regionRankExtraData);
	}

	public string GetNationalRankType()
	{
		return string.Format("region_rank_national_{0}", Issue);
	}

	public void UploadSelfScore(Action<bool> callback)
	{
		Singleton<GuestLoginManager>.I.Login(delegate(GuestLoginState state)
		{
			if (state != GuestLoginState.Success)
			{
				callback?.Invoke(false);
			}
			else if (GSingleton<RegionRankActivityManager>.ins.GetSelfData().AddScore == 0)
			{
				callback(true);
			}
			else
			{
				List<RegionRankUploadData> list = new List<RegionRankUploadData>();
				RegionRankUploadData regionRankUploadData = new RegionRankUploadData();
				regionRankUploadData.rankType = GetPersonalRankType();
				regionRankUploadData.member = DB.Account.Userid;
				regionRankUploadData.score = GSingleton<RegionRankActivityManager>.ins.GetSelfData().Score;
				regionRankUploadData.expireAt = EndTime;
				regionRankUploadData.isAddUp = false;
				regionRankUploadData.extraData = GetPersonalExtraData();
				list.Add(regionRankUploadData);
				if (!GSingleton<RegionRankActivityManager>.ins.IsUnknownRegion())
				{
					RegionRankUploadData regionRankUploadData2 = new RegionRankUploadData();
					regionRankUploadData2.rankType = GetNationalRankType();
					regionRankUploadData2.member = RegionCode;
					regionRankUploadData2.score = GSingleton<RegionRankActivityManager>.ins.GetSelfData().AddScore;
					regionRankUploadData2.expireAt = EndTime;
					regionRankUploadData2.isAddUp = true;
					list.Add(regionRankUploadData2);
				}
				UploadScore(list, delegate(bool succ)
				{
					if (succ)
					{
						RefreshNationalCache();
					}
					callback?.Invoke(succ);
				});
			}
		});
	}

	public void UploadScore(List<RegionRankUploadData> data, Action<bool> callback)
	{
		if (data == null || data.Count == 0 || !DataManager.I.bLogined)
		{
			return;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("list", data);
		RegionRankUploadData regionRankUploadData = data[0];
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2.Add("rankType", regionRankUploadData.rankType);
		dictionary2.Add("member", regionRankUploadData.member);
		dictionary2.Add("score", regionRankUploadData.score);
		dictionary2.Add("expireAt", regionRankUploadData.expireAt);
		Dictionary<string, string> headers = RequestsMgr.GetCommonHeaders();
		RequestsMgr.Sign(ref headers, dictionary2);
		OwnSingleHttp ownSingleHttp = RequestsMgr.I.Create(HTTPMethods.Post, "https://service.fuqiangame.com/rank/api/v1/app/upCustomRank", delegate(ResultInfo info)
		{
			if (info.State != RequestState.Success)
			{
				callback?.Invoke(false);
			}
			else
			{
				ServerDataRootBase serverDataRootBase = JsonConvert.DeserializeObject<ServerDataRootBase>(info.Response.DataAsText);
				if (serverDataRootBase.code == 1)
				{
					callback?.Invoke(true);
				}
				else
				{
					// UNVERIFIED: the original calls LogError (a [Conditional] method) when msg is not empty; that call is not present in the machine code.
					callback?.Invoke(false);
				}
			}
		});
		ownSingleHttp.AddHeader(headers);
		ownSingleHttp.AddParameter(JsonConvert.SerializeObject(dictionary));
		ownSingleHttp.AddCustomTimeout(30f);
		ownSingleHttp.Send();
	}

	public void ClearCache()
	{
		_personalCacheData = null;
		_nationalCacheData = null;
	}

	public int GetCachePersonalRanking()
	{
		if (_personalCacheData == null)
		{
			return -1;
		}
		if (GSingleton<RegionRankActivityManager>.ins.GetActivityState() == ActivityStateBase.StartPrepare)
		{
			return -1;
		}
		return _personalCacheData.SelfRank;
	}

	private void RefreshPersonalCache()
	{
		if (_personalCacheData != null)
		{
			RegionRankClientItemData regionRankClientItemData = new RegionRankClientItemData();
			RegionRankExtraData regionRankExtraData = new RegionRankExtraData();
			regionRankExtraData.name = DB.Account.GetName();
			regionRankExtraData.medal = DB.User.Medal;
			regionRankClientItemData.Data = regionRankExtraData;
			regionRankClientItemData.Key = DB.Account.Userid;
			regionRankClientItemData.Score = GSingleton<RegionRankActivityManager>.ins.GetSelfData().Score;
			RegionRankServerUtils.UpdatePersonalData(ref _personalCacheData, regionRankClientItemData);
		}
	}

	public void GetPersonalRankData(Action<bool, RegionRankClientData> callback, bool canUseCache = true)
	{
		if (canUseCache && _personalCacheData != null && _personalCacheData.NextRefreshAt - NowTimestamp > 0)
		{
			RefreshPersonalCache();
			callback?.Invoke(true, _personalCacheData);
			return;
		}
		if (DataManager.I.bLogined && GSingleton<RegionRankActivityManager>.ins.GetSelfData().AddScore < 1)
		{
			todo();
			return;
		}
		UploadSelfScore(delegate(bool succ)
		{
			GSingleton<RegionRankActivityManager>.ins.UpScoreCallback(succ);
			todo();
		});

		void todo()
		{
			GetRankData(GetPersonalRankType(), EndTime, DB.Account.Userid, delegate(bool succ, RegionRankClientData clientData)
			{
				if (succ)
				{
					_personalCacheData = clientData;
					if (_personalCacheData.List.Count == 0)
					{
						_personalCacheData.NextRefreshAt = NowTimestamp;
					}
					if (canUseCache)
					{
						RefreshPersonalCache();
					}
					callback?.Invoke(true, _personalCacheData);
				}
				else if (canUseCache && _personalCacheData != null)
				{
					RefreshPersonalCache();
					callback?.Invoke(true, _personalCacheData);
				}
				else
				{
					callback?.Invoke(false, null);
				}
			});
		}
	}

	private void RefreshNationalCache()
	{
		if (_nationalCacheData != null && GSingleton<RegionRankActivityManager>.ins.GetSelfData().AddScoreCount != 0)
		{
			RegionRankClientItemData regionRankClientItemData = new RegionRankClientItemData();
			regionRankClientItemData.Key = RegionCode;
			regionRankClientItemData.Score = GSingleton<RegionRankActivityManager>.ins.GetSelfData().AddScore;
			RegionRankServerUtils.UpdateNationalData(ref _nationalCacheData, regionRankClientItemData);
			GSingleton<RegionRankActivityManager>.ins.ResetAddScoreCount();
		}
	}

	public void GetNationalRankData(Action<bool, RegionRankClientData> callback, bool canUseCache = true)
	{
		if (canUseCache && _nationalCacheData != null && _nationalCacheData.NextRefreshAt - NowTimestamp > 0)
		{
			RefreshNationalCache();
			callback?.Invoke(true, _nationalCacheData);
			return;
		}
		if (DataManager.I.bLogined && GSingleton<RegionRankActivityManager>.ins.GetSelfData().AddScore < 1)
		{
			todo();
			return;
		}
		UploadSelfScore(delegate(bool succ)
		{
			GSingleton<RegionRankActivityManager>.ins.UpScoreCallback(succ);
			todo();
		});

		void todo()
		{
			GetRankData(GetNationalRankType(), EndTime, RegionCode, delegate(bool succ, RegionRankClientData clientData)
			{
				if (succ)
				{
					_nationalCacheData = clientData;
					if (_nationalCacheData.List.Count == 0)
					{
						_nationalCacheData.NextRefreshAt = NowTimestamp;
					}
					if (canUseCache)
					{
						RefreshNationalCache();
					}
					callback?.Invoke(true, _nationalCacheData);
				}
				else if (canUseCache && _nationalCacheData != null)
				{
					RefreshNationalCache();
					callback?.Invoke(true, _nationalCacheData);
				}
				else
				{
					callback?.Invoke(false, null);
				}
			});
		}
	}

	private void GetRankData(string rankType, long expireAt, string member, Action<bool, RegionRankClientData> callback)
	{
		Dictionary<string, string> headers = null;
		if (rankType == null || member == null)
		{
			return;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("rankType", rankType);
		dictionary.Add("expireAt", expireAt);
		dictionary.Add("member", member);
		dictionary.Add("page", 1);
		dictionary.Add("size", 100);
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2.Add("rankType", rankType);
		dictionary2.Add("expireAt", expireAt);
		headers = RequestsMgr.GetCommonHeaders();
		RequestsMgr.Sign(ref headers, dictionary2);
		_currnthHttp = RequestsMgr.I.Create(HTTPMethods.Post, "https://service.fuqiangame.com/rank/api/v1/app/findCustomRankList", delegate(ResultInfo info)
		{
			_currnthHttp = null;
			if (info.State != RequestState.Success)
			{
				callback?.Invoke(false, null);
				return;
			}
			ServerDataRoot<RegionRankServerData> serverDataRoot = JsonConvert.DeserializeObject<ServerDataRoot<RegionRankServerData>>(info.Response.DataAsText);
			if (serverDataRoot.code == 1 && serverDataRoot.data != null)
			{
				RegionRankClientData regionRankClientData = RegionRankServerUtils.ParseData(serverDataRoot.data);
				if (regionRankClientData != null)
				{
					callback?.Invoke(true, regionRankClientData);
				}
				else
				{
					callback?.Invoke(false, null);
				}
			}
			else
			{
				// UNVERIFIED: the original calls LogError (a [Conditional] method) when msg is not empty; that call is not present in the machine code.
				callback?.Invoke(false, null);
			}
		});
		_currnthHttp.AddHeader(headers);
		_currnthHttp.AddParameter(JsonConvert.SerializeObject(dictionary));
		_currnthHttp.AddCustomTimeout(30f);
		_currnthHttp.Send();
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("OPEN_TEST_BUTTON")]
	private void LogError(string msg)
	{
		string message = "[RegionRankServerManager] " + msg;
		UnityEngine.Debug.unityLogger.Log(UnityEngine.LogType.Error, message);
	}

	public void ForceAbort()
	{
		if (_currnthHttp != null)
		{
			_currnthHttp.ForceAbort();
			_currnthHttp = null;
		}
	}
}
