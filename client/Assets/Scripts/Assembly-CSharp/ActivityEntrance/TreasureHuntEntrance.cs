using Festival;
using TreasureHunt;
using UnityEngine;

namespace ActivityEntrance
{
	public class TreasureHuntEntrance : EntranceItem
	{
		public override void Init(cfgActivityEntranceInformation cfg)
		{
			base.Init(cfg);
			lvTex.text = string.Format("Lv.{0}", cf_const.treasureHuntActivityUnlockLv);
			bool flag = TreasureHuntActivityMgr.I.JudgeUnlock();
			lockObj.SetActive(!flag);
			if (flag)
			{
				SetDesc(cfg.ordinaryInfo, cfg.defaultOrdinaryInfo);
			}
			else
			{
				SetDesc(cfg.lockInfo, cfg.defaultLockInfo);
			}
			bool flag2 = TreasureHuntActivityMgr.I.JudgeOpen();
			enterBtn.gameObject.SetActive(flag2);
		}

		protected override void ClickEnter(MButton btn)
		{
			LogEvent.I.HomeIconClick("TreasureHunt");
			int endTime = TreasureHuntActivityMgr.I.GetEndTime();
			if (TreasureHuntActivityMgr.I.JudgeHasState(ActivityState.JoinedTeam) && (TreasureHuntActivityMgr.I.JudgeHasState(ActivityState.WaitingEnd) || !TreasureHuntActivityMgr.I.JudgeHasState(ActivityState.PosterDialog)))
			{
				TreasureHuntActivityDialog treasureHuntActivityDialog = FestivalMgr.I.AddTopNode<TreasureHuntActivityDialog>("Prefabs/Activity/TreasureHunt/{0}/TreasureHuntActivityDialog");
				treasureHuntActivityDialog.closeCallback = delegate
				{
					Init(Cfg);
				};
				treasureHuntActivityDialog.Show("Click", endTime);
			}
			else
			{
				TreasureHuntActivityPosterDialog treasureHuntActivityPosterDialog = FestivalMgr.I.AddTopNode<TreasureHuntActivityPosterDialog>("Prefabs/Activity/TreasureHunt/{0}/TreasureHuntActivityPosterDialog");
				treasureHuntActivityPosterDialog.closeCallback = delegate(int state)
				{
					if (state == 1)
					{
						Init(Cfg);
					}
					else
					{
						DispatcherMgr.Emit("ActivityEntrance_ActivityEnterOtherPage");
					}
				};
				treasureHuntActivityPosterDialog.Show("Click", endTime);
			}
		}
	}
}
