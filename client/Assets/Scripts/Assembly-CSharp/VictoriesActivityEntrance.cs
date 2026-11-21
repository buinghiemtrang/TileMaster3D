using DG.Tweening;
using Festival;
using FunctionEject;

public class VictoriesActivityEntrance : EjectItem
{
	private int _surTime;

	private Tweener _timeTw;

	public override void Init()
	{
		VictoriesActivityState victoriesActivityState = VictoriesActivityMgr.I.RefreshData();
		TweenMgr.OwnKill(_timeTw);
		if (victoriesActivityState == VictoriesActivityState.During)
		{
			_Countdown(false);
		}
		else if (victoriesActivityState == VictoriesActivityState.BeforeStarting)
		{
			_Countdown(true);
		}
	}

	public override bool Eject()
	{
		switch (VictoriesActivityMgr.I.JudgeAutoEject())
		{
		case 0:
			return false;
		case 1:
			_EnterNoticeDialog();
			return true;
		case 2:
		case 3:
			_EnterActivityDialog();
			return true;
		default:
			return true;
		}
	}

	public override void ForceEject()
	{
		_EnterActivityDialog();
	}

	private void _EnterNoticeDialog()
	{
		VictoriesActivityStartNoticeDialog victoriesActivityStartNoticeDialog = Singleton<FestivalMgr>.I.AddTopNode<VictoriesActivityStartNoticeDialog>("Prefabs/Activity/Victories/{0}/VictoriesActivityStartNoticeDialog");
		victoriesActivityStartNoticeDialog.closedCallback = delegate
		{
			End(EjectType.Continue);
		};
		victoriesActivityStartNoticeDialog.Show("Initiative", 0);
	}

	private void _EnterActivityDialog()
	{
		VictoriesActivityDialog victoriesActivityDialog = Singleton<FestivalMgr>.I.AddTopNode<VictoriesActivityDialog>("Prefabs/Activity/Victories/{0}/VictoriesActivityDialog");
		victoriesActivityDialog.closedCallback = delegate(int state)
		{
			Init();
			End((state != 3) ? EjectType.Continue : EjectType.Break);
		};
		victoriesActivityDialog.Show("Initiative", _surTime);
	}

	private void _Countdown(bool isNotice)
	{
		_surTime = VictoriesActivityMgr.I.GetActivityStartOrEndTime(!isNotice);
		_timeTw = TweenMgr.NumberTo((float)_surTime, _surTime, 0, delegate(int x)
		{
			_surTime = x;
			DispatcherMgr.Emit("VictoriesActivity_TimeChange", _surTime);
		}).SetEase(Ease.Linear).SetId("VictoriesActivityEntrance_Time").OnComplete(Init);
	}

	private void OnDestroy()
	{
		TweenMgr.OwnKill(_timeTw);
	}
}
