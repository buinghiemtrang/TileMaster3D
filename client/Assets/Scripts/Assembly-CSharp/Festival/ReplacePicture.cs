using UnityEngine;
using UnityEngine.UI;

namespace Festival
{
	[RequireComponent(typeof(Image))]
	public class ReplacePicture : OperationBase
	{
		[SerializeField]
		private bool isNativeSize;

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void Refresh()
		{
			if (PointInfo != null)
			{
				Image component = GetComponent<Image>();
				Sprite sprite = LoadMgr.LoadSprite(PointInfo.information);
				if (sprite)
				{
					component.sprite = sprite;
					if (isNativeSize)
					{
						component.SetNativeSize();
					}
				}
			}
		}
	}
}
