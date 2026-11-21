using System;
using GameScene;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene_Function
{
	public class FreezeView : MonoBehaviour
	{
		[SerializeField]
		private GameObject rootObj;

		[SerializeField]
		private Image freezeImg;

		private void Awake()
		{
			DispatcherMgr.On<GameStatus>("GameStatus_Change", _ChangeStatus);
		}

		private void OnDestroy()
		{
			this.DOKill();
			DispatcherMgr.Off<GameStatus>("GameStatus_Change", _ChangeStatus);
		}

		public void Init()
		{
			this.DOKill();
			freezeImg.DOKill();
			rootObj.SetActive(false);
		}

		private void _ChangeStatus(GameStatus status)
		{
			if (status != GameStatus.None)
			{
				if (status == GameStatus.Playing)
				{
					DOTween.Play(this);
				}
				else
				{
					DOTween.Pause(this);
				}
			}
		}

		public void StartFreeze()
		{
			freezeImg.DOKill();
			rootObj.SetActive(true);
			freezeImg.SetAlpha(0f);
			freezeImg.DOFade(1f, 0.3f);
		}

		public void EndFreeze(bool isUseAnim)
		{
			freezeImg.DOKill();
			if (isUseAnim)
			{
				freezeImg.DOFade(1f, 0.3f).OnComplete(delegate
				{
					rootObj.SetActive(false);
				});
			}
			else
			{
				rootObj.SetActive(false);
			}
		}

		public void Countdown(int duration, Action completed)
		{
			this.DOKill();
			this.DOCounter(duration, 0f, duration).OnComplete(completed.Invoke); // UNVERIFIED: target của TweenCallback bị decompiler mất, giả định completed.Invoke
		}
	}
}
