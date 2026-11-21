using GamePark.BrushstrokeEliminate.BaseData;
using GameScene;
using UnityEngine;

namespace GamePark.BrushstrokeEliminate
{
	public class DataMgr : Singleton<DataMgr>
	{
		public static readonly int UnitLayerID;

		public readonly LevelData Level;

		public readonly Configuration cfg;

		public GameBrushstrokeScene pMain;

		public GameStatus kGameStatus;

		public bool IsCanPause;

		private const int CfgScore = 1;

		static DataMgr()
		{
			UnitLayerID = LayerMask.NameToLayer("3D");
		}

		public DataMgr()
		{
			Level = new LevelData();
			cfg = LoadMgr.LoadJson<Configuration>("BrushstrokeEliminate/Configuration");
			IsCanPause = true;
		}

		public void ModifyScore(int count)
		{
			pMain.pScore.ModifyScore(count);
		}

		public void ModifyScoreShow(Vector3 startPos)
		{
			pMain.pScore.ModifyScoreShow(1, startPos);
		}
	}
}
