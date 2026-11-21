using System;
using UnityEngine;

namespace GameMagicCube
{
	public class DataMgr
	{
		public static readonly int CubeLayerID;

		private static DataMgr _instance;

		public static GameMagicCubeScene pMain;

		public static CubeControl pCubeCtrl;

		public static CubeManager pCubeMgr;

		public CfgData pCfg;

		public static DataMgr I => _instance;

		static DataMgr()
		{
			CubeLayerID = LayerMask.NameToLayer("3D");
			_instance = new DataMgr();
		}

		public void Destroy()
		{
			pMain = null;
			if (pCubeCtrl != null)
			{
				pCubeCtrl.DestroySelf();
				pCubeCtrl = null;
			}
		}

		public Vector3 GetCubePoint(Vector3 index)
		{
			return new Vector3(
				index.x * pCfg.size - pCfg.center.x,
				index.y * pCfg.size - pCfg.center.y,
				index.z * pCfg.size - pCfg.center.z
			);
		}

		public void CreateLevel()
		{
			pCfg = new CfgData();
			pCfg.range.x = MMath.Random(3, 6);
			pCfg.range.y = MMath.Random(3, 6);
			if ((pCfg.range.x * pCfg.range.y) % 3 == 0)
			{
				pCfg.range.z = MMath.Random(3, 11);
			}
			else
			{
				pCfg.range.z = MMath.Random(1, 4) * 3;
			}
			pCfg.allModeCount = pCfg.range.x * pCfg.range.y * pCfg.range.z;
			pCfg.time = pCfg.allModeCount * 6;

			Rect rect = pCubeCtrl.tfRoot2D.Rect();
			int num = (int)(rect.width / (float)pCfg.range.x);
			int val = Math.Max(pCfg.range.y, pCfg.range.z);
			int num2 = (int)(rect.height / (float)val);
			int val2 = Math.Min(num, num2);
			int num3 = Math.Min(110, val2);
			pCfg.size = (float)num3 / 100f;
			pCfg.center = new Vector3(
				pCfg.size * (float)(pCfg.range.x - 1) * 0.5f,
				pCfg.size * (float)(pCfg.range.y - 1) * 0.5f,
				pCfg.size * (float)(pCfg.range.z - 1) * 0.5f
			);
		}

		public bool NextLayer()
		{
			return true;
		}
	}
}
