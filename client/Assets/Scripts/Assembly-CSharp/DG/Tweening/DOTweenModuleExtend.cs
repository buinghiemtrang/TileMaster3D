using System;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace DG.Tweening
{
	public static class DOTweenModuleExtend
	{
		public static TweenerCore<int, int, NoOptions> DOCounter(this Text target, int start, int end, float duration, Func<int, string> callback)
		{
			int? _value = null;
			return DOTween.To(() => start, delegate(int v)
			{
				if (_value.HasValue && _value.GetValueOrDefault() == v)
				{
					return;
				}
				_value = v;
				target.text = callback(v);
			}, end, duration).SetEase(Ease.Linear).SetTarget(target);
		}

		public static Tweener DOCounter(this Text target, float start, float end, float duration, Func<float, string> callback)
		{
			return DOTween.To(delegate(float v)
			{
				target.text = callback(v);
			}, start, end, duration).SetEase(Ease.Linear).SetTarget(target);
		}

		public static TweenerCore<int, int, NoOptions> DOCounter(this Component target, int start, int end, float duration, Action<int> callback = null)
		{
			int? _value = null;
			return DOTween.To(() => start, delegate(int v)
			{
				if (_value.HasValue && _value.GetValueOrDefault() == v)
				{
					return;
				}
				_value = v;
				callback?.Invoke(v);
			}, end, duration).SetEase(Ease.Linear).SetTarget(target);
		}

		public static Tweener DOCounter(this Component target, float start, float end, float duration, Action<float> callback = null)
		{
			return DOTween.To(delegate(float v)
			{
				callback?.Invoke(v);
			}, start, end, duration).SetEase(Ease.Linear).SetTarget(target);
		}

		private static int __delay_call_get()
		{
			return 0;
		}

		private static void __delay_call_set(int v)
		{
		}

		public static TweenerCore<int, int, NoOptions> DelayCall(this Component target, float delay, TweenCallback callback)
		{
			return DOTween.To(__delay_call_get, __delay_call_set, 0, 0f).SetDelay(delay).SetTarget(target).OnComplete(callback);
		}

		public static TweenerCore<int, int, NoOptions> DelayCall(float delay, TweenCallback callback)
		{
			return DOTween.To(__delay_call_get, __delay_call_set, 0, 0f).SetDelay(delay).OnComplete(callback);
		}

		public static TweenerCore<int, int, NoOptions> Delay(float delay)
		{
			return DOTween.To(__delay_call_get, __delay_call_set, 0, 0f).SetDelay(delay);
		}

		public static void InvokeUntilSuccess(Action<TweenCallback> callback, float failDelay, int failLoop = -1)
		{
			int loop = failLoop;
			void FailFunc()
			{
				if (loop > 0)
				{
					loop--;
				}
				if (loop != 0)
				{
					DelayCall(failDelay, delegate
					{
						callback(FailFunc);
					});
				}
			}
			callback(FailFunc);
		}
	}
}
