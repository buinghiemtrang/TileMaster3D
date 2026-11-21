using Newtonsoft.Json;

namespace DataCenter
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Chest : IChest
	{
		public void AddStar(int num, bool isDirect = false)
		{
			if (isDirect)
			{
				Num = num;
				return;
			}
			for (int i = 0; i < DataCfg.cfgModelChests.Count; i++)
			{
				if (Id == DataCfg.cfgModelChests[i].id)
				{
					if (Num < DataCfg.cfgModelChests[i].completedNum)
					{
						if (Num + num <= DataCfg.cfgModelChests[i].completedNum)
						{
							Num += num;
						}
						else
						{
							Num = DataCfg.cfgModelChests[i].completedNum;
						}
					}
					return;
				}
			}
		}
	}
}
