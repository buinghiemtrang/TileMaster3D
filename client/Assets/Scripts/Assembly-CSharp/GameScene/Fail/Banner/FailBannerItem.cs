using System;
using DG.Tweening;
using Util;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene.Fail.Banner
{
	public abstract class FailBannerItem : MonoBehaviour
	{
		[SerializeField]
		private GameObject _offTagObj;

		[SerializeField]
		private Text _offTagText;

		internal const string CountdownEndEventKey = "FailBannerCountdownEnd";

		internal abstract void Init(FailBanner banner);

		protected virtual void CountdownFixedFormat(int second, Text timeTex, bool isSendEndEvent, Action endCallback = null)
		{
			timeTex.DOKill();
			if (second > 0)
			{
				timeTex.DOCounter(second, 0, second, Helper.FormatTimer).OnComplete(delegate
				{
					endCallback?.Invoke();
					if (isSendEndEvent)
					{
						DispatcherMgr.Emit("FailBannerCountdownEnd");
					}
				});
				return;
			}
			timeTex.text = "00:00:00";
			endCallback?.Invoke();
			if (isSendEndEvent)
			{
				DispatcherMgr.Emit("FailBannerCountdownEnd");
			}
		}

		protected virtual void SetOffTag(int discount)
		{
			if (!(_offTagObj == null))
			{
				_offTagObj.SetActive(discount > 0);
				if (discount > 0)
				{
					_offTagText.text = string.Format("{0}%", discount);
				}
			}
		}
	}
}
