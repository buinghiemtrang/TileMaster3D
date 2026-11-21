using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using UnityEngine;
using UnityEngine.UI;

namespace Ranking
{
	public abstract class RankingControlBase : MonoBehaviour
	{
		[SerializeField]
		protected RankingScrollView pScroll;

		[SerializeField]
		protected RankingCellItem pUserItem;

		[SerializeField]
		protected Image ImageLine;

		protected readonly List<RankingData> _data = new List<RankingData>(100);

		private Action<bool, HTTPRequestStates> _loadCallback;

		private HTTPRequest request;

		private Coroutine deserialIE;

		protected abstract string Field { get; }

		public virtual int RequestCount => 100;

		public virtual string Query => "{}";

		public abstract string Address { get; }

		public virtual int DataCount => _data.Count;

		protected abstract string StorageKey_Data { get; }

		protected abstract string StorageKey_RefreshTime { get; }

		protected virtual bool Descending => true;

		public virtual int OwnRanking => pUserItem.Ranking;

		public virtual RankingData GetData(int index)
		{
			return _data[index];
		}

		protected virtual RankingData GetUserData()
		{
			return _data.Find((RankingData x) => x.userID == DB.Account.Userid);
		}

		protected void Awake()
		{
			pUserItem.gameObject.SetActive(false);
		}

		protected virtual void OnDisable()
		{
			if (request != null)
			{
				request.Abort();
				request = null;
			}
			if (_loadCallback != null)
			{
				_loadCallback = null;
			}
			if (deserialIE != null)
			{
				StopCoroutine(deserialIE);
			}
		}

		protected void RequestRankingData()
		{
			if (request != null)
			{
				request.Abort();
				request = null;
			}
			request = new HTTPRequest(new Uri(Address), delegate(HTTPRequest req, HTTPResponse resp)
			{
				if (req.State == HTTPRequestStates.Finished)
				{
					if (resp == null)
					{
						Debug.Log("Get ranking data failed:resp is null");
						OnRequestFailed(req.State);
					}
					else
					{
						if (!resp.IsSuccess)
						{
							Debug.Log(string.Format("Get ranking data failed with server sent ERROR:{0}-{1}-{2}", resp.StatusCode, resp.Message, resp.DataAsText));
							OnRequestFailed(req.State);
							request = null;
							return;
						}
						ProcessRespondData(resp.DataAsText);
						OnRequestSucceeded(req.State);
						_loadCallback = null;
					}
					request = null;
				}
				else
				{
					Debug.Log(string.Format("Get ranking data failed with request STATE: {0}", req.State));
					OnRequestFailed(req.State);
					request = null;
				}
			});
			request.IsKeepAlive = false;
			request.DisableCache = true;
			request.ConnectTimeout = TimeSpan.FromSeconds(5.0);
			request.Timeout = TimeSpan.FromSeconds(5.0);
			request.AddHeader("Content-Type", "application/json");
			request.AddHeader("Authorization", "Bearer " + DataManager.I.Token);
			request.AddHeader("client-version", Application.version);
			request.Send();
			LogEvent.I.Track("Rank_Request").CommonAttr().Send();
		}

		protected virtual void OnRequestFailed(HTTPRequestStates state)
		{
			_loadCallback?.Invoke(false, state);
		}

		protected virtual void OnRequestSucceeded(HTTPRequestStates state)
		{
			RefreshContent();
			_loadCallback?.Invoke(true, state);
		}

		protected virtual void ProcessRespondData(string respondContent)
		{
			DeserializeProcessRespondData(respondContent);
			StorageMgr.SetString(StorageKey_RefreshTime, DateTime.Now.AddSeconds(5.0).ToString());
			StorageMgr.SetString(StorageKey_Data, respondContent);
		}

		protected abstract void DeserializeProcessRespondData(string respondContent);

		public virtual void LoadData(Action<bool, HTTPRequestStates> initCallback)
		{
			if (pScroll != null)
			{
				pScroll.Controller = this;
			}
			if (pUserItem != null)
			{
				pUserItem.gameObject.SetActive(false);
			}
			_loadCallback = initCallback;
			DateTime now = DateTime.Now;
			if (StorageMgr.GetDateTime(StorageKey_RefreshTime, now) > now && StorageMgr.IsHave(StorageKey_Data))
			{
				if (RequestCount <= 100)
				{
					DeserializeProcessRespondData(StorageMgr.GetString(StorageKey_Data, "{}"));
					RefreshContent();
					initCallback?.Invoke(true, HTTPRequestStates.Finished);
				}
				else
				{
					deserialIE = StartCoroutine(IEDeserialData(initCallback));
				}
			}
			else
			{
				RequestRankingData();
			}
		}

		public virtual void RefreshContent()
		{
			if (!(pScroll == null))
			{
				pScroll.Controller = this;
				pScroll.gameObject.SetActive(true);
				RankingData userData = GetUserData();
				SetShowOwnItem(userData != null);
				if (userData != null)
				{
					pUserItem.InitData(userData);
				}
				pScroll.RefreshItems(RequestCount, userData != null);
			}
		}

		public virtual void SetShowOwnItem(bool bShow)
		{
			if (ImageLine != null)
			{
				ImageLine.gameObject.SetActive(bShow);
			}
			if (pUserItem != null)
			{
				pUserItem.gameObject.SetActive(bShow);
			}
			float height = pScroll.transform.parent.RT().rect.height;
			float num = (bShow ? 166f : 0f);
			pScroll.transform.RT().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, num, height - num);
		}

		private IEnumerator IEDeserialData(Action<bool, HTTPRequestStates> initCallback)
		{
			DeserializeProcessRespondData(StorageMgr.GetString(StorageKey_Data, "{}"));
			yield return new WaitForSeconds(2f);
			RefreshContent();
			initCallback?.Invoke(true, HTTPRequestStates.Finished);
		}
	}
}
