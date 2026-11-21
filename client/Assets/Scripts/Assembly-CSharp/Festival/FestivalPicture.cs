using UnityEngine;
using UnityEngine.UI;

namespace Festival
{
	[RequireComponent(typeof(Image))]
	public class FestivalPicture : OperationBase
	{
		private Image img;

		private Sprite originalSpr;

		protected override void Awake()
		{
			img = GetComponent<Image>();
			originalSpr = img.sprite;
			DispatcherMgr.On("UPDATE_FESTIVAL", Refresh);
			Refresh();
		}

		protected override void Refresh()
		{
			PointInfo = Singleton<FestivalMgr>.I.GetFestivalInfo(pointKey);
			if (PointInfo != null)
			{
				Sprite sprite = LoadMgr.LoadSprite(PointInfo.information);
				if (sprite)
				{
					if (Singleton<FestivalMgr>.I.IsShow)
					{
						if (originalSpr == null)
						{
							img.enabled = true;
						}
						img.sprite = sprite;
					}
					else
					{
						if (originalSpr == null)
						{
							img.enabled = false;
						}
						else
						{
							img.sprite = originalSpr;
						}
					}
				}
			}
			else if (Singleton<FestivalMgr>.I.HasFestival)
			{
				img.enabled = false;
			}
		}
	}
}
