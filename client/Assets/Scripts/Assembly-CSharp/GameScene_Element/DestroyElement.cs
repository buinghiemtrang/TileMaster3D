using GameScene;

namespace GameScene_Element
{
	public class DestroyElement : Element
	{
		public override void ExecuteDestroyEffect()
		{
			if (DataCfg.cfgSpecialModelFuntions.TryGetValue((int)Type, out var funtion))
			{
				int[] one = funtion.FuntionValueOne;
				if (one != null && one.Length == 2)
				{
					int number = UnityEngine.Random.Range(one[0], one[1] + 1);
					if (number > 0)
					{
						GameMgr.I.DestroyControl.Use(GameMgr.I.DestroyControl.JudgeCanUse(number));
					}
				}
			}
		}
	}
}
