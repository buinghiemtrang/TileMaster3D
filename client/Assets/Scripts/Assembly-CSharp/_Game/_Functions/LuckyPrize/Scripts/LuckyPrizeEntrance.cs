using DG.Tweening;
using FunctionEject;
using UnityEngine;
using UnityEngine.UI;

namespace _Game._Functions.LuckyPrize.Scripts
{
	public class LuckyPrizeEntrance : EjectItem
	{
		[SerializeField]
		private Text _ttTime;

		[SerializeField]
		private MButton _btnEntrance;

		private void Awake()
		{
			Init();
			_btnEntrance.onClick.AddListener(ClickEntrance);
			DispatcherMgr.On("Update_UserData", Init);
		}

		private void OnDestroy()
		{
			_ttTime.DOKill();
			DispatcherMgr.Off("Update_UserData", Init);
		}

		public override void Init()
		{
			if (DB.User.Maxlv < cf_const.LuckyPrizeUnlockLv)
			{
				gameObject.SetActive(false);
				return;
			}
			gameObject.SetActive(true);
			Singleton<LuckyPrizeMgr>.I.CheckRefreshReward();
			int refreshTime = Singleton<LuckyPrizeMgr>.I.GetRefreshTime();
			_ttTime.DOCounter(refreshTime, 0, refreshTime, Util.Helper.FormatTimerD).OnComplete(Init);
		}

		public override bool Eject()
		{
			return false;
		}

		private void ClickEntrance(MButton button)
		{
			LuckyPrizeDialog.Create();
		}
	}
}
