namespace GameScene_Level
{
	public class BakingMasterLevel : LevelData
	{
		private readonly LevelTimeControl _levelTimeControl = new LevelTimeControl();

		private readonly WaveControl _waveControl = new WaveControl();

		public BakingMasterLevel()
		{
			Difficulty.Init(this);
		}

		public override void RestoreRecords()
		{
		}

		public override void Init(int lv, cfgLevelFixed config)
		{
			Lv = lv;
			Type = LevelType.BakingMaster;
			OrdLevelType = OrdinaryLevelType.None;
			Models.Clear();
			Wave.Clear();
			ModelConfigs.Clear();
			RemainingModels.Clear();
			if (config != null)
			{
				AddUnlockedModels(Models, config.Models);
				AddModelsByModelGroupConfig(Models, config.RandomModels);
				cfgLevelDifficulty config2 = Difficulty.DynamicDifficulty.GetConfig();
				_levelTimeControl.AddTime(config, this, config2);
				_waveControl.WaveDivision(this, isHas: false);
				AddModelConfigs(ModelConfigs, Models);
			}
		}
	}
}
