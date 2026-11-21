using DG.Tweening;
using UnityEngine;

namespace GameMagicCube
{
	public class TablePlacePoint : MonoBehaviour
	{
		[SerializeField]
		private int _id;

		public Transform tfContent;

		[SerializeField]
		private CubeItem _cube;

		public int ID => _id;

		public CubeItem Cube => _cube;

		public void CaptureCube(CubeItem cube)
		{
			cube.transform.DOKill(false);
			cube.Selected = true;
			cube.SetColliderEnabled(false);
			Vector3 localScale = cube.transform.localScale;
			cube.transform.SetParent(tfContent, true);
			cube.transform.localScale = localScale;
			cube.transform.localEulerAngles = Vector3.zero;
			cube.transform.DOLocalRotate(Utils.Vec3(0f, -360f, 0f), 5f, RotateMode.FastBeyond360)
				.SetEase(Ease.Linear)
				.SetLoops(-1, LoopType.Restart)
				.SetId("TablePlacePoint_Rotate");
			Vector3 localPosition = cube.transform.localPosition;
			_cube = cube;
			cube.transform.DOBlendableLocalMoveBy(transform.localPosition - localPosition, 0.3f, false)
				.SetId("TablePlacePoint_Move");
		}

		public void SetCube(CubeItem cube, Vector3 lastPoint, float fdelay = 0f)
		{
			_cube = cube;
			Tweener tweener = _cube.transform.DOBlendableLocalMoveBy(transform.localPosition - lastPoint, 0.3f, false)
				.SetId("TablePlacePoint_Move");
			if (fdelay > 0f)
			{
				tweener.SetDelay(fdelay);
			}
		}

		public void ReleaseCube()
		{
			_cube = null;
		}
	}
}
