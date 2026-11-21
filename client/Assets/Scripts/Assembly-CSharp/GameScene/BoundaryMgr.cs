using UnityEngine;

namespace GameScene
{
	public class BoundaryMgr : MonoBehaviour
	{
		public static BoundaryMgr I;

		[SerializeField]
		private Transform leftWallTf;

		[SerializeField]
		private Transform rightWallTf;

		[SerializeField]
		private Transform forwardWallTf;

		[SerializeField]
		private Transform backWallTf;

		[SerializeField]
		private Transform platformTf;

		private void Awake()
		{
			I = this;
		}

		public void Init(Transform gameRect)
		{
			Rect rect = gameRect.Rect();
			Vector3 position = gameRect.position;
			Transform transform = platformTf;
			float num = rect.height / 100f * 0.5f;
			float num2 = rect.width / 100f * 0.5f;
			float num3 = num + position.z;
			float num4 = position.z - num;
			Config.CorrectUpBoundary = 11f;
			Config.CorrectDownBoundary = -1f;
			Config.CorrectForwardBoundary = num3 + 3f;
			Config.CorrectRightBoundary = num2 + 1f;
			Config.CorrectBackBoundary = num4;
			Config.CorrectLeftBoundary = -1f - num2;
			Config.CreateLeftBoundary = 1f - num2;
			Config.CreateRightBoundary = num2 - 1f;
			Config.CreateBackBoundary = num4 + 3.5f;
			Config.CreateForwardBoundary = num3 - 1.3f;
			Config.CreateUpBoundary = 8f;
			Config.CreateDownBoundary = 1f;
			Vector3 position2 = transform.position;
			position2.z = num4 + 5.4f;
			platformTf.position = position2;
			Vector3 position3 = leftWallTf.position;
			position3.x = -3.4f - num2;
			leftWallTf.position = position3;
			Vector3 position4 = rightWallTf.position;
			position4.x = num2 + 3.4f;
			rightWallTf.position = position4;
			Vector3 position5 = forwardWallTf.position;
			position5.z = num3 + 6.6f;
			forwardWallTf.position = position5;
		}
	}
}
