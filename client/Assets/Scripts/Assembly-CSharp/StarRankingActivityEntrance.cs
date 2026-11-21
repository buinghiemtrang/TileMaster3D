using DG.Tweening;
using FunctionEject;

public class StarRankingActivityEntrance : EjectItem
{
	private int _surTime;

	private Tweener _timeTw;

	private StarRankingActivityState _state;

	private StarRankingActivityDialog _activityDialog;

	public override void Init()
	{
		_state = Singleton<StarRankingActivityMgr>.I.GetActivityState();
		TweenMgr.OwnKill(_timeTw);
		if (_state == StarRankingActivityState.During)
		{
			_Countdown();
		}
	}

	public override bool Eject()
	{
		int num = Singleton<StarRankingActivityMgr>.I.JudgeAutoEject();
		if (num == 0)
		{
			return false;
		}
		if (num == 1)
		{
			_EnterActivityDialog();
		}
		else
		{
			_EnterActivityRewardDialog();
		}
		return true;
	}

	private void _EnterActivityDialog()
	{
		_activityDialog = Singleton<Festival.FestivalMgr>.I.AddTopNode<StarRankingActivityDialog>("Prefabs/Activity/StarRanking/{0}_StarRankingActivityDialog");
		_activityDialog.closeCallback = delegate(bool isForce)
		{
			_activityDialog = null;
			if (!isForce)
			{
				End(EjectType.Continue);
			}
		};
		_activityDialog.Show(_surTime);
	}

	private void _EnterActivityRewardDialog()
	{
		StarRankingActivityRewardDialog starRankingActivityRewardDialog = Singleton<Festival.FestivalMgr>.I.AddTopNode<StarRankingActivityRewardDialog>("Prefabs/Activity/StarRanking/{0}_StarRankingActivityRewardDialog");
		starRankingActivityRewardDialog.closeCallback = delegate
		{
			Init();
			End(EjectType.Continue);
		};
		starRankingActivityRewardDialog.Show();
	}

	private void _Countdown()
	{
		_surTime = Singleton<StarRankingActivityMgr>.I.GetActivityEndTime();
		_timeTw = TweenMgr.NumberTo((float)_surTime, _surTime, 0, delegate(int value)
		{
			_surTime = value;
			_SendTimeChangeEvent();
		}).SetEase(Ease.Linear).SetId("StarRankingActivityEntrance_Time").OnComplete(delegate
		{
			Init();
			DispatcherMgr.Emit("StarRankingActivity_Countdown_End");
			if (_activityDialog == null)
			{
				return;
			}
			_activityDialog.CloseBtn(null);
			_EnterActivityRewardDialog();
		});
	}

	private void _SendTimeChangeEvent()
	{
		DispatcherMgr.Emit("StarRankingActivity_TimeChange", _surTime);
	}

	private void OnDestroy()
	{
		TweenMgr.OwnKill(_timeTw);
	}
}
