using GameScene;

namespace GameScene_Element
{
	public class RefreshElement : Element
	{
		public override void ExecuteDestroyEffect()
		{
			if (DataCfg.cfgSpecialModelFuntions.TryGetValue((int)Type, out var funtion))
			{
				int[] one = funtion.FuntionValueOne;
				if (one != null && one.Length == 1 && one[0] > 0)
				{
					GameMgr.I.RefreshControl.Use();
				}
			}
		}
	}
}
