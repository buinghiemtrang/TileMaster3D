using Festival;
using UnityEngine;

namespace ActivityEntrance
{
	public class VictoriesEntrance : EntranceItem
	{
		private VictoriesActivityState _state;

		public override void Init(cfgActivityEntranceInformation cfg)
		{
			base.Init(cfg);
			lvTex.text = string.Format("Lv.{0}", cf_const.victoriesActivityUnlockLv);
			bool flag = VictoriesActivityMgr.I.JudgeUnlock();
			lockObj.SetActive(!flag);
			if (flag)
			{
				SetDesc(cfg.ordinaryInfo, cfg.defaultOrdinaryInfo);
			}
			else
			{
				SetDesc(cfg.lockInfo, cfg.defaultLockInfo);
			}
			_state = VictoriesActivityMgr.I.GetActivityState();
			bool flag2 = true;
			if (_state == VictoriesActivityState.NoStart)
			{
				flag2 = VictoriesActivityMgr.I.JudgeHaveNoReceive();
			}
			enterBtn.gameObject.SetActive(flag2);
		}

		protected override void ClickEnter(MButton btn)
		{
			LogEvent.I.HomeIconClick("WinningStreak");
			if (_state != VictoriesActivityState.During)
			{
				if (_state == VictoriesActivityState.BeforeStarting)
				{
					VictoriesActivityStartNoticeDialog victoriesActivityStartNoticeDialog = FestivalMgr.I.AddTopNode<VictoriesActivityStartNoticeDialog>("Prefabs/Activity/Victories/{0}/VictoriesActivityStartNoticeDialog");
					victoriesActivityStartNoticeDialog.closedCallback = delegate
					{
						Init(Cfg);
					};
					victoriesActivityStartNoticeDialog.Show("Click", VictoriesActivityMgr.I.GetActivityStartOrEndTime(false));
					return;
				}
				if (!VictoriesActivityMgr.I.JudgeHaveNoReceive())
				{
					return;
				}
			}
			VictoriesActivityDialog victoriesActivityDialog = FestivalMgr.I.AddTopNode<VictoriesActivityDialog>("Prefabs/Activity/Victories/{0}/VictoriesActivityDialog");
			victoriesActivityDialog.closedCallback = delegate(int state)
			{
				if (state != 3)
				{
					Init(Cfg);
				}
				else
				{
					DispatcherMgr.Emit("ActivityEntrance_ActivityEnterOtherPage");
				}
			};
			victoriesActivityDialog.Show("Click", VictoriesActivityMgr.I.GetActivityStartOrEndTime(true));
		}
	}
}
