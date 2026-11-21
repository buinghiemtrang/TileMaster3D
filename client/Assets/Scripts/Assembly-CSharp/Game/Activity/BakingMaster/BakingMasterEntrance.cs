using System;
using ActivityEntrance;
using UnityEngine.UI;

namespace Game.Activity.BakingMaster
{
	public class BakingMasterEntrance : EntranceItem
	{
		public Text testDes;

		public override void Init(cfgActivityEntranceInformation cfg)
		{
			base.Init(cfg);
			testDes.gameObject.SetActive(false);
			if (!DataManager.I.bLogined)
			{
				base.gameObject.SetActive(false);
				return;
			}
			lvTex.text = string.Format("Lv.{0}", cf_const.activityBakingMasterUnlockLv);
			bool flag = DB.User.Maxlv + 1 > cf_const.activityBakingMasterUnlockLv;
			lockObj.SetActive(!flag);
			if (flag)
			{
				SetDesc(cfg.ordinaryInfo, cfg.defaultOrdinaryInfo);
			}
			else
			{
				SetDesc(cfg.lockInfo, cfg.defaultLockInfo);
			}
			enterBtn.gameObject.SetActive(false); // [rebuild] offset 0x20 = enterBtn (trước dịch nhầm descTex 0x28)
			if (!flag)
			{
				return;
			}
			RequestIssueData info = DB.BakingMasterActivity.Info;
			if (info != null && info.EndTime != default(DateTime) && info.StartTime != default(DateTime) && !(DateTime.Now < info.StartTime) && !(DateTime.Now > info.EndTime))
			{
				enterBtn.gameObject.SetActive(true);
			}
		}

		protected override void ClickEnter(MButton btn)
		{
			LogEvent.I.HomeIconClick("BakingMaster");
			if (!DB.BakingMasterActivity.IsTeamUser())
			{
				BakingMasterActivityPosterDialog.Create(true);
			}
			else
			{
				BakingMasterActivityRootDialog.Create();
				DispatcherMgr.Emit("ActivityEntrance_ActivityEnterOtherPage");
			}
		}
	}
}
