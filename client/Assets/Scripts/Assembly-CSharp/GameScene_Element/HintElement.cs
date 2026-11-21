using GameScene;

namespace GameScene_Element
{
	public class HintElement : Element
	{
		public override void ExecuteDestroyEffect()
		{
			if (DataCfg.cfgSpecialModelFuntions.TryGetValue((int)Type, out var funtion))
			{
				int[] one = funtion.FuntionValueOne;
				if (one != null && one.Length == 1)
				{
					for (int i = one[0]; i > 0; i--)
					{
						if (!GameMgr.I.RandomHintControl.JudgeCanUse())
						{
							return;
						}
						GameMgr.I.RandomHintControl.Use();
					}
				}
			}
		}
	}
}
