using System.Collections.Generic;

namespace GameScene_Record
{
	public class RecordData
	{
		public int StarNumber;

		public int ResidueTime;

		public int ElementTotalNumber;

		public bool IsRegionRankLevel;

		public int RegionRankScore;

		public bool EnableDynamicDifficulty = true;

		public readonly List<int> PlatformData = new List<int>();

		public readonly List<List<int>> WaveData = new List<List<int>>();

		public readonly List<RecordElement> DisplayedData = new List<RecordElement>();
	}
}
