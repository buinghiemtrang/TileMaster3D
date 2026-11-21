using UnityEngine;

namespace ExtraFoundation.Components
{
	public class LineRenderer3D : MonoBehaviour
	{
		[SerializeField]
		private LineRenderer lineRenderer;

		public int Count => lineRenderer.positionCount;

		public void Clear()
		{
			lineRenderer.positionCount = 0;
		}

		public void Add(Vector3 position)
		{
			int positionCount = lineRenderer.positionCount;
			lineRenderer.positionCount = positionCount + 1;
			lineRenderer.SetPosition(positionCount, position);
		}

		public void DelLast()
		{
			lineRenderer.positionCount = lineRenderer.positionCount - 1;
		}

		public void Set(Vector3 position)
		{
			lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
		}

		public void Set(int i, Vector3 position)
		{
			lineRenderer.SetPosition(i, position);
		}

		public Vector3 Last(int i = 0)
		{
			int num = lineRenderer.positionCount - 1 - i;
			if (num < 0)
			{
				return Vector3.zero;
			}
			return lineRenderer.GetPosition(num);
		}
	}
}
