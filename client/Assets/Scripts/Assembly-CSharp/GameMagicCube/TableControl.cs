using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace GameMagicCube
{
	public class TableControl : MonoBehaviour
	{
		private const int CFG_SCORE = 5;

		private List<TablePlacePoint> _points;

		[SerializeField]
		private Transform tfContent;

		[SerializeField]
		private ParticleSystemControl _boomEffect;

		private static Vector3 posOff;

		public List<TablePlacePoint> Points => _points;

		static TableControl()
		{
			posOff = new Vector3(0f, 0.3f, 0f);
		}

		private void Awake()
		{
			_points = GetComponentsInChildren<TablePlacePoint>().ToList();
		}

		private void OnDestroy()
		{
			DOTween.Kill("TablePlacePoint_Move", false);
			DOTween.Kill("TablePlacePoint_Rotate", false);
		}

		private int GetPlaceLength()
		{
			int num = 0;
			foreach (TablePlacePoint point in _points)
			{
				if (point.Cube == null)
				{
					num++;
				}
			}
			return num;
		}

		public void Collect(CubeItem cube)
		{
			TablePlacePoint point = _points.Find(a => a.Cube == null);
			if (point == null)
			{
				if (DataMgr.pMain != null)
				{
					DataMgr.pMain.GameFail("Box");
				}
				return;
			}
			TablePlacePoint lastSame = _points.FindLast(a => a.Cube != null && a.Cube.ID == cube.ID);
			if (lastSame != null)
			{
				point = _points[lastSame.ID];
				FallBackPoints(lastSame.ID);
			}
			point.CaptureCube(cube);
			JudgetLink();
		}

		private void FallBackPoints(int start)
		{
			for (int i = _points.Count - 2; i >= start; i--)
			{
				CubeItem cube = _points[i].Cube;
				if (cube != null)
				{
					Vector3 localPosition = _points[i].transform.localPosition;
					_points[i + 1].SetCube(cube, localPosition, 0f);
					_points[i].ReleaseCube();
				}
			}
		}

		private void ForwardPoints(float fdelay)
		{
			if (_points.Count > 1)
			{
				for (int i = 1; i < _points.Count; i++)
				{
					if (_points[i].Cube != null)
					{
						TablePlacePoint targetPoint = null;
						for (int j = i - 1; j >= 0; j--)
						{
							if (_points[j].Cube != null)
							{
								break;
							}
							targetPoint = _points[j];
						}
						if (targetPoint != null)
						{
							CubeItem cube = _points[i].Cube;
							Vector3 localPosition = _points[i].transform.localPosition;
							targetPoint.SetCube(cube, localPosition, fdelay);
							_points[i].ReleaseCube();
						}
					}
				}
			}
		}

		public void JudgetLink()
		{
			if (_points.Count > 0)
			{
				bool flag = false;
				int count = 0;
				TablePlacePoint currentPoint = null;
				for (int i = 0; i < _points.Count; i++)
				{
					TablePlacePoint point = _points[i];
					if (point.Cube == null)
					{
						continue;
					}
					if (currentPoint == null)
					{
						count = 1;
						currentPoint = point;
					}
					else if (currentPoint.Cube.ID == point.Cube.ID)
					{
						count++;
						if (count == 3)
						{
							Eliminate(currentPoint.ID - 1);
							flag = true;
							currentPoint = null;
							count = 0;
						}
					}
					else
					{
						count = 1;
						currentPoint = point;
					}
				}
				if (flag)
				{
					ForwardPoints(0.5f);
				}
			}
			if (_points.Find(a => a.Cube == null) == null)
			{
				if (DataMgr.pMain != null)
				{
					DataMgr.pMain.GameFail("Box");
				}
			}
		}

		private void Eliminate(int i)
		{
			CubeItem cube1 = _points[i].Cube;
			CubeItem cube2 = _points[i + 1].Cube;
			CubeItem cube3 = _points[i + 2].Cube;
			_points[i].ReleaseCube();
			_points[i + 1].ReleaseCube();
			_points[i + 2].ReleaseCube();
			foreach (CubeItem cube in DataMgr.pCubeMgr.ltCubes)
			{
				cube.SetHintShow(false);
			}
			LevelInterAd.TryShow();
			if (DataMgr.pMain != null && DataMgr.pMain.pScore != null)
			{
				DataMgr.pMain.pScore.ModifyScore(5);
				if (DataMgr.pCubeCtrl != null)
				{
					DataMgr.pCubeCtrl.DelCubeCount();
					bool bWin = DataMgr.pMain.kGameStatus == GameScene.GameStatus.Victory;
					Vector3 centerPos = _points[i + 1].transform.localPosition;
					Vector3 p1Pos = _points[i].transform.localPosition;
					Vector3 p3Pos = _points[i + 2].transform.localPosition;
					cube1.transform.DOBlendableLocalMoveBy(centerPos - p1Pos, 0.2f, false).SetDelay(0.3f);
					cube3.transform.DOBlendableLocalMoveBy(centerPos - p3Pos, 0.2f, false).SetDelay(0.3f).OnComplete(() =>
					{
						AudioMgr.I.PlaySound("fusion");
						ParticleSystemControl boom = Object.Instantiate(_boomEffect, tfContent);
						boom.transform.localPosition = _points[i + 1].transform.localPosition + posOff;
						boom.gameObject.SetActive(true);
						cube1.Eliminate();
						cube2.Eliminate();
						cube3.Eliminate();
						Vector3 uiPos = DataMgr.pCubeCtrl.CameraToUIPos(_points[i + 1].transform.position);
						DataMgr.pMain.pScore.ModifyScoreShow(5, uiPos);
						if (!bWin)
						{
							if (DataMgr.pCubeCtrl.nCubeCount > 0)
							{
								DataMgr.pCubeMgr.GatherTogetherCubes();
							}
						}
						else
						{
							DataMgr.pMain.GameVictory("PassGame");
						}
					});
				}
			}
		}
	}
}
