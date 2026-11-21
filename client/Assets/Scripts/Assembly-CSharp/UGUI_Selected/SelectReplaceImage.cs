using UnityEngine;
using UnityEngine.UI;

namespace UGUI_Selected
{
	public class SelectReplaceImage : SelectBase
	{
		[SerializeField]
		private Sprite selectedSprite;

		private Image _image;

		private Sprite _oldSprite;

		public override void Select(bool isSelected)
		{
			if (_image == null)
			{
				_image = GetComponent<Image>();
			}
			if (isSelected)
			{
				_oldSprite = _image.sprite;
				_image.sprite = selectedSprite;
			}
			else if (_oldSprite != null)
			{
				_image.sprite = _oldSprite;
			}
		}
	}
}
