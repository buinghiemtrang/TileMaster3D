using GameScene;

namespace GameScene_Function
{
	public class FreezeControl : FunctionBase
	{
		private readonly FreezeView _view;

		private bool _isExecuting;

		public FreezeControl(FreezeView view)
		{
			_view = view;
		}

		public override void Init()
		{
			_view.Init();
			_isExecuting = false;
		}

		public bool JudgeCanUse()
		{
			return !_isExecuting;
		}

		public void Use(int value)
		{
			_view.Countdown(value, _EndFreeze);
			GameMgr.I.propControl.pPropFreeze.SetCoolDown(value);
			OperationTip.I.CancelFreezeTip();
			if (!_isExecuting)
			{
				_view.StartFreeze();
				DispatcherMgr.Emit("GameScene_Freeze_Start");
			}
			_isExecuting = true;
		}

		private void _EndFreeze()
		{
			_isExecuting = false;
			_view.EndFreeze(true);
			DispatcherMgr.Emit("GameScene_Freeze_End");
		}
	}
}
