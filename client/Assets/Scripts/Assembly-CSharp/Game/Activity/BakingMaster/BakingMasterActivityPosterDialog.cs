using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Activity.BakingMaster
{
	public class BakingMasterActivityPosterDialog : MonoBehaviour
	{
		private DialogShow _pDialog;

		public Action<bool> CloseCallback;

		[SerializeField]
		private MButton btnPlay;

		[SerializeField]
		private MButton btnJoin;

		[SerializeField]
		private Text ttTime;

		private void OnDestroy()
		{
			DispatcherMgr.Off<string, int>("UpdateBakingMasterActivityTime", UpdateActivityTime);
		}

		private void Awake()
		{
			_pDialog = GetComponent<DialogShow>();
			DispatcherMgr.On<string, int>("UpdateBakingMasterActivityTime", UpdateActivityTime);
			if (DB.BakingMasterActivity.State == 0)
			{
				DB.BakingMasterActivity.State = 2;
			}
		}

		private void UpdateActivityTime(string time, int remain)
		{
			ttTime.text = time;
			if (remain == 0)
			{
				_CloseSelf(false);
			}
		}

		public void ClickBtnPlay(MButton button)
		{
			_CloseSelf(true);
			DispatcherMgr.Emit("ActivityEntrance_ActivityEnterOtherPage");
			SceneMgr.AddTopNode<BakingMasterActivityRootDialog>("Prefabs/Activity/BakingMaster/ActivityRootDialog");
		}

		public void ClickBtnJoinTeam(MButton button)
		{
			_CloseSelf(false);
			DispatcherMgr.Emit("ActivityEntrance_ActivityEnterOtherPage");
			SceneMainBottom.instance.ClickBtnTeam(null);
		}

		public void ClickBtnClose(MButton btn)
		{
			_CloseSelf(false);
		}

		private void _CloseSelf(bool bIsEnterGame)
		{
			_pDialog._Close(delegate
			{
				CloseCallback?.Invoke(bIsEnterGame);
			});
		}

		public static BakingMasterActivityPosterDialog Create(bool bJoinTeam)
		{
			BakingMasterActivityPosterDialog bakingMasterActivityPosterDialog = SceneMgr.AddTopNode<BakingMasterActivityPosterDialog>("Prefabs/Activity/BakingMaster/ActivityPosterDialog");
			bakingMasterActivityPosterDialog.btnJoin.gameObject.SetActive(bJoinTeam);
			bakingMasterActivityPosterDialog.btnPlay.gameObject.SetActive(!bJoinTeam);
			return bakingMasterActivityPosterDialog;
		}
	}
}
