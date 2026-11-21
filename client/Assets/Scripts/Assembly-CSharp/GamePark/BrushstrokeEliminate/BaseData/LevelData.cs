using System.Collections.Generic;

namespace GamePark.BrushstrokeEliminate.BaseData
{
	public class LevelData
	{
		private readonly List<ShapeData> _shapes;

		private readonly List<TargetData> _targets;

		private int _curLayerIndex;

		public int CurLayerIndex => _curLayerIndex;

		public List<TargetData> TargetDatas => _targets;

		public LevelData()
		{
			_shapes = new List<ShapeData>();
			_targets = new List<TargetData>();
		}

		public void CreateLevel()
		{
			_shapes.Clear();
			_targets.Clear();
			List<int[]> list = new List<int[]>();
			list.AddRange(DataMgr.I.cfg.TargetId);
			int num = 6;
			int shapeIndex = 6;
			while (true)
			{
				shapeIndex += MMath.Random(0, 4);
				int maxShape = DataMgr.I.cfg.ShapeCount[shapeIndex.ToString()];
				int randomShapeSubIndex = MMath.Random(1, maxShape);
				int[] targetIds = list.PopRFast();
				TargetData targetData = new TargetData();
				targetData.Index = _targets.Count;
				targetData.ModelId = targetIds[0];
				targetData.Count = shapeIndex;
				_targets.Add(targetData);

				string path = string.Format("BrushstrokeEliminate/LevelShapes/Shape{0}_{1}", shapeIndex, randomShapeSubIndex);
				ShapeData shapeData = LoadMgr.LoadJson<ShapeData>(path);
				for (int i = shapeData.Data.Length - 1; i >= 0; i--)
				{
					if (shapeData.Data[i] == 1)
					{
						shapeData.Data[i] = targetIds[0];
					}
					else
					{
						shapeData.Data[i] = targetIds[MMath.Random(1, targetIds.Length)];
					}
				}
				_shapes.Add(shapeData);
				num--;
				if (num == 0)
				{
					_curLayerIndex = -1;
					NextLayer();
					break;
				}
			}
		}

		public bool NextLayer()
		{
			_curLayerIndex++;
			if (_curLayerIndex < _shapes.Count)
			{
				ElementMgr.I.SetShapeCfg(_shapes[_curLayerIndex]);
			}
			return _curLayerIndex >= _shapes.Count;
		}

		public TargetData GetTarget()
		{
			return _targets[_curLayerIndex];
		}
	}
}
