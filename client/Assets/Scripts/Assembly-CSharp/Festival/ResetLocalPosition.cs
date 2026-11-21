using UnityEngine;

namespace Festival
{
	[RequireComponent(typeof(RectTransform))]
	public class ResetLocalPosition : OperationBase
	{
		protected override void Awake()
		{
			base.Awake();
		}

		protected override void Refresh()
		{
			if (PointInfo != null)
			{
				transform.localPosition = PointInfo.information.ToVector2();
			}
		}
	}
}
