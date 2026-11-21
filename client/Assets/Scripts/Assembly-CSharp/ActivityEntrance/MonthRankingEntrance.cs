using MonthRanking;
using Festival;

namespace ActivityEntrance
{
	public class MonthRankingEntrance : EntranceItem
	{
		public override void Init(cfgActivityEntranceInformation cfg)
		{
			base.Init(cfg);
			lockObj.SetActive(false);
			SetDesc(cfg.ordinaryInfo, cfg.defaultOrdinaryInfo);
			enterBtn.gameObject.SetActive(MonthRankingActivityMgr.I.JudgeOpen());
		}

		protected override void ClickEnter(MButton btn)
		{
			LogEvent.I.HomeIconClick("MonthRanking");
			int localEndTimeSpan = MonthRankingActivityMgr.I.GetLocalEndTimeSpan();
			if (DB.MonthRankingActivity.State == 2 && !DB.MonthRankingActivity.IsAutoEjected)
			{
				MonthRankingActivityPosterDialog monthRankingActivityPosterDialog = FestivalMgr.I.AddTopNode<MonthRankingActivityPosterDialog>("Prefabs/Activity/MonthRanking/{0}/MonthRankingActivityPosterDialog");
				monthRankingActivityPosterDialog.closeCallback = delegate(bool isEnterGame)
				{
					if (!isEnterGame)
					{
						Init(Cfg);
					}
					else
					{
						DispatcherMgr.Emit("ActivityEntrance_ActivityEnterOtherPage");
					}
				};
				monthRankingActivityPosterDialog.Show("Click", (int)localEndTimeSpan);
				return;
			}
			MonthRankingActivityDialog monthRankingActivityDialog = FestivalMgr.I.AddTopNode<MonthRankingActivityDialog>("Prefabs/Activity/MonthRanking/MonthRankingActivityDialog");
			monthRankingActivityDialog.closeCallback = delegate
			{
				Init(Cfg);
			};
			monthRankingActivityDialog.Show("Click", (int)localEndTimeSpan);
		}
	}
}
