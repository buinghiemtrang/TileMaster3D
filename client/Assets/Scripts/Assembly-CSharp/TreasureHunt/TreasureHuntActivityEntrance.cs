using DG.Tweening;
using Festival;
using FunctionEject;

namespace TreasureHunt
{
	public class TreasureHuntActivityEntrance : EjectItem
	{
		private Tweener _timeTw;

		private int _surTime;

		private void Awake()
		{
			DispatcherMgr.On<int>("TreasureHuntActivity_GetServerTime", _Countdown);
		}

		public override void Init()
		{
			TreasureHuntHttpMgr.I.GetActivity();
			_Countdown(-1);
		}

		public override bool Eject()
		{
			int num = Singleton<TreasureHuntActivityMgr>.I.JudgeAutoEject();
			if (num == 0)
			{
				return false;
			}
			if (num == 1)
			{
				_EnterPosterDialog();
			}
			else
			{
				_EnterActivityDialog();
			}
			return true;
		}

		private void _EnterPosterDialog()
		{
			TreasureHuntActivityPosterDialog treasureHuntActivityPosterDialog = Singleton<FestivalMgr>.I.AddTopNode<TreasureHuntActivityPosterDialog>("Prefabs/Activity/TreasureHunt/{0}/TreasureHuntActivityPosterDialog");
			treasureHuntActivityPosterDialog.closeCallback = delegate(int type)
			{
				End((type == 1) ? EjectType.Continue : EjectType.Break);
			};
			treasureHuntActivityPosterDialog.Show("Initiative", _surTime);
		}

		private void _EnterActivityDialog()
		{
			TreasureHuntActivityDialog treasureHuntActivityDialog = Singleton<FestivalMgr>.I.AddTopNode<TreasureHuntActivityDialog>("Prefabs/Activity/TreasureHunt/{0}/TreasureHuntActivityDialog");
			treasureHuntActivityDialog.closeCallback = delegate(int type)
			{
				if (type == 1)
				{
					TreasureHuntHttpMgr.I.GetActivity();
				}
				End(EjectType.Continue);
			};
			treasureHuntActivityDialog.Show("Initiative", _surTime);
		}

		private void _Countdown(int second = -1)
		{
			TweenMgr.OwnKill(_timeTw);
			if (second == -1)
			{
				second = Singleton<TreasureHuntActivityMgr>.I.GetEndTime();
			}
			_surTime = second;
			if (second > 0)
			{
				_timeTw = TweenMgr.NumberTo((float)second, second, 0, delegate(int val)
				{
					_surTime = val;
					_SendTimeChangeEvent();
				}).SetEase(Ease.Linear).SetId("TreasureHuntActivityEntrance_Time").OnComplete(TreasureHuntHttpMgr.I.GetActivity);
			}
			else
			{
				_SendTimeChangeEvent();
			}
		}

		private void _SendTimeChangeEvent()
		{
			DispatcherMgr.Emit("TreasureHuntActivity_TimeChange", _surTime);
		}

		private void OnDestroy()
		{
			TweenMgr.OwnKill(_timeTw);
			DispatcherMgr.Off<int>("TreasureHuntActivity_GetServerTime", _Countdown);
		}
	}
}
