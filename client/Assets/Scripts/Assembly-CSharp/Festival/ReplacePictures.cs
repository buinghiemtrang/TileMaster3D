using UnityEngine;
using UnityEngine.UI;

namespace Festival
{
	public class ReplacePictures : OperationBase
	{
		[SerializeField]
		private Image[] images;

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
				Sprite sprite = LoadMgr.LoadSprite(PointInfo.information);
				if (sprite)
				{
					for (int i = 0; i < images.Length; i++)
					{
						images[i].sprite = sprite;
						if (isNativeSize)
						{
							images[i].SetNativeSize();
						}
					}
				}
			}
		}
	}
}
