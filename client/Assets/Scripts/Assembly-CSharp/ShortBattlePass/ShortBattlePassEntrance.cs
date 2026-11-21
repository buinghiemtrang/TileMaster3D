using DG.Tweening;
using FunctionEject;
using UnityEngine;
using UnityEngine.UI;

namespace ShortBattlePass
{
	public class ShortBattlePassEntrance : EjectItem
	{
		[SerializeField]
		private Text timeTex;

		[SerializeField]
		private GameObject finishedWordObj;

		[SerializeField]
		private GameObject timeObj;

		[SerializeField]
		private GameObject redObj;

		private int _surTime;

		private void Awake()
		{
			base.gameObject.SetActive(false);
			DispatcherMgr.On("ShortBattlePass_Received_Reward", _RefreshRed);
		}

		private void OnDestroy()
		{
			timeTex.DOKill();
			DispatcherMgr.Off("ShortBattlePass_Received_Reward", _RefreshRed);
		}

		public override void Init()
		{
			Singleton<ShortBattlePassMgr>.I.Init();
			if (!Singleton<ShortBattlePassMgr>.I.JudgeHasState(ActivityState.SentRewards))
			{
				Singleton<ShortBattlePassMgr>.I.SendTeamReward();
			}
			_ShowEntrance();
			_ShowEndTime();
		}

		public override bool Eject()
		{
			int num = Singleton<ShortBattlePassMgr>.I.JudgeAutoEject();
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

		public void ClickEntrance(MButton button)
		{
			_EnterActivityDialog("Click");
			LogEvent.I.HomeIconClick("LittleMasterPass");
		}

		private void _RefreshRed()
		{
			redObj.SetActive(Singleton<ShortBattlePassMgr>.I.JudgeHaveCanReceiveRewards());
		}

		private void _EnterActivityDialog(string openType)
		{
			Singleton<ShortBattlePassMgr>.I.ShowDialog();
			ShortBattlePassDialog shortBattlePassDialog = ShortBattlePassDialog.Create(openType, _surTime);
			shortBattlePassDialog.CloseCallback = delegate
			{
				End(EjectType.Continue);
			};
		}

		private void _ShowEntrance()
		{
			bool flag = Singleton<ShortBattlePassMgr>.I.JudgeHasState(ActivityState.Started);
			base.gameObject.SetActive(flag);
			if (flag)
			{
				_RefreshRed();
			}
		}

		private void _ShowEndTime()
		{
			timeTex.DOKill();
			int endTime = Singleton<ShortBattlePassMgr>.I.GetEndTime();
			_surTime = endTime;
			_ActivityEnd(endTime == 0);
			if (endTime != 0)
			{
				timeTex.DOCounter(endTime, 0, endTime, delegate(int value)
				{
					_surTime = value;
					timeTex.text = Util.Helper.FormatTimer(value, "hh:mm:ss");
					DispatcherMgr.Emit("ShortBattlePass_Time_Change", value);
				}).OnComplete(delegate
				{
					_ActivityEnd(isEnd: true);
				});
			}
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

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				ShortBattlePassDialog.Create("", 300);
			}
		}
	}
}
