using System;
using System.Collections.Generic;
using System.IO;
using GamePark.BrushstrokeEliminate.BaseData;
using Newtonsoft.Json;
using UnityEngine;

namespace GamePark.BrushstrokeEliminate.EditorShape
{
	[ExecuteInEditMode]
	public class ShapeGizmosControl : MonoBehaviour
	{
		[SerializeField]
		public Vector2Int origin;

		[SerializeField]
		public int count;

		private int _size;

		private const int Interval = 3;

		private Dictionary<int, Func<int, int, int, int, IReadOnlyList<ShapeGizmosOne>, bool[], ShapeGizmosOne>> _funcs;

		public ShapeGizmosControl()
		{
			_funcs = new Dictionary<int, Func<int, int, int, int, IReadOnlyList<ShapeGizmosOne>, bool[], ShapeGizmosOne>>
			{
				{ 0, (x, y, row, col, shapes, signs) => (x < 1 || signs[(x - 1) + row * y]) ? null : shapes[(x - 1) + row * y] },
				{ 1, (x, y, row, col, shapes, signs) => (x >= row - 1 || signs[(x + 1) + row * y]) ? null : shapes[(x + 1) + row * y] },
				{ 2, (x, y, row, col, shapes, signs) => (y < 1 || signs[x + (y - 1) * row]) ? null : shapes[x + (y - 1) * row] },
				{ 3, (x, y, row, col, shapes, signs) => (y >= col - 1 || signs[x + (y + 1) * row]) ? null : shapes[x + (y + 1) * row] }
			};
		}

		public void CreateShape()
		{
			Transform contentTf = transform.Find("Content");
			if (contentTf != null)
			{
				DestroyImmediate(contentTf.gameObject);
			}
			GameObject contentGo = new GameObject("Content");
			contentGo.transform.SetParent(transform, false);
			Rect rect = transform.Rect();
			int num = (int)(rect.width / (float)origin.x);
			int num2 = (int)(rect.height / (float)origin.y);
			_size = Math.Min(100, Math.Min(num, num2));
			int totalWidth = _size * origin.x;
			int maxY = origin.y - 1;
			for (int y = 0; y < origin.y; y++)
			{
				for (int x = 0; x < origin.x; x++)
				{
					GameObject cellGo = new GameObject();
					cellGo.transform.SetParent(contentGo.transform, false);
					ShapeGizmosOne cell = cellGo.AddComponent<ShapeGizmosOne>();
					cell.Init(x + y * origin.x, x, y);
					float posX = (float)(_size - totalWidth) / 2f + (float)(_size * x);
					float posY = (float)(maxY * _size) / 2f - (float)(_size * y);
					cell.transform.SetLocPos(posX, posY);
					cell.SetSize(_size - 3, _size - 3);
				}
			}
		}

		public void CreateLinkPath()
		{
			Transform content = transform.Find("Content");
			if (content != null)
			{
				ShapeGizmosOne[] shapes = content.GetComponentsInChildren<ShapeGizmosOne>();
				int attempts = 1000;
				while (attempts > 0)
				{
					if (_judgeLinkPath(shapes))
					{
						return;
					}
					attempts--;
				}
			}
		}

		private bool _judgeLinkPath(IReadOnlyList<ShapeGizmosOne> shapes)
		{
			foreach (ShapeGizmosOne shape in shapes)
			{
				shape.kType = SlotType.Empty;
			}
			bool[] signs = new bool[shapes.Count];
			int remaining = count;
			int startIndex = MMath.Random(shapes.Count);
			ShapeGizmosOne current = shapes[startIndex];
			if (current != null)
			{
				current.kType = SlotType.Solid;
				signs[current.Index - 1] = true;
				remaining--;
				string pathLog = current.Index.ToString();
				while (true)
				{
					List<int> dirs = new List<int> { 0, 1, 2, 3 };
					int x = current.vLoc.x;
					int y = current.vLoc.y;
					ShapeGizmosOne next = null;
					while (dirs.Count > 0)
					{
						int dir = dirs.PopR();
						next = _funcs[dir](x, y, origin.x, origin.y, shapes, signs);
						if (next != null)
						{
							break;
						}
					}
					if (next == null)
					{
						return false;
					}
					current = next;
					current.kType = SlotType.Solid;
					signs[current.Index - 1] = true;
					pathLog += string.Format(">>{0}", current.Index);
					remaining--;
					if (remaining < 1)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void OutputData()
		{
			string sWritePath = Application.dataPath + "/_GamePark/BrushstrokeEliminate/Resources/BrushstrokeEliminate/LevelShapes/";
			WriteJson(sWritePath);
		}

		private void WriteJson(string sWritePath)
		{
			string path = sWritePath + base.name + ".json";
			Transform content = transform.Find("Content");
			ShapeData shapeData = new ShapeData();
			if (content != null)
			{
				shapeData.Data = new int[content.childCount];
				shapeData.Row = origin.x;
				shapeData.Col = origin.y;
				foreach (Transform child in content)
				{
					ShapeGizmosOne component = child.GetComponent<ShapeGizmosOne>();
					shapeData.Data[component.Index - 1] = (int)component.kType;
				}
			}
			string json = JsonConvert.SerializeObject(shapeData);
			File.WriteAllText(path, json);
		}
	}
}
