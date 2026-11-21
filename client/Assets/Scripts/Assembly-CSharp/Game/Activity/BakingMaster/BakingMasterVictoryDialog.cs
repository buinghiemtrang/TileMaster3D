using DG.Tweening;
using GameScene_Element;
using GameScene_Level;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Activity.BakingMaster
{
	public class BakingMasterVictoryDialog : VictoryBase
	{
		private DialogShow _pDialog;

		[SerializeField]
		private Text ttGrade;

		[SerializeField]
		private GameObject objHint;

		[SerializeField]
		private GameObject objRetry;

		[SerializeField]
		private GameObject objBtnTrue;

		private RequestStatus _requestStatus;

		private int _levelId;

		private int _grade;

		private void OnDestroy()
		{
			DispatcherMgr.Off("BakingMaster_UpdateOwnGrade", UpdateOwnGrade);
			DOTween.Kill(this);
		}

		private void Awake()
		{
			DispatcherMgr.On("BakingMaster_UpdateOwnGrade", UpdateOwnGrade);
			_pDialog = GetComponent<DialogShow>();
			objHint.SetActive(false);
			objRetry.SetActive(false);
			_requestStatus = RequestStatus.None;
			_pDialog.func_callback_close = delegate
			{
				BlackCutDialog.Show(delegate
				{
					GiftDataMgr.ins.ResetShowPopStatus(true);
					SceneMgr.SetData("OpenBakingMaster");
					SceneMgr.LoadScene("MainScene", "GameScene", delegate
					{
						BlackCutDialog.Hide();
					});
				});
			};
			DB.BakingMasterActivity.ClearFailureData();
			ModifySumStar(DB.User.SumStarLevel);
		}

		private void Start()
		{
			AudioMgr.I.PauseMusic();
			AudioMgr.I.PlaySound("victory");
			int totalNumber = ElementsMgr.I.Data.TotalNumber;
			int remainingNumber = ElementsMgr.I.Data.RemainingNumber;
			LogEvent.I.Level_Victory(LevelMgr.Data.Type, totalNumber, remainingNumber, false);
		}

		public void ClickBtnTrue(MButton button)
		{
			objBtnTrue.SetActive(false);
			objRetry.SetActive(false);
			if (_requestStatus == RequestStatus.Complete)
			{
				_pDialog.ClickClose();
				return;
			}
			if (!DB.BakingMasterActivity.Levels.TryGet(_levelId, out LevelData value))
			{
				_pDialog.ClickClose();
				return;
			}
			if (_levelId == 14 && value.data.Count == 0)
			{
				ActivityHttpMgr.I.RequestClearLoserAll();
			}
			if (value.data.Count > 0)
			{
				RequestMatchData first = value.data[0];
				if (first.userId.Equals(DB.Account.Userid) && first.grade <= _grade)
				{
					_pDialog.ClickClose();
					return;
				}
			}
			objHint.SetActive(true);
			_requestStatus = RequestStatus.Request;
			ActivityHttpMgr.I.PostActivityGrade(_levelId + 1, _grade);
			TweenMgr.DelayCall(2f, delegate
			{
				objHint.SetActive(false);
				objRetry.SetActive(true);
			}).SetId(this);
		}

		public void ClickBtnClose(MButton button)
		{
			_pDialog.ClickClose();
		}

		private void UpdateOwnGrade()
		{
			_requestStatus = RequestStatus.Complete;
			DOTween.Kill(this);
			_pDialog.ClickClose();
		}

		public static BakingMasterVictoryDialog Create(int grade)
		{
			BakingMasterVictoryDialog bakingMasterVictoryDialog = SceneMgr.AddTopNode<BakingMasterVictoryDialog>("Prefabs/Activity/BakingMaster/ActivityVictoryDialog");
			bakingMasterVictoryDialog._levelId = Singleton<ActivityMgr>.I.LevelId;
			bakingMasterVictoryDialog._grade = grade;
			bakingMasterVictoryDialog.ttGrade.SetLBy(grade);
			return bakingMasterVictoryDialog;
		}
	}
}
