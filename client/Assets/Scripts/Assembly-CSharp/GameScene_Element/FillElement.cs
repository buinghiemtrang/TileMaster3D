using GameScene;

namespace GameScene_Element
{
	public class FillElement : Element
	{
		public override void ExecuteDestroyEffect()
		{
			if (DataCfg.cfgSpecialModelFuntions.TryGetValue((int)Type, out var funtion))
			{
				if (funtion.FuntionValueOne != null || funtion.FuntionValueTwo != null)
				{
					GameMgr.I.FillControl.Use(funtion.FuntionValueOne);
					GameMgr.I.FillControl.Use(funtion.FuntionValueTwo);
				}
			}
		}
	}
}
