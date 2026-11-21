using UnityEngine;
using UnityEngine.UI;
using zFrame.Event;

namespace Game.Activity.BakingMaster
{
	public class BakingMasterActivityRootDialog : MonoBehaviour
	{
		private DialogShow _pDialog;

		[SerializeField]
		private BakingMasterBrandItem[] items = new BakingMasterBrandItem[15];

		[SerializeField]
		private SpineUIControl _spine;

		[SerializeField]
		private Text ttCage;

		[SerializeField]
		private AnimationControl[] aniLayer = new AnimationControl[5];

		[SerializeField]
		private Animation[] aniGift = new Animation[5];

		[SerializeField]
		private Animator aniBaking;

		private RequestStatus _nRequestStatus;

		private const string CounterKey = "BakingMasterRefreshUI";

		private bool bLogEventOne = true;

		private void OnDestroy()
		{
			DispatcherMgr.Off("BakingMaster_RefreshActivityData", RefreshActivityData);
			SceneMgr.CloseWaiting();
		}

		private void Awake()
		{
			DispatcherMgr.On("BakingMaster_RefreshActivityData", RefreshActivityData);
			DispatcherMgr.On("BakingMaster_UpdateOwnGrade", RefreshActivityData);
			_pDialog = GetComponent<DialogShow>();
			for (int i = 0; i < items.Length; i++)
			{
				BakingMasterBrandItem item = items[i];
				item.button.onClick.AddListener(delegate
				{
					ClickEnterLevel(item);
				});
			}
			_nRequestStatus = RequestStatus.None;
			ActivityHttpMgr.I.RequestActivityData();
		}

		private void Start()
		{
			for (int i = 0; i < items.Length; i++)
			{
				items[i].Init(DB.BakingMasterActivity.Levels[i].data, i);
			}
			UpdateCageCount(0);
			for (int j = 1; j < DB.BakingMasterActivity.UnlockLayerStatus.Length; j++)
			{
				aniLayer[j].Play(string.Format(DB.BakingMasterActivity.UnlockLayerStatus[j] ? "cake_unlock_{0}" : "cake_lock_{0}", j + 1));
			}
			for (int k = 0; k < aniGift.Length; k++)
			{
				aniGift[k].gameObject.SetActive(!DB.BakingMasterActivity.Data.receive[k]);
			}
			string clipName = DB.BakingMasterActivity.IsUnlockLayer(5) ? "cake-into-1" : "cake-into-2";
			aniBaking.SetTarget(clipName).OnCompleted(delegate
			{
				aniBaking.StopPlayback();
				if (_nRequestStatus == RequestStatus.Complete)
				{
					RefreshUI();
				}
				else
				{
					RetryNetwork();
				}
			});
			aniBaking.Play(clipName);
		}

		private void RetryNetwork()
		{
			_nRequestStatus = RequestStatus.None;
			Global.ShowRetryDialog("Language55", delegate(bool isRetry)
			{
				if (!isRetry)
				{
					_pDialog.ClickClose();
				}
				else if (_nRequestStatus != RequestStatus.Complete)
				{
					_nRequestStatus = RequestStatus.Request;
					ActivityHttpMgr.I.RequestActivityData();
					SceneMgr.OpenWaiting(3f, 0f, RetryNetwork);
				}
				else
				{
					RefreshUI();
				}
			});
		}

		private void RefreshActivityData()
		{
			if (!DB.BakingMasterActivity.GameFailure.Issue.Equals(DB.BakingMasterActivity.Info.Issue))
			{
				SceneMgr.CloseWaiting();
				if (_nRequestStatus == RequestStatus.Request)
				{
					RefreshUI();
				}
				_nRequestStatus = RequestStatus.Complete;
			}
			else
			{
				ActivityHttpMgr.I.PostActivityGrade(DB.BakingMasterActivity.GameFailure.LevelId, DB.BakingMasterActivity.GameFailure.Grade);
			}
		}

		private void RefreshUI()
		{
			DataCenter.BakingMasterActivity db = DB.BakingMasterActivity;
			if (bLogEventOne)
			{
				bLogEventOne = false;
				LogEvent.I.Track("Cake_Baking_Open").Add("ActivityTime", db.Info.Issue).CommonAttr().Send();
			}
			for (int i = 0; i < items.Length; i++)
			{
				items[i].RefreshData();
			}
			UpdateCageCount(-1);
			CounterMgr.Create(delegate
			{
				for (int k = 0; k < aniGift.Length; k++)
				{
					if ((k == 0 || db.UnlockLayerStatus[k]) && !db.Data.receive[k] && db.IsCompleteLayer(k + 1))
					{
						aniGift[k].Play("gift-tips");
					}
				}
			}, CounterKey, true);
			for (int j = 2; j <= 5; j++)
			{
				if (db.IsUnlockLayer(j) && !db.UnlockLayerStatus[j - 1])
				{
					UnlockLayer(j - 1);
					db.UnlockLayerStatus[j - 1] = true;
					db.SaveUnlockLayerStatus();
				}
			}
			CounterMgr.Finish(CounterKey);
		}

		private void UnlockLayer(int id)
		{
			CounterMgr.Add(CounterKey);
			aniLayer[id].Play(string.Format("cake_unlocking_{0}", id + 1));
			aniLayer[id].Add(string.Format("cake_unlock_{0}", id + 1));
			aniLayer[id].SetCompleteOnce(delegate
			{
				CounterMgr.Del(CounterKey);
			});
		}

		private void UpdateCageCount(int value = -1)
		{
			if (value == -1)
			{
				value = DB.BakingMasterActivity.Loser.Count;
			}
			ttCage.SetLBy(value);
			if (value == 0)
			{
				_spine.FrameStart(0);
			}
			else
			{
				_spine.Play("animation", true);
			}
		}

		public void ClickBtnGift(MButton button)
		{
			if (button != null)
			{
				DataCenter.BakingMasterActivity db = DB.BakingMasterActivity;
				int id = button.nTag;
				if (db.IsCompleteLayer(id))
				{
					int num = id - 1;
					if (!db.Data.receive[num])
					{
						button.gameObject.SetActive(false);
						db.Data.receive[num] = true;
						db.SaveData();
						Global.CreateReward(Singleton<ActivityMgr>.I.CfgGiftReward[num], RewardType.None, REWARD_PLACE.None, "BakingMaster", false);
						LogEvent.I.Track("Cake_Baking_Reward")
							.Add("ActivityTime", db.Info.Issue)
							.Add("Team_ID", db.Info.myTeam.id)
							.Add("TeamBox", button.nTag)
							.Add("CakeGrade", Singleton<ActivityMgr>.I.GetUnlockLayerCount())
							.Add("MyFirst", Singleton<ActivityMgr>.I.GetMyFirstCount())
							.CommonAttr()
							.Send();
					}
				}
			}
		}

		private void ClickEnterLevel(BakingMasterBrandItem item)
		{
			BakingMasterActivityEnterLevelDialog.Create(item.LevelId, _pDialog.ClickClose);
		}

		public void ClickBtnCage(MButton button)
		{
			BakingMasterActivityCageDialog dialog = SceneMgr.AddTopNode<BakingMasterActivityCageDialog>("Prefabs/Activity/BakingMaster/ActivityCageDialog");
			dialog.GetComponent<DialogShow>().func_callback_close = delegate
			{
				UpdateCageCount(-1);
			};
		}

		public static BakingMasterActivityRootDialog Create()
		{
			return SceneMgr.AddTopNode<BakingMasterActivityRootDialog>("Prefabs/Activity/BakingMaster/ActivityRootDialog");
		}
	}
}
