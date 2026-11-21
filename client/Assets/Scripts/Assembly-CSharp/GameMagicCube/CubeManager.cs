using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameScene;
using UnityEngine;

namespace GameMagicCube
{
	public class CubeManager
	{
		public Transform _tfContent;

		private CubeItem _pCell;

		private readonly Dictionary<Vector3Int, CubeItem> _dicCubes;

		private Queue<CubeItem> _poolCubes;

		private LinearTriggerOne triggerGatherTogether;

		private Dictionary<int, List<CubeItem>> _dicOptionalCube;

		public List<CubeItem> ltCubes;

		public void Destroy()
		{
			LinearTrigger.Clear("GatherTogetherCubes");
		}

		public CubeManager(Transform _content)
		{
			_dicCubes = new Dictionary<Vector3Int, CubeItem>();
			_poolCubes = new Queue<CubeItem>(64);
			_dicOptionalCube = new Dictionary<int, List<CubeItem>>();
			ltCubes = new List<CubeItem>();
			_tfContent = _content;
			triggerGatherTogether = LinearTrigger.Create("GatherTogetherCubes")
				.Push(() =>
				{
					if (_GatherTogetherCubesX())
					{
						TweenMgr.DelayCall(0.5f, triggerGatherTogether.Next);
					}
					else
					{
						triggerGatherTogether.Next();
					}
				})
				.Push(() =>
				{
					if (_GatherTogetherCubesY())
					{
						TweenMgr.DelayCall(0.5f, triggerGatherTogether.Next);
					}
					else
					{
						triggerGatherTogether.Next();
					}
				})
				.Push(() =>
				{
					if (_GatherTogetherCubesZ())
					{
						TweenMgr.DelayCall(0.5f, triggerGatherTogether.Next);
					}
					else
					{
						triggerGatherTogether.Next();
					}
				})
				.Push(JudgetEliminateCube);
		}

		public void SetCellCube(Transform tfContent)
		{
			GameObject gameObject = LoadMgr.LoadRes<GameObject>("MagicCubes/Cubes/TM_cube");
			gameObject.transform.SetParent(tfContent, false);
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			gameObject.SetActive(false);
			boxCollider.enabled = false;
			_pCell = gameObject.AddComponent<CubeItem>();
		}

		private CubeItem PopItem()
		{
			if (_poolCubes.Count > 0)
			{
				return _poolCubes.Dequeue();
			}
			return UnityEngine.Object.Instantiate(_pCell, _tfContent, false);
		}

		private void PushItem(CubeItem slot)
		{
			_poolCubes.Enqueue(slot);
		}

		public void CreateCubes()
		{
			foreach (var item in _dicCubes.Values)
			{
				PushItem(item);
			}
			_dicCubes.Clear();
			Vector3Int range = DataMgr.I.pCfg.range;
			for (int z = 0; z < range.z; z++)
			{
				for (int y = 0; y < range.y; y++)
				{
					for (int x = 0; x < range.x; x++)
					{
						_dicCubes.Add(new Vector3Int(x, y, z), PopItem());
					}
				}
			}
			_SetCubesLoc();
		}

		private void _SetCubesLoc()
		{
			foreach (var pair in _dicCubes)
			{
				pair.Value.gameObject.SetActive(true);
				pair.Value.SetIndex(pair.Key);
			}
		}

		public float SetSlotLocAni()
		{
			_tfContent.DOLocalRotate(new Vector3(-110f, 0f, 46f), 0.45f);
			return 0.45f;
		}

		public List<int> GetCubeID(out int _cubeCount)
		{
			Vector3Int range = DataMgr.I.pCfg.range;
			_cubeCount = (range.x * range.y * range.z) / 3;
			List<int> list = new List<int>();
			for (int i = 92; i >= 3; i--)
			{
				list.Add(i);
			}
			list.SortR();
			int num = _cubeCount - list.Count;
			if (num < 0)
			{
				list.RemoveRange(_cubeCount, -num);
			}
			else
			{
				for (int j = 0; j < num; j++)
				{
					list.Add(list.Random());
				}
			}
			List<int> list2 = new List<int>();
			list2.AddRange(list);
			list2.AddRange(list);
			list2.AddRange(list);
			list2.SortR();
			return list2;
		}

		public void SetCubeID(List<int> values)
		{
			int num = 0;
			foreach (var item in _dicCubes.Values)
			{
				item.SetData(values[num++]);
			}
		}

		private IEnumerator LoadCubeTexture(List<Vector3Int> keys, int start, int end)
		{
			for (int i = start; i < end; i++)
			{
				yield return _dicCubes[keys[i]].LoadTexture();
			}
		}

		public void LoadCubeTexture()
		{
			List<Vector3Int> list = _dicCubes.Keys.ToList();
			int count = list.Count;
			int num = count / 3;
			int num2 = 0;
			int num3 = num;
			for (int i = 0; i < 3; i++)
			{
				DataMgr.pCubeCtrl.StartCoroutine(LoadCubeTexture(list, num2, num3));
				num2 = Mathf.Clamp(num2 + num, 0, count - 1);
				num3 = Mathf.Clamp(num3 + num, num, count);
			}
		}

		private void SwapCube(CubeItem cube1, CubeItem cube2)
		{
			_dicCubes[cube1.Index] = cube2;
			_dicCubes[cube2.Index] = cube1;
			Vector3 originPoint = cube1.OriginPoint;
			if (!cube1.Selected && cube1.gameObject.activeSelf)
			{
				if (cube1.IsEmty)
				{
					cube1.transform.localPosition = cube2.OriginPoint;
				}
				else
				{
					cube1.transform.DOLocalMove(cube2.OriginPoint, 0.5f);
				}
			}
			if (!cube2.Selected && cube2.gameObject.activeSelf)
			{
				if (cube2.IsEmty)
				{
					cube2.transform.localPosition = originPoint;
				}
				else
				{
					cube2.transform.DOLocalMove(originPoint, 0.5f);
				}
			}
			Vector3Int index = cube1.Index;
			cube1.Index = cube2.Index;
			cube2.Index = index;
		}

		public void GatherTogetherCubes()
		{
			triggerGatherTogether.Init();
			triggerGatherTogether.Next();
		}

		private bool _GatherTogetherCubesX()
		{
			Vector3Int range = DataMgr.I.pCfg.range;
			int mid = (int)Math.Round((double)(range.x / 2));
			bool flag = false;
			for (int x = mid - 1; x >= 0; x--)
			{
				for (int y = range.y - 1; y >= 0; y--)
				{
					for (int z = range.z - 1; z >= 0; z--)
					{
						CubeItem cube = _dicCubes[Utils.Vec3Int(x, y, z)];
						if (!cube.IsEmty)
						{
							CubeItem emptyCube = null;
							for (int sx = x + 1; sx <= mid; sx++)
							{
								CubeItem candidate = _dicCubes[Utils.Vec3Int(sx, y, z)];
								if (candidate.IsEmty)
								{
									emptyCube = candidate;
								}
								else
								{
									break;
								}
							}
							if (emptyCube != null)
							{
								SwapCube(cube, emptyCube);
								flag = true;
								break;
							}
						}
					}
				}
			}
			int mid2 = mid + (~range.x & 1);
			for (int x2 = mid2 + 1; x2 < range.x; x2++)
			{
				for (int y = range.y - 1; y >= 0; y--)
				{
					for (int z = range.z - 1; z >= 0; z--)
					{
						CubeItem cube = _dicCubes[Utils.Vec3Int(x2, y, z)];
						if (!cube.IsEmty)
						{
							CubeItem emptyCube = null;
							for (int sx = x2 - 1; sx >= mid2; sx--)
							{
								CubeItem candidate = _dicCubes[Utils.Vec3Int(sx, y, z)];
								if (candidate.IsEmty)
								{
									emptyCube = candidate;
								}
								else
								{
									break;
								}
							}
							if (emptyCube != null)
							{
								SwapCube(cube, emptyCube);
								flag = true;
								break;
							}
						}
					}
				}
			}
			return flag;
		}

		private bool _GatherTogetherCubesY()
		{
			Vector3Int range = DataMgr.I.pCfg.range;
			int mid = (int)Math.Round((double)(range.y / 2));
			bool flag = false;
			for (int y = mid - 1; y >= 0; y--)
			{
				for (int x = range.x - 1; x >= 0; x--)
				{
					for (int z = range.z - 1; z >= 0; z--)
					{
						CubeItem cube = _dicCubes[Utils.Vec3Int(x, y, z)];
						if (!cube.IsEmty)
						{
							CubeItem emptyCube = null;
							for (int sy = y + 1; sy <= mid; sy++)
							{
								CubeItem candidate = _dicCubes[Utils.Vec3Int(x, sy, z)];
								if (candidate.IsEmty)
								{
									emptyCube = candidate;
								}
								else
								{
									break;
								}
							}
							if (emptyCube != null)
							{
								SwapCube(cube, emptyCube);
								flag = true;
								break;
							}
						}
					}
				}
			}
			int mid2 = mid + (~range.y & 1);
			for (int y2 = mid2 + 1; y2 < range.y; y2++)
			{
				for (int x = range.x - 1; x >= 0; x--)
				{
					for (int z = range.z - 1; z >= 0; z--)
					{
						CubeItem cube = _dicCubes[Utils.Vec3Int(x, y2, z)];
						if (!cube.IsEmty)
						{
							CubeItem emptyCube = null;
							for (int sy = y2 - 1; sy >= mid2; sy--)
							{
								CubeItem candidate = _dicCubes[Utils.Vec3Int(x, sy, z)];
								if (candidate.IsEmty)
								{
									emptyCube = candidate;
								}
								else
								{
									break;
								}
							}
							if (emptyCube != null)
							{
								SwapCube(cube, emptyCube);
								flag = true;
								break;
							}
						}
					}
				}
			}
			return flag;
		}

		private bool _GatherTogetherCubesZ()
		{
			Vector3Int range = DataMgr.I.pCfg.range;
			int mid = (int)Math.Round((double)(range.z / 2));
			bool flag = false;
			for (int z = mid - 1; z >= 0; z--)
			{
				for (int x = range.x - 1; x >= 0; x--)
				{
					for (int y = range.y - 1; y >= 0; y--)
					{
						CubeItem cube = _dicCubes[Utils.Vec3Int(x, y, z)];
						if (!cube.IsEmty)
						{
							CubeItem emptyCube = null;
							for (int sz = z + 1; sz <= mid; sz++)
							{
								CubeItem candidate = _dicCubes[Utils.Vec3Int(x, y, sz)];
								if (candidate.IsEmty)
								{
									emptyCube = candidate;
								}
								else
								{
									break;
								}
							}
							if (emptyCube != null)
							{
								SwapCube(cube, emptyCube);
								flag = true;
								break;
							}
						}
					}
				}
			}
			int mid2 = mid + (~range.z & 1);
			for (int z2 = mid2 + 1; z2 < range.z; z2++)
			{
				for (int x = range.x - 1; x >= 0; x--)
				{
					for (int y = range.y - 1; y >= 0; y--)
					{
						CubeItem cube = _dicCubes[Utils.Vec3Int(x, y, z2)];
						if (!cube.IsEmty)
						{
							CubeItem emptyCube = null;
							for (int sz = z2 - 1; sz >= mid2; sz--)
							{
								CubeItem candidate = _dicCubes[Utils.Vec3Int(x, y, sz)];
								if (candidate.IsEmty)
								{
									emptyCube = candidate;
								}
								else
								{
									break;
								}
							}
							if (emptyCube != null)
							{
								SwapCube(cube, emptyCube);
								flag = true;
								break;
							}
						}
					}
				}
			}
			return flag;
		}

		private void AddOptionalCube(CubeItem cube)
		{
			if (!_dicOptionalCube.ContainsKey(cube.ID))
			{
				_dicOptionalCube.Add(cube.ID, new List<CubeItem> { cube });
			}
			else
			{
				_dicOptionalCube[cube.ID].Add(cube);
			}
		}

		public void CalculateOptionalCube()
		{
			Vector3Int range = DataMgr.I.pCfg.range;
			int maxX = range.x - 1;
			int maxY = range.y - 1;
			int maxZ = range.z - 1;
			_dicOptionalCube.Clear();
			foreach (var pair in _dicCubes)
			{
				if (pair.Value.IsEmty)
				{
					continue;
				}
				Vector3Int key = pair.Key;
				if (key.x == 0 || key.x == maxX || key.y == 0 || key.y == maxY || key.z == 0 || key.z == maxZ)
				{
					AddOptionalCube(pair.Value);
					continue;
				}
				if (key.x > 0 && _dicCubes[Utils.Vec3Int(key.x - 1, key.y, key.z)].IsEmty)
				{
					AddOptionalCube(pair.Value);
					continue;
				}
				if (key.x < maxX && _dicCubes[Utils.Vec3Int(key.x + 1, key.y, key.z)].IsEmty)
				{
					AddOptionalCube(pair.Value);
					continue;
				}
				if (key.y > 0 && _dicCubes[Utils.Vec3Int(key.x, key.y - 1, key.z)].IsEmty)
				{
					AddOptionalCube(pair.Value);
					continue;
				}
				if (key.y < maxY && _dicCubes[Utils.Vec3Int(key.x, key.y + 1, key.z)].IsEmty)
				{
					AddOptionalCube(pair.Value);
					continue;
				}
				if (key.z > 0 && _dicCubes[Utils.Vec3Int(key.x, key.y, key.z - 1)].IsEmty)
				{
					AddOptionalCube(pair.Value);
					continue;
				}
				if (key.z < maxZ && _dicCubes[Utils.Vec3Int(key.x, key.y, key.z + 1)].IsEmty)
				{
					AddOptionalCube(pair.Value);
					continue;
				}
			}
			foreach (var point in DataMgr.pCubeCtrl.pTableCtrl.Points)
			{
				if (point.Cube != null)
				{
					AddOptionalCube(point.Cube);
				}
			}
		}

		public bool HaveEliminateCube()
		{
			ltCubes.Clear();
			foreach (var pair in _dicOptionalCube)
			{
				if (pair.Value.Count >= 3)
				{
					for (int i = 0; i < 3; i++)
					{
						ltCubes.Add(pair.Value[i]);
					}
					return true;
				}
			}
			return false;
		}

		public void RearrangeCube()
		{
			List<Vector3Int> list = _dicCubes.Keys.ToList();
			list.SortR();
			while (list.Count >= 2)
			{
				Vector3Int key1 = list.PopR();
				CubeItem cube1 = _dicCubes[key1];
				Vector3Int key2 = list.PopR();
				CubeItem cube2 = _dicCubes[key2];
				SwapCube(cube1, cube2);
			}
			JudgetEliminateCube();
		}

		public void JudgetEliminateCube()
		{
			if (DataMgr.pMain.kGameStatus != GameStatus.Victory)
			{
				CalculateOptionalCube();
				if (!HaveEliminateCube())
				{
					RearrangeCube();
				}
				else
				{
					DataMgr.pMain.kGameStatus = GameStatus.Playing;
				}
			}
		}
	}
}
