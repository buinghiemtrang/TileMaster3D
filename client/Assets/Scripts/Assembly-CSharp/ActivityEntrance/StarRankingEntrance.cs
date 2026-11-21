using Festival;

namespace ActivityEntrance
{
	public class StarRankingEntrance : EntranceItem
	{
		private StarRankingActivityDialog _activityDialog;

		private StarRankingActivityState _state;

		protected override void Awake()
		{
			base.Awake();
			DispatcherMgr.On("StarRankingActivity_Countdown_End", _CountdownEnd);
		}

		public override void Init(cfgActivityEntranceInformation cfg)
		{
			base.Init(cfg);
			lockObj.SetActive(false);
			SetDesc(cfg.ordinaryInfo, cfg.defaultOrdinaryInfo);
			_state = StarRankingActivityMgr.I.GetActivityState();
			bool active = _state == StarRankingActivityState.During || StarRankingActivityMgr.I.JudgeHaveNoReceive();
			enterBtn.gameObject.SetActive(active);
		}

		protected override void ClickEnter(MButton btn)
		{
			LogEvent.I.HomeIconClick("StarRank");
			if (StarRankingActivityMgr.I.JudgeHaveNoReceive())
			{
				_EnterActivityRewardDialog();
				return;
			}
			int activityEndTime = StarRankingActivityMgr.I.GetActivityEndTime();
			_activityDialog = FestivalMgr.I.AddTopNode<StarRankingActivityDialog>("Prefabs/Activity/StarRanking/{0}_StarRankingActivityDialog");
			_activityDialog.closeCallback = delegate
			{
				_activityDialog = null;
			};
			_activityDialog.Show(activityEndTime);
		}

		private void _CountdownEnd()
		{
			if (_activityDialog != null)
			{
				_activityDialog.CloseBtn(null);
				_EnterActivityRewardDialog();
			}
		}

		private void _EnterActivityRewardDialog()
		{
			StarRankingActivityRewardDialog starRankingActivityRewardDialog = FestivalMgr.I.AddTopNode<StarRankingActivityRewardDialog>("Prefabs/Activity/StarRanking/{0}_StarRankingActivityRewardDialog");
			starRankingActivityRewardDialog.closeCallback = delegate
			{
				Init(Cfg);
			};
			starRankingActivityRewardDialog.Show();
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off("StarRankingActivity_Countdown_End", _CountdownEnd);
		}
	}
}
