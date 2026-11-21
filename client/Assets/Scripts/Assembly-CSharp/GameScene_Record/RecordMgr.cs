using System.Collections.Generic;
using GameScene;
using GameScene_Element;
using GameScene_Level;
using Newtonsoft.Json;
using UnityEngine;

namespace GameScene_Record
{
	public class RecordMgr : Singleton<RecordMgr>
	{
		private const string KeyFixedDataKey = "GameScene.Record.FixedData";

		private const string KeyLevelDataKey = "GameScene.Record.LevelData";

		public RecordData Data;

		public RecordFixed Fixed;

		public bool IsLoading;

		public void LoadStart()
		{
			IsLoading = true;
			if (!JudgeHasRecord())
			{
				Clear();
				IsLoading = false;
			}
		}

		public void LoadEnd()
		{
			IsLoading = false;
		}

		public bool JudgeHasRecord()
		{
			if (!cf_const.IsOpenLevelRecord)
			{
				return false;
			}
			if (DB.User.Maxlv == 0)
			{
				return false;
			}
			Fixed = StorageMgr.GetObject<RecordFixed>("GameScene.Record.FixedData", null);
			if (Fixed == null)
			{
				return false;
			}
			if (Fixed.Lv != DB.User.Maxlv + 1)
			{
				return false;
			}
			if (!Fixed.Version.Equals(Application.version))
			{
				return false;
			}
			Data = StorageMgr.GetObject<RecordData>("GameScene.Record.LevelData", null);
			return Data != null;
		}

		public void Clear(bool isForce = false)
		{
			if (!isForce && LevelMgr.Data.Type != LevelType.Ordinary)
			{
				return;
			}
			StorageMgr.DelKey("GameScene.Record.FixedData");
			Fixed = null;
			StorageMgr.DelKey("GameScene.Record.LevelData");
			Data = null;
		}

		public void SaveFixed()
		{
			if (LevelMgr.Data.Type == LevelType.Ordinary && !IsLoading)
			{
				if (Fixed == null)
				{
					Fixed = new RecordFixed();
				}
				Fixed.Version = Application.version;
				Fixed.Lv = LevelMgr.Data.Lv;
				Fixed.Time = LevelMgr.Data.Time;
				Fixed.LevelType = (int)LevelMgr.Data.Type;
				Fixed.OrdinaryLevelType = (int)LevelMgr.Data.OrdLevelType;
				Fixed.Models = LevelMgr.Data.Models;
				Fixed.Wave = LevelMgr.Data.Wave;
				Fixed.RemainingModels = LevelMgr.Data.RemainingModels;
				StorageMgr.SetObject("GameScene.Record.FixedData", Fixed);
			}
		}

		public void SaveData()
		{
			if (LevelMgr.Data.Type == LevelType.Ordinary && !IsLoading && (bool)GameMgr.I && (bool)ElementsMgr.I && ElementsMgr.I.Data != null && (bool)PlatformMgr.I && PlatformMgr.I.points != null)
			{
				if (Data == null)
				{
					Data = new RecordData();
				}
				Data.ResidueTime = GameMgr.I.timeControl.ResidueTime;
				Data.StarNumber = DB.User.SumStarLevel + GameMgr.I.comboControl.animNumber;
				Data.ElementTotalNumber = ElementsMgr.I.Data.TotalNumber;
				Data.IsRegionRankLevel = LevelMgr.Data.IsRegionRankLevel;
				Data.RegionRankScore = LevelMgr.Data.RegionRankScore;
				Data.EnableDynamicDifficulty = LevelMgr.Data.EnableDynamicDifficulty;
				_RecordDisplayedElements();
				_RecordWaveElements();
				_RecordPlatformData();
				StorageMgr.SetObject("GameScene.Record.LevelData", Data);
			}
		}

		private void _RecordPlatformData()
		{
			List<PlacePoint> points = PlatformMgr.I.points;
			Data.PlatformData.Clear();
			for (int i = 0; i < points.Count; i++)
			{
				PlacePoint placePoint = points[i];
				if ((bool)placePoint && !placePoint.IsFree && placePoint.PlacedElement != null)
				{
					Data.PlatformData.Add(placePoint.PlacedElement.Config.id);
				}
			}
		}

		private void _RecordDisplayedElements()
		{
			Dictionary<int, List<Element>> displayedElementsId = ElementsMgr.I.Data.DisplayedElementsId;
			Data.DisplayedData.Clear();
			foreach (KeyValuePair<int, List<Element>> item in displayedElementsId)
			{
				List<Element> value = item.Value;
				for (int i = 0; i < value.Count; i++)
				{
					Element element = value[i];
					RecordElement recordElement = new RecordElement();
					recordElement.Id = element.Config.id;
					recordElement.IsPlaced = (element.IsPlaced ? 1 : 0);
					recordElement.Position = element.Self.position;
					recordElement.Angle = element.Self.localEulerAngles;
					Data.DisplayedData.Add(recordElement);
				}
			}
		}

		private void _RecordWaveElements()
		{
			List<Dictionary<int, List<Element>>> waveElements = ElementsMgr.I.Data.WaveElements;
			Data.WaveData.Clear();
			for (int i = 0; i < waveElements.Count; i++)
			{
				List<int> list = new List<int>();
				foreach (KeyValuePair<int, List<Element>> item in waveElements[i])
				{
					List<Element> value = item.Value;
					for (int j = 0; j < value.Count; j++)
					{
						list.Add(value[j].Config.id);
					}
				}
				Data.WaveData.Add(list);
			}
		}
	}
}
