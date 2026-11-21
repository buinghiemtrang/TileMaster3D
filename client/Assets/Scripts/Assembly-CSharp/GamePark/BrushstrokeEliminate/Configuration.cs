using System.Collections.Generic;

namespace GamePark.BrushstrokeEliminate
{
	public class Configuration
	{
		public int TotalTimeFactor;

		public int DeductTime;

		public readonly List<int[]> TargetId;

		public readonly Dictionary<string, int> ShapeCount;

		public Configuration()
		{
			TargetId = new List<int[]>();
			ShapeCount = new Dictionary<string, int>();
		}
	}
}
