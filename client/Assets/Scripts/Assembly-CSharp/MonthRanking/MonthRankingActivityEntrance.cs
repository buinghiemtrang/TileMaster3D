using DG.Tweening;
using FunctionEject;

namespace MonthRanking
{
	public class MonthRankingActivityEntrance : EjectItem
	{
		private int _surTime;

		private Tweener _timeTw;

		private string festivalType = "Ordinary";

		private void Awake()
		{
			DispatcherMgr.On<int>("MonthRankingActivity_GetServerTime", _Countdown);
			DispatcherMgr.On("MonthRanking_GetActivityInfo_Succ", _GetRankingData);
		}

		public override void Init()
		{
			MonthRankingHttpMgr.I.GetPersonalRankingActivity();
			_GetRankingData();
			_Countdown(-1);
		}

		public override bool Eject()
		{
			int num = MonthRankingActivityMgr.I.JudgeAutoEject();
			if (num == 0)
			{
				return false;
			}
			if (num == 1)
			{
				_EnterPosterDialog();
			}
			else if (num == 2)
			{
				_EnterActivityDialog();
			}
			return true;
		}

		private void _EnterPosterDialog()
		{
			string path = string.Concat("Prefabs/Activity/MonthRanking/", festivalType, "/MonthRankingActivityPosterDialog");
			MonthRankingActivityPosterDialog dialog = SceneMgr.AddTopNode<MonthRankingActivityPosterDialog>(path);
			dialog.closeCallback = delegate(bool isCancel)
			{
				End(isCancel ? EjectType.Break : EjectType.Continue);
			};
			dialog.Show("Initiative", _surTime);
		}

		private void _EnterActivityDialog()
		{
			MonthRankingActivityDialog dialog = SceneMgr.AddTopNode<MonthRankingActivityDialog>("Prefabs/Activity/MonthRanking/MonthRankingActivityDialog");
			dialog.closeCallback = delegate
			{
				End(EjectType.Continue);
			};
			dialog.Show("Initiative", _surTime);
		}

		private void _Countdown(int second = -1)
		{
			TweenMgr.OwnKill(_timeTw);
			if (second == -1)
			{
				second = MonthRankingActivityMgr.I.GetLocalEndTimeSpan();
			}
			_surTime = second;
			if (_surTime > 0)
			{
				_timeTw = TweenMgr.NumberTo((float)_surTime, _surTime, 0, delegate(int x)
				{
					_surTime = x;
					_SendTimeChangeEvent();
				}).SetEase(Ease.Linear).SetId("MonthRankingActivityEntrance_Time").OnComplete(MonthRankingHttpMgr.I.GetPersonalRankingActivity);
			}
			else
			{
				_SendTimeChangeEvent();
			}
		}

		private void _GetRankingData()
		{
			if (MonthRankingActivityMgr.I.JudgeOpen() && DB.MonthRankingActivity.PersonalMonthStarRankingDatas.Count <= 0)
			{
				MonthRankingActivityMgr.I.RequestRankingData();
			}
		}

		private void _SendTimeChangeEvent()
		{
			DispatcherMgr.Emit("MonthRankingActivity_TimeChange", _surTime);
		}

		private void OnDestroy()
		{
			TweenMgr.OwnKill(_timeTw);
			DispatcherMgr.Off<int>("MonthRankingActivity_GetServerTime", _Countdown);
			DispatcherMgr.Off("MonthRanking_GetActivityInfo_Succ", _GetRankingData);
		}
	}
}
