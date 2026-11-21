using UnityEngine;
using UnityEngine.UI;

namespace _Game._Functions.LuckyPrize.Scripts
{
	public class LuckyPrizeGameBlockItem
	{
		public readonly Transform tfRoot;

		public readonly Image imgIcon;

		public readonly Transform tfMask;

		public int nKind;

		public readonly int nIndex;

		public bool IsActive { get; set; }

		public LuckyPrizeGameBlockItem(Transform root, int index)
		{
			tfRoot = root;
			nIndex = index;
			tfMask = root.Find("Mask");
			imgIcon = root.FindC<Image>("Icon");
			IsActive = false;
		}

		public void Initialized(bool show, Sprite sprite, int kind)
		{
			nKind = kind;
			IsActive = show;
			imgIcon.gameObject.SetActive(show);
			tfMask.gameObject.SetActive(!imgIcon.gameObject.activeSelf);
			imgIcon.sprite = sprite;
		}

		public void SetIcon()
		{
			imgIcon.sprite = LoadMgr.LoadSprite(string.Format("Activity/Textures/1{0:D2}", nKind));
		}

		public void Eliminate()
		{
			tfRoot.gameObject.SetActive(false);
			IsActive = false;
		}
	}
}
