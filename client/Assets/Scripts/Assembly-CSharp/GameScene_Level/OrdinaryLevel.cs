using GameScene_Record;
using UnityEngine;

namespace GameScene_Level
{
	public class OrdinaryLevel : LevelData
	{
		private readonly LevelTimeControl _levelTimeControl = new LevelTimeControl();

		private readonly RandomLevelControl _randomLevelControl = new RandomLevelControl();

		private readonly WaveControl _waveControl = new WaveControl();

		private cfgLevelFixed _config;

		public OrdinaryLevel()
		{
			Difficulty.Init(this);
		}

		public override void RestoreRecords()
		{
			Models.Clear();
			Wave.Clear();
			ModelConfigs.Clear();
			RemainingModels.Clear();
			RecordFixed fixedData = Singleton<RecordMgr>.I.Fixed;
			Lv = fixedData.Lv;
			Time = fixedData.Time;
			Type = (LevelType)fixedData.LevelType;
			OrdLevelType = (OrdinaryLevelType)fixedData.OrdinaryLevelType;
			Models.AddRange(Singleton<RecordMgr>.I.Fixed.Models);
			Wave.AddRange(Singleton<RecordMgr>.I.Fixed.Wave);
			RemainingModels.AddRange(Singleton<RecordMgr>.I.Fixed.RemainingModels);
			AddModelConfigs(ModelConfigs, Models);
			IsRegionRankLevel = Singleton<RecordMgr>.I.Data.IsRegionRankLevel;
			RegionRankScore = Singleton<RecordMgr>.I.Data.RegionRankScore;
			EnableDynamicDifficulty = Singleton<RecordMgr>.I.Data.EnableDynamicDifficulty;
		}

		public override void Restart()
		{
			RegionRankScore = 0;
			if (_config == null && DataCfg.cfgLevelFixeds.ContainsKey(Lv))
			{
				_config = DataCfg.cfgLevelFixeds[Lv];
			}
			Init(Lv, _config);
		}

		public override void Init(int lv, cfgLevelFixed config)
		{
			Lv = lv;
			Type = LevelType.Ordinary;
			Models.Clear();
			Wave.Clear();
			ModelConfigs.Clear();
			RemainingModels.Clear();
			EnableDynamicDifficulty = true;
			IsRegionRankLevel = false;
			RegionRankScore = 0;
			if (GSingleton<RegionRankActivityManager>.ins.TryGetLevelConfig(out var config2, out var crownModels))
			{
				IsRegionRankLevel = true;
				config = config2;
			}
			cfgLevelDifficulty config3 = Difficulty.DynamicDifficulty.GetConfig();
			_config = config;
			if (config == null)
			{
				OrdLevelType = RandomLevelControl.RandomOrdinaryLevelType(Lv);
				AddModelsEliminateSpecialModels(RemainingModels, _randomLevelControl.AddRandomLevelModels(Models, this));
			}
			else
			{
				OrdLevelType = (config.IsRapidlyLevel ? OrdinaryLevelType.Rapidly : OrdinaryLevelType.Normal);
				AddUnlockedModels(Models, config.Models);
				if (crownModels != null && crownModels.Length != 0)
				{
					AddUnlockedModels(Models, crownModels);
				}
				AddModelsEliminateSpecialModels(RemainingModels, AddModelsByModelGroupConfig(Models, config.RandomModels));
				if (OrdLevelType != OrdinaryLevelType.Rapidly)
				{
					RemoveModelsByProportion(Models, config3.FixedLevelModelTypeProportion);
				}
				AddUnlockedModels(Models, config.SpecialModels);
				EnableDynamicDifficulty = config.EnableDynamicDifficulty;
				Debug.LogError("EnableDynamicDifficulty: " + EnableDynamicDifficulty);
			}
			if (RemainingModels.Count == 0)
			{
				AddModelsEliminateSpecialModels(RemainingModels, _randomLevelControl.GetModelsByRandomLevelConfig(this));
			}
			if (OrdLevelType != OrdinaryLevelType.Rapidly)
			{
				Difficulty.PeriodicityDifficulty.ControlModelNumber(this);
				Difficulty.PayingUserDifficulty.ControlModelNumber(this);
			}
			_levelTimeControl.AddTime(config, this, config3);
			if (OrdLevelType != OrdinaryLevelType.Rapidly)
			{
				Difficulty.PeriodicityDifficulty.ControlTime(this);
			}
			bool isHas = ((config != null) ? config.IsWaveDivision : RandomLevelControl.JudgeWaveDivision(Lv));
			_waveControl.WaveDivision(this, isHas);
			AddModelConfigs(ModelConfigs, Models);
		}
	}
}
