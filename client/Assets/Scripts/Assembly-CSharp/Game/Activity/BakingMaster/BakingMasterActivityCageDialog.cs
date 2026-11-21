using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Activity.BakingMaster
{
	public class BakingMasterActivityCageDialog : MonoBehaviour
	{
		[SerializeField]
		private Text ttPrice;

		[SerializeField]
		private Text ttSurplus;

		[SerializeField]
		private Text[] ttNames;

		[SerializeField]
		private MButton btnSure;

		private RequestStatus _nRequestStatus;

		private void OnDestroy()
		{
			DispatcherMgr.Off("BakingMaster_RefreshLoserData", RequestCallBack);
		}

		private void Awake()
		{
			DispatcherMgr.On("BakingMaster_RefreshLoserData", RequestCallBack);
		}

		private void Start()
		{
			ttPrice.text = cf_const.activityBakingMasterReviveGold.ToString();
			List<RequestMatchData> list = DB.BakingMasterActivity.Loser.Get();
			RequestMatchData requestMatchData = list.Find(a => a.userId == DB.Account.Userid);
			int num = 0;
			if (requestMatchData != null)
			{
				ttNames[0].text = requestMatchData.GetUserName();
				num = 1;
			}
			List<RequestMatchData> list2 = list.FindAll(a => a.userId != DB.Account.Userid);
			for (int i = 0; num + i < ttNames.Length; i++)
			{
				if (i < list2.Count)
				{
					ttNames[num + i].text = list2[i].GetUserName();
				}
				else
				{
					ttNames[num + i].text = string.Empty;
				}
			}
			btnSure.interactable = list.Count > 0;
			int num2 = list.Count - 4;
			if (num2 > 0)
			{
				ttSurplus.text = string.Format("......+{0}", num2);
			}
			else
			{
				ttSurplus.text = string.Empty;
			}
		}

		public void ClickBtnSure(MButton button)
		{
			if (DB.Bag.GetProp(0) < cf_const.activityBakingMasterReviveGold)
			{
				Global.ShowMsgTip("Language53");
				return;
			}
			_nRequestStatus = RequestStatus.Request;
			ActivityHttpMgr.I.RequestClearLoserAll();
			SceneMgr.OpenWaiting(3f, 0f, RetryNetwork);
		}

		public void RequestCallBack()
		{
			DB.Bag.Modify(0, -cf_const.activityBakingMasterReviveGold, true, string.Empty);
			if (_nRequestStatus == RequestStatus.Request)
			{
				SceneMgr.CloseWaiting();
				Start();
			}
			_nRequestStatus = RequestStatus.Complete;
		}

		public void RefreshCage()
		{
			Start();
		}

		private void RetryNetwork()
		{
			_nRequestStatus = RequestStatus.None;
			Global.ShowRetryDialog("Language55", delegate(bool isRetry)
			{
				if (isRetry)
				{
					if (_nRequestStatus == RequestStatus.Complete)
					{
						Start();
					}
					else
					{
						_nRequestStatus = RequestStatus.Request;
						ActivityHttpMgr.I.RequestClearLoserAll();
						SceneMgr.OpenWaiting(3f, 0f, RetryNetwork);
					}
				}
			});
		}

		public static BakingMasterActivityCageDialog Create(Action callback)
		{
			BakingMasterActivityCageDialog bakingMasterActivityCageDialog = SceneMgr.AddTopNode<BakingMasterActivityCageDialog>("Prefabs/Activity/BakingMaster/ActivityCageDialog");
			bakingMasterActivityCageDialog.GetComponent<DialogShow>().func_callback_close = callback;
			return bakingMasterActivityCageDialog;
		}
	}
}
