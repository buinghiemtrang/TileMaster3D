using UnityEngine;

namespace Festival
{
	public class ReplacePrefab : OperationBase
	{
		[SerializeField]
		private GameObject target;

		[SerializeField]
		private int index;

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void Refresh()
		{
			if (PointInfo != null)
			{
				GameObject gameObject = LoadMgr.Load<GameObject>(PointInfo.information);
				if (gameObject)
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, transform, false);
					if (target)
					{
						UnityEngine.Object.DestroyImmediate(target);
					}
					gameObject2.transform.SetSiblingIndex(index);
					target = gameObject2;
				}
			}
		}
	}
}
