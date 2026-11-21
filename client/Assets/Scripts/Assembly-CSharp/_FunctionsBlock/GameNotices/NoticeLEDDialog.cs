using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _FunctionsBlock.GameNotices
{
	public class NoticeLEDDialog : MonoBehaviour
	{
		private static NoticeLEDDialog _instance;

		[SerializeField]
		private Text _text;

		[SerializeField]
		private RectTransform _rtfContent;

		private List<NoticeInfo> _ayLEDInfo;

		private void OnDestroy()
		{
			DOTween.Kill(_text);
		}

		private void Next()
		{
			if (_ayLEDInfo.Count != 0)
			{
				SetText(_ayLEDInfo.Pop(0));
				return;
			}
			base.transform.DestroySelf();
			_instance = null;
		}

		public void SetText(NoticeInfo info)
		{
			_text.text = info.content;
			RectTransform rectTransform = _text.transform as RectTransform;
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
			Rect rect = _rtfContent.rect;
			Rect rect2 = rectTransform.rect;
			rectTransform.DOKill();
			_text.transform.SetLocPosX(rect.width);
			rectTransform.DOAnchorPosX(0f - rect2.width, 60f).SetEase(Ease.Linear).SetSpeedBased()
				.OnComplete(delegate
				{
					info.isUsed = true;
					Next();
				});
		}

		public static NoticeLEDDialog Create()
		{
			if ((bool)_instance)
			{
				return null;
			}
			List<NoticeInfo> list = NoticeHttpMgr.I.ayGameLED.FindAll((NoticeInfo info) => !info.isUsed);
			if (list.Count == 0)
			{
				return null;
			}
			_instance = SceneMgr.AddTopNode<NoticeLEDDialog>("NoticeLEDDialog");
			_instance._ayLEDInfo = list;
			_instance.Next();
			return _instance;
		}
	}
}
