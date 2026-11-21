using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ExtraFoundation.Components
{
	public class TextMarqueeControl : MonoBehaviour
	{
		[SerializeField]
		private Text ttMsg;

		[SerializeField]
		private int duration = 1;

		private void OnDestroy()
		{
			DOTween.Kill(ttMsg.transform);
		}

		public void SetText(string text)
		{
			ttMsg.text = text;
			LayoutRebuilder.ForceRebuildLayoutImmediate(ttMsg.transform as RectTransform);
			Rect rect = this.Rect();
			Rect rect2 = ttMsg.Rect();
			ttMsg.transform.DOKill();
			if (rect2.width > rect.width)
			{
				ttMsg.transform.SetLocPosX((rect2.width - rect.width) * 0.5f);
			}
		}
	}
}
