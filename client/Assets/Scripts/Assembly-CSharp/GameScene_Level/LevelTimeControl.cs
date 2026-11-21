namespace GameScene_Level
{
	public class LevelTimeControl
	{
		public void AddTime(cfgLevelFixed config, LevelData data, cfgLevelDifficulty difficultConfig)
		{
			if (config == null || config.Time == -1)
			{
				float num = ((data.OrdLevelType == OrdinaryLevelType.Normal) ? difficultConfig.NormalTimeK : difficultConfig.RapidlyTimeK);
				data.Time = (int)(num * (float)(data.Models.Count * 3));
			}
			else
			{
				data.Time = config.Time;
			}
		}
	}
}
