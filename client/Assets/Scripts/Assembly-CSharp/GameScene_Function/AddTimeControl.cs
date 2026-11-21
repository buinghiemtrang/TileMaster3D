using GameScene;

namespace GameScene_Function
{
	public class AddTimeControl : FunctionBase
	{
		public override void Init()
		{
		}

		public void Use(int value)
		{
			GameMgr.I.timeControl.AddTimeByAnim(value);
		}
	}
}
