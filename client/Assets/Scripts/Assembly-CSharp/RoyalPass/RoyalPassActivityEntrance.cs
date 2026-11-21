using DG.Tweening;
using Festival;
using FunctionEject;
using UnityEngine;
using UnityEngine.UI;

namespace RoyalPass
{
	public class RoyalPassActivityEntrance : EjectItem
	{
		[SerializeField]
		private Text timeTex;

		[SerializeField]
		private GameObject finishedWordObj;

		[SerializeField]
		private GameObject timeObj;

		private Tweener _timeTw;

		private int _surTime;

		private void Awake()
		{
			base.gameObject.SetActive(false);
			DispatcherMgr.On<int>("RoyalPasHttp_GetServerTime", _ShowEndTime);
			DispatcherMgr.On("RoyalPasHttp_GetActivity_Succ", _Refresh);
		}

		public override void Init()
		{
			RoyalPassHttpMgr.I.GetActivity();
			if (!RoyalPassActivityMgr.I.JudgeHasState(ActivityState.WaitingReceivePaidReward))
			{
				RoyalPassHttpMgr.I.SendTeamReward();
			}
			_ShowEntrance();
			_ShowEndTime();
		}

		public override bool Eject()
		{
			int num = RoyalPassActivityMgr.I.JudgeAutoEject();
			if (num != 0)
			{
				_EnterActivityDialog("Initiative");
			}
			return num != 0;
		}

		public override void ForceEject()
		{
			_EnterActivityDialog("Initiative");
		}

		public void ClickEntrance(MButton btn)
		{
			_EnterActivityDialog("Click");
			LogEvent.I.HomeIconClick("MaterPass");
		}

		private void _EnterActivityDialog(string openType)
		{
			RoyalPassActivityMgr.I.ShowDialog(ActivityState.ActivityDialog);
			RoyalPassActivityDialog royalPassActivityDialog = FestivalMgr.I.AddTopNode<RoyalPassActivityDialog>("Prefabs/Activity/RoyalPass/{0}_RoyalPassActivityDialog");
			royalPassActivityDialog.closeCallback = delegate
			{
				End(EjectType.Continue);
			};
			royalPassActivityDialog.Show(openType, _surTime);
		}

		private void _ShowEntrance()
		{
			base.gameObject.SetActive(RoyalPassActivityMgr.I.JudgeOpen());
		}

		private void _Refresh()
		{
			_ShowEntrance();
		}

		private void _ShowEndTime(int second = -1)
		{
			_timeTw.OwnKill();
			if (second == -1)
			{
				second = RoyalPassActivityMgr.I.GetEndTime();
			}
			_surTime = second;
			_ActivityEnd(second == 0);
			if (second != 0)
			{
				_timeTw = TweenMgr.NumberTo(second, second, 0, delegate(int va)
				{
					_surTime = va;
					timeTex.text = Util.Helper.FormatTimer(va, "hh:mm:ss");
					_SendTimeChangeEvent();
				}).SetEase(Ease.Linear).SetId("RoyalPassActivityEntrance_EndTime").OnComplete(delegate
				{
					RoyalPassHttpMgr.I.GetActivity();
					_ActivityEnd(true);
				});
			}
			else
			{
				_SendTimeChangeEvent();
			}
		}

		private void _SendTimeChangeEvent()
		{
			DispatcherMgr.Emit("RoyalPassActivity_TimeChange", _surTime);
		}

		private void _ActivityEnd(bool isEnd)
		{
			if (finishedWordObj.activeSelf != isEnd)
			{
				finishedWordObj.SetActive(isEnd);
			}
			if (timeObj.activeSelf == isEnd)
			{
				timeObj.SetActive(!isEnd);
			}
		}

		private void OnDestroy()
		{
			_timeTw.OwnKill();
			DispatcherMgr.Off<int>("RoyalPasHttp_GetServerTime", _ShowEndTime);
			DispatcherMgr.Off("RoyalPasHttp_GetActivity_Succ", _Refresh);
		}
	}
}
