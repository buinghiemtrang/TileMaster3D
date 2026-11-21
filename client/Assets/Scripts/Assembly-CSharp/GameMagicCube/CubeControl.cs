using System.Collections.Generic;
using DG.Tweening;
using GameScene;
using UnityEngine;

namespace GameMagicCube
{
	public class CubeControl : MonoBehaviour
	{
		public SpriteRenderer spBackground;

		public Transform tfRoot2D;

		public Transform tfContentCubes;

		public TableControl pTableCtrl;

		private CubeItem lastSelect;

		[SerializeField]
		private Camera _camera;

		private int _raycastLayerMask;

		public int nCubeCount;

		private void Awake()
		{
			DataMgr.pCubeMgr = new CubeManager(tfContentCubes);
			DataMgr.pCubeMgr.SetCellCube(transform);
			_raycastLayerMask = 1 << LayerMask.NameToLayer("3D");
		}

		private void Start()
		{
			tfRoot2D.Rect();
		}

		private void OnDestroy()
		{
			DataMgr.pCubeMgr.Destroy();
		}

		public Vector3 CameraToUIPos(Vector3 pos3d)
		{
			return Camera.main.ViewportToWorldPoint(_camera.WorldToViewportPoint(pos3d));
		}

		public void RestartData()
		{
			DataMgr.pMain.pScore.SetScore(0);
			DataMgr.pMain.pTimeBar.InitTime(DataMgr.I.pCfg.time, DataMgr.I.pCfg.time, () =>
			{
				DataMgr.pMain.GameFail("Time");
			}, true);
			ClearSelect();
			DataMgr.pCubeMgr.CreateCubes();
			List<int> cubeID = DataMgr.pCubeMgr.GetCubeID(out nCubeCount);
			DataMgr.pCubeMgr.SetCubeID(cubeID);
			DataMgr.pCubeMgr.LoadCubeTexture();
			DataMgr.pCubeMgr._tfContent.DOLocalRotate(new Vector3(-110f, 0f, 46f), 0.45f);
			TweenMgr.DelayCall(0.45f, () =>
			{
				DataMgr.pMain.kGameStatus = GameStatus.Playing;
				DataMgr.pMain.pTimeBar.Play();
				DataMgr.pCubeMgr.JudgetEliminateCube();
				GameParkGuideDialog.Create(GameParkType.MagicCube);
			});
		}

		public void DelCubeCount()
		{
			if (nCubeCount > 0)
			{
				nCubeCount--;
				if (nCubeCount == 0)
				{
					DataMgr.pMain.pTimeBar.Kill();
					DataMgr.pMain.kGameStatus = GameStatus.Victory;
				}
			}
		}

		public void OnOptMove(Vector3 touch, Vector3 difference)
		{
			tfContentCubes.Rotate(difference.y * 0.1f, -difference.x * 0.1f, 0f, Space.World);
		}

		public void OnOptDown(Vector3 touch)
		{
			if (DataMgr.pMain.kGameStatus == GameStatus.Playing)
			{
				DataMgr.pMain.isCanPause = false;
				if (RaycastCube(touch, out GameObject hitObject))
				{
					CubeSelect(hitObject);
				}
			}
		}

		public void OnOptUp(Vector3 touch)
		{
			if (DataMgr.pMain.kGameStatus == GameStatus.Playing)
			{
				DataMgr.pMain.isCanPause = true;
				if (lastSelect != null)
				{
					lastSelect.SetSelectColor(false);
					lastSelect = null;
				}
			}
		}

		public void OnOptClick(Vector3 touch)
		{
			if (DataMgr.pMain.kGameStatus == GameStatus.Playing)
			{
				if (lastSelect != null)
				{
					AudioMgr.I.PlaySound("selected");
					pTableCtrl.Collect(lastSelect);
					if (lastSelect != null)
					{
						lastSelect.SetSelectColor(false);
						lastSelect = null;
					}
				}
			}
		}

		private bool RaycastCube(Vector2 pointerPosition, out GameObject hitObject)
		{
			Ray ray = _camera.ScreenPointToRay(pointerPosition);
			if (Physics.Raycast(ray, out RaycastHit hitInfo, 50f, _raycastLayerMask))
			{
				hitObject = hitInfo.collider.gameObject;
				return true;
			}
			hitObject = null;
			return false;
		}

		public void CubeSelect(GameObject objCube)
		{
			CubeItem component = objCube.GetComponent<CubeItem>();
			if (component == null)
			{
				return;
			}
			if (lastSelect != null && lastSelect == component)
			{
				return;
			}
			AudioMgr.I.PlaySound("button");
			ClearSelect();
			lastSelect = component;
			lastSelect.SetSelectColor(true);
		}

		public void ClearSelect()
		{
			if (lastSelect != null)
			{
				lastSelect.SetSelectColor(false);
				lastSelect = null;
			}
		}
	}
}
