using System;
using System.Collections.Generic;
using DG.Tweening;
using GamePark.BrushstrokeEliminate.BaseData;
using GameScene;
using UnityEngine;

namespace GamePark.BrushstrokeEliminate
{
	public class ElementMgr : Singleton<ElementMgr>
	{
		private ElementBase _cellElementBase;

		private Transform _tfElementLayer;

		private Transform _tfElementRange;

		private readonly Queue<ElementBase> _elementPool = new Queue<ElementBase>();

		private readonly List<ElementBase> _elements = new List<ElementBase>();

		private int _row;

		private int _col;

		private int _size;

		private int[] _datas;

		public void Init(ElementBase cell, Transform tfLayer, Transform tfRange)
		{
			_cellElementBase = cell;
			_tfElementLayer = tfLayer;
			_tfElementRange = tfRange;
		}

		public void SetShapeCfg(ShapeData shape)
		{
			_row = shape.Row;
			_col = shape.Col;
			Rect rect = _tfElementRange.Rect();
			_size = Math.Min(150, Math.Min((int)(rect.width / (float)_row), (int)(rect.height / (float)_col)));
			_datas = shape.Data;
		}

		public ElementBase PopElement()
		{
			if (_elementPool.Count > 0)
			{
				return _elementPool.Dequeue();
			}
			ElementBase elementBase = UnityEngine.Object.Instantiate(_cellElementBase, _tfElementLayer, false);
			elementBase.gameObject.SetActive(false);
			return elementBase;
		}

		public void PushElement(ElementBase element)
		{
			element.gameObject.SetActive(false);
			element.Clear();
			_elementPool.Enqueue(element);
		}

		public void CreateElements()
		{
			_elements.Clear();
			int[] datas = _datas;
			for (int i = 0; i < datas.Length; i++)
			{
				ElementBase elementBase = PopElement();
				elementBase.Init(datas[i], _size);
				_elements.Add(elementBase);
			}
			_SetElementLocation();
			_PlayElementEnter();
		}

		private void _SetElementLocation()
		{
			for (int i = 0; i < _col; i++)
			{
				for (int j = 0; j < _row; j++)
				{
					ElementBase element = GetElement(j, i);
					element.loc.x = j;
					element.loc.y = i;
					element.transform.SetLocPos(((float)(_size - _size * _row) / 2f + (float)(_size * j)) / 100f, ((float)((_col - 1) * _size) / 2f - (float)(_size * i)) / 100f);
					element.UpdateSiblingIndex(_row);
					element.gameObject.SetActive(true);
				}
			}
		}

		private void _PlayElementEnter()
		{
			int totalNum = _elements.Count;
			float height = SceneMgr.height;
			foreach (ElementBase element in _elements)
			{
				element.transform.DOLocalMoveY(height / 100f, 0.5f).SetDelay(UnityEngine.Random.Range(0.1f, 0.3f)).From(true)
					.SetEase(Ease.OutBack)
					.OnComplete(delegate
					{
						totalNum--;
						if (totalNum <= 0)
						{
							Singleton<DataMgr>.I.kGameStatus = GameStatus.Playing;
							GameParkGuideDialog.Create(GameParkType.BrushstrokeEliminate);
						}
					});
			}
		}

		public ElementBase GetElement(int x, int y)
		{
			return _elements[x + _row * y];
		}

		public ElementBase GetElement(Vector3 position)
		{
			int num = (int)Math.Ceiling((float)(_size * (_row - 1)) * 0.5f + position.x * 100f);
			int num2 = (int)Math.Ceiling((float)(_size * (_col - 1)) * 0.5f - position.y * 100f);
			return _elements[num / _size + num2 / _size * _row];
		}

		public void ClearElement()
		{
			List<ElementBase> list = new List<ElementBase>();
			list.AddRange(_elements);
			_elements.Clear();
			CounterMgr.Create(delegate
			{
				foreach (ElementBase item in list)
				{
					PushElement(item);
				}
				list.Clear();
			}, "ClearElement");
			float height = SceneMgr.height;
			foreach (ElementBase item2 in list)
			{
				CounterMgr.Add("ClearElement");
				item2.transform.DOLocalMoveY(height / -100f, 0.5f).SetDelay(UnityEngine.Random.Range(0.1f, 0.3f)).SetEase(Ease.InBack)
					.OnComplete(delegate
					{
						CounterMgr.Finish("ClearElement");
					});
			}
			CounterMgr.Finish("ClearElement");
		}
	}
}
