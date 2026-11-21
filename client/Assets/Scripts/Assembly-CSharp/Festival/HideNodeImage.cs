using UnityEngine;
using UnityEngine.UI;

namespace Festival
{
	[RequireComponent(typeof(Image))]
	public class HideNodeImage : OperationBase
	{
		protected override void Awake()
		{
			base.Awake();
			if (PointInfo != null)
			{
				Image component = GetComponent<Image>();
				if (component != null)
				{
					component.enabled = false;
				}
			}
		}
	}
}
