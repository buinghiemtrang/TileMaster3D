using UnityEngine;
using UnityEngine.UI;

namespace GameScene
{
	public class ActivityInResult : MonoBehaviour
	{
		[SerializeField]
		private Image _imageIcon;

		[SerializeField]
		private Text _textAddNum;

		private void Start()
		{
		}

		public virtual bool IsOpen()
		{
			return false;
		}

		public virtual int GetScore()
		{
			return 0;
		}

		public virtual void Init()
		{
			bool flag = IsOpen();
			base.gameObject.SetActive(flag);
			if (flag)
			{
				int score = GetScore();
				bool flag2 = ActivityInResultMgr.I != null && score != 0 && ActivityInResultMgr.I.bIsVictory;
				_textAddNum.gameObject.SetActive(flag2);
				if (flag2)
				{
					_textAddNum.text = string.Format("x{0}", GetScore());
				}
			}
		}

		public Sprite GetIconSprite()
		{
			if (_imageIcon != null)
			{
				return _imageIcon.sprite;
			}
			return null;
		}
	}
}
