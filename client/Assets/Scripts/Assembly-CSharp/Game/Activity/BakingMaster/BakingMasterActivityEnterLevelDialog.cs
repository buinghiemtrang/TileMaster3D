using System;
using System.Collections.Generic;
using DG.Tweening;
using GameScene_Level;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Activity.BakingMaster
{
	public class BakingMasterActivityEnterLevelDialog : MonoBehaviour
	{
		private DialogShow _pDialog;

		[SerializeField]
		private MButton btnPlay;

		private Action _funcClickPlay;

		[SerializeField]
		private GameObject objUser1;

		[SerializeField]
		private GameObject objDesc;

		[SerializeField]
		private Text ttLevel;

		[SerializeField]
		private Text ttName;

		[SerializeField]
		private Text ttLevelTime;

		private RequestStatus _nRequestStatus;

		private int _levelId;

		private void OnDestroy()
		{
			DispatcherMgr.Off("BakingMaster_RefreshActivityInfo", _requestIssueCallBack);
		}

		private void Awake()
		{
			_pDialog = GetComponent<DialogShow>();
			DispatcherMgr.On("BakingMaster_RefreshActivityInfo", _requestIssueCallBack);
		}

		private void Start()
		{
			ttLevel.SetLBy(_levelId + 1);
			List<RequestMatchData> levelData = DB.BakingMasterActivity.GetLevelData(_levelId);
			objDesc.SetActive(levelData == null || levelData.Count == 0);
			objUser1.SetActive(!objDesc.activeSelf);
			if (objUser1.activeSelf)
			{
				ttName.text = levelData[0].GetUserName();
				ttLevelTime.text = string.Format("{0}s", levelData[0].grade);
			}
		}

		public void ClickBtnHelp(MButton button)
		{
			SceneMgr.AddTopNode<DialogShow>("Prefabs/Activity/BakingMaster/ActivityHelpDialog");
		}

		public void ClickBtnPlay(MButton button)
		{
			if (DB.BakingMasterActivity.IsLoser(DB.Account.Userid))
			{
				Global.ShowMsgTip("Language408");
				return;
			}
			if (_nRequestStatus == RequestStatus.Complete)
			{
				_requestIssueCallBack();
				return;
			}
			_nRequestStatus = RequestStatus.Request;
			ActivityHttpMgr.I.RequestIssueById();
			btnPlay.gameObject.SetActive(false);
			TweenMgr.DelayCall(2f, btnPlay.Show).SetId(btnPlay);
		}

		private void _requestIssueCallBack()
		{
			btnPlay.DOKill();
			_nRequestStatus = RequestStatus.Complete;
			ErrorCode errorCode = Singleton<ActivityMgr>.I.JudgeEnterCondition();
			if (errorCode == ErrorCode.NotTeam)
			{
				Global.ShowMsgTip("Language409");
				btnPlay.Show();
				return;
			}
			if (errorCode == ErrorCode.Ended)
			{
				Global.ShowMsgTip("Language410");
				btnPlay.Show();
				return;
			}
			Singleton<ActivityMgr>.I.LevelId = _levelId;
			DB.BakingMasterActivity.SetFailureData();
			RequestIssueData info = DB.BakingMasterActivity.Info;
			info.RefreshTime();
			int dayOfYear = info.StartTime.DayOfYear;
			int count = DataCfg.cfgBakingMasterLevels.Count;
			int groupId = dayOfYear % count + 1;
			BlackCutDialog.EnterGame(null, LevelType.BakingMaster, groupId, _levelId);
			_pDialog._Close(_funcClickPlay);
		}

		public static BakingMasterActivityEnterLevelDialog Create(int levelId, Action func)
		{
			BakingMasterActivityEnterLevelDialog bakingMasterActivityEnterLevelDialog = SceneMgr.AddTopNode<BakingMasterActivityEnterLevelDialog>("Prefabs/Activity/BakingMaster/ActivityEnterLevelDialog");
			bakingMasterActivityEnterLevelDialog._levelId = levelId;
			bakingMasterActivityEnterLevelDialog._funcClickPlay = func;
			return bakingMasterActivityEnterLevelDialog;
		}
	}
}
