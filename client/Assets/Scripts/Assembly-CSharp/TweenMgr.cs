using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public static class TweenMgr
{
	public static Vector3 vTempVec3 = Vector3.zero;

	private static int __n_call_value = 0;

	public static Vector3 SetVec3(float x, float y, float z = 0f)
	{
		vTempVec3.x = x;
		vTempVec3.y = y;
		vTempVec3.z = z;
		return vTempVec3;
	}

	public static Sequence DialogIn(Transform tf, Vector2 orign)
	{
		CanvasGroup group = tf.GetComponent<CanvasGroup>();
		if (group == null)
		{
			group = tf.gameObject.AddComponent<CanvasGroup>();
		}
		group.interactable = false;
		float x = tf.localPosition.x;
		tf.localPosition = SetVec3(x, orign.y + 200f);
		group.alpha = 0f;
		Sequence sequence = DOTween.Sequence();
		sequence.Append(group.DOFade(1f, 0.2f));
		sequence.Join(tf.DOLocalMove(SetVec3(x, orign.y - 10f), 0.2f));
		sequence.Append(tf.DOLocalMove(SetVec3(x, orign.y), 0.1f));
		sequence.AppendCallback(delegate
		{
			group.interactable = true;
		});
		return sequence;
	}

	public static Sequence DialogOut(Transform tf, Vector2 orign)
	{
		CanvasGroup group = tf.GetComponent<CanvasGroup>();
		if (group == null)
		{
			group = tf.gameObject.AddComponent<CanvasGroup>();
		}
		group.interactable = false;
		float x = tf.localPosition.x;
		tf.localPosition = SetVec3(x, orign.y);
		group.alpha = 1f;
		Sequence sequence = DOTween.Sequence();
		sequence.Append(tf.DOLocalMove(SetVec3(x, orign.y - 10f), 0.1f));
		sequence.Append(group.DOFade(0f, 0.2f));
		sequence.Join(tf.DOLocalMove(SetVec3(x, orign.y + 200f), 0.2f));
		sequence.AppendCallback(delegate
		{
			group.interactable = true;
		});
		return sequence;
	}

	public static void OwnKill(this Tweener tw)
	{
		if (tw != null)
		{
			tw.Kill();
		}
	}

	public static Sequence FadeInMoveTo(this Transform tf, Vector3 pos, float time = 0.2f, bool b_from = false, Sequence sequence = null)
	{
		return FadeMove(tf, pos, b_from, time, 0f, 1f, sequence);
	}

	public static Sequence FadeInMoveBy(this Transform tf, Vector3 off, float time = 0.2f, bool b_from = false, Sequence sequence = null)
	{
		return FadeMove(tf, tf.localPosition + off, b_from, time, 0f, 1f, sequence);
	}

	public static Sequence FadeOutMoveTo(this Transform tf, Vector3 pos, float time = 0.2f, bool b_from = false, Sequence sequence = null)
	{
		return FadeMove(tf, pos, b_from, time, 1f, 0f, sequence);
	}

	public static Sequence FadeOutMoveBy(this Transform tf, Vector3 off, float time = 0.2f, bool b_from = false, Sequence sequence = null)
	{
		return FadeMove(tf, tf.localPosition + off, b_from, time, 1f, 0f, sequence);
	}

	private static Sequence FadeMove(Transform tf, Vector3 pos, bool b_from, float time, float f_alpha_s, float f_alpha_e, Sequence sequence = null)
	{
		CanvasGroup canvasGroup = tf.GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			canvasGroup = tf.gameObject.AddComponent<CanvasGroup>();
		}
		canvasGroup.alpha = f_alpha_s;
		if (sequence == null)
		{
			sequence = DOTween.Sequence();
		}
		Tweener tweener = tf.DOLocalMove(pos, time);
		if (b_from)
		{
			tweener = tweener.From();
		}
		sequence.Append(tweener);
		sequence.Join(canvasGroup.DOFade(f_alpha_e, time));
		return sequence;
	}

	public static Tweener FadeTo(this Component self, float start, float end, float time)
	{
		CanvasGroup canvasGroup = self.GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			canvasGroup = self.gameObject.AddComponent<CanvasGroup>();
		}
		// UNVERIFIED: binary calls DOTweenModuleUI.DOFade(CanvasGroup, startValue, endValue, duration) (RVA 0x1181c00), which is missing from DOTweenModuleUI.cs (outside chunk); expanded here
		canvasGroup.alpha = start;
		return canvasGroup.DOFade(end, time);
	}

	public static Sequence ScaleMinToMax(this Transform tf, float min, float max, float time)
	{
		tf.localScale = SetVec3(min, min, 1f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(tf.DOScale(SetVec3(max + 0.1f, max + 0.1f, 1f), time));
		sequence.AppendInterval(0.1f);
		sequence.Append(tf.DOScale(SetVec3(max - 0.05f, max - 0.05f, 1f), 0.1f));
		sequence.Append(tf.DOScale(SetVec3(max, max, 1f), 0.1f));
		return sequence;
	}

	public static Sequence ScaleMaxToMin(this Transform tf, float max, float min, float time)
	{
		tf.localScale = SetVec3(max, max, 1f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(tf.DOScale(SetVec3(max + 0.1f, max + 0.1f, 1f), 0.05f));
		sequence.AppendInterval(0.1f);
		sequence.Append(tf.DOScale(SetVec3(min, min, 1f), time));
		return sequence;
	}

	public static void DisperseAndFly(Sprite sprite, float scale1, float scale2, int count, Vector2 start, Vector2 end, int dest, float time = 0.8f)
	{
		GameObject gameObject = new GameObject();
		gameObject.transform.SetScale(scale1);
		Image image = gameObject.AddComponent<Image>();
		image.raycastTarget = false;
		image.sprite = sprite;
		for (int i = 0; i <= count; i++)
		{
			Image img = UnityEngine.Object.Instantiate(image);
			img.transform.SetParent(SceneMgr.tfTopRoot, false);
			img.transform.localPosition = new Vector3(start.x, start.y, 0f);
			img.SetAlpha(0f);
			Sequence sequence = DOTween.Sequence();
			sequence.AppendInterval((float)i * 0.1f);
			sequence.Append(img.transform.DOLocalMove(SetVec3(start.x + (float)MMath.Random(-dest, dest), start.y + (float)MMath.Random(-dest, dest)), 0.15f));
			sequence.Join(img.DOFade(1f, 0.15f));
			sequence.AppendInterval(0.1f);
			sequence.Append(img.transform.DOLocalMove(new Vector3(end.x, end.y, 0f), time).SetEase(Ease.InOutQuad));
			sequence.Join(img.transform.DOScale(scale2, time));
			sequence.OnComplete(delegate
			{
				UnityEngine.Object.Destroy(img.gameObject);
			});
		}
		UnityEngine.Object.Destroy(gameObject);
	}

	public static List<BezierInfo> BezierFlyerH(Sprite sprite, float scale, Vector3 p1, Vector3 p2, Vector3 p3, int count, Vector2 off, float fdelay = 1.5f, float interval = 0.01f, Action callback = null)
	{
		GameObject gameObject = new GameObject();
		gameObject.transform.SetScale(scale);
		Image image = gameObject.AddComponent<Image>();
		image.raycastTarget = false;
		image.sprite = sprite;
		image.Hide();
		List<BezierInfo> result = BezierFlyerH(image.transform, p1, p2, p3, count, off, fdelay, interval, callback);
		UnityEngine.Object.Destroy(gameObject);
		return result;
	}

	public static List<BezierInfo> BezierFlyerV(Sprite sprite, float scale, Vector3 p1, Vector3 p2, Vector3 p3, int count, Vector2 off, float fdelay = 1.5f, float interval = 0.01f, Action callback = null)
	{
		GameObject gameObject = new GameObject();
		gameObject.transform.SetScale(scale);
		Image image = gameObject.AddComponent<Image>();
		image.raycastTarget = false;
		image.sprite = sprite;
		image.Hide();
		List<BezierInfo> result = BezierFlyerV(image.transform, p1, p2, p3, count, off, fdelay, interval, callback);
		UnityEngine.Object.Destroy(gameObject);
		return result;
	}

	public static List<BezierInfo> BezierFlyerH(Transform cell, Vector3 p1, Vector3 p2, Vector3 p3, int count, Vector2 off, float fdelay = 1.5f, float interval = 0.01f, Action callback = null)
	{
		return BezierFlyer(cell, p1, p2, p3, count, off, Vector2.zero, fdelay, interval, callback);
	}

	public static List<BezierInfo> BezierFlyerV(Transform cell, Vector3 p1, Vector3 p2, Vector3 p3, int count, Vector2 off, float fdelay = 1.5f, float interval = 0.01f, Action callback = null)
	{
		return BezierFlyer(cell, p1, p2, p3, count, Vector2.zero, off, fdelay, interval, callback);
	}

	public static List<BezierInfo> BezierFlyer(Transform cell, Vector3 p1, Vector3 p2, Vector3 p3, int count, Vector2 offh, Vector2 offv, float fdelay = 1.5f, float interval = 0.01f, Action callback = null)
	{
		List<BezierInfo> list = new List<BezierInfo>();
		for (int i = 0; i < count; i++)
		{
			Transform tran = UnityEngine.Object.Instantiate(cell);
			tran.SetParent(SceneMgr.tfTopRoot, false);
			tran.localPosition = p1;
			Sequence sequence = DOTween.Sequence();
			sequence.AppendInterval((float)i * interval);
			sequence.AppendCallback(tran.Show);
			float delay = UnityEngine.Random.Range(fdelay, fdelay + 0.5f);
			float num = UnityEngine.Random.Range(offh.x, offh.y);
			float num2 = UnityEngine.Random.Range(offv.x, offv.y);
			Tweener t = tran.DOLocalBezier(delay, p1, Utils.Vec3(p2.x + num, p2.y + num2), p3);
			sequence.Append(t);
			sequence.SetEase(Ease.InQuad);
			sequence.OnComplete(delegate
			{
				UnityEngine.Object.Destroy(tran.gameObject);
				if (callback != null)
				{
					callback();
				}
			});
			list.Add(new BezierInfo
			{
				Sequence = sequence,
				Target = tran.gameObject
			});
		}
		return list;
	}

	public static Tween MoveToOff(Transform tf, Vector2 orign, Vector2 off, float time = 0.2f)
	{
		tf.localPosition = new Vector3(orign.x, orign.y, 0f);
		return tf.DOLocalMove(new Vector3(orign.x + off.x, orign.y + off.y, 0f), time);
	}

	public static Tween MoveByOff(Transform tf, Vector2 orign, Vector2 off, float time = 0.2f)
	{
		tf.localPosition = new Vector3(orign.x + off.x, orign.y + off.y, 0f);
		return tf.DOLocalMove(new Vector3(orign.x, orign.y, 0f), time);
	}

	public static Tween DOWidth(Transform tf, float f_start, float f_end, float time = 0.2f)
	{
		return DOTween.To(delegate(float v)
		{
			RectTransform rectTransform = (RectTransform)tf;
			Vector2 sizeDelta = rectTransform.sizeDelta;
			// UNVERIFIED: pseudo-C shows get/set sizeDelta but not which axis is written (x assumed for width)
			sizeDelta.x = v;
			rectTransform.sizeDelta = sizeDelta;
		}, f_start, f_end, time);
	}

	public static Tween DOHeight(Transform tf, float f_start, float f_end, float time = 0.2f)
	{
		return DOTween.To(delegate(float v)
		{
			RectTransform rectTransform = (RectTransform)tf;
			Vector2 sizeDelta = rectTransform.sizeDelta;
			// UNVERIFIED: pseudo-C shows get/set sizeDelta but not which axis is written (y assumed for height)
			sizeDelta.y = v;
			rectTransform.sizeDelta = sizeDelta;
		}, f_start, f_end, time);
	}

	public static TweenerCore<int, int, NoOptions> DelayCall(float delay, TweenCallback callback)
	{
		return DOTween.To(() => __n_call_value, delegate(int v)
		{
			__n_call_value = v;
		}, 0, delay).OnComplete(callback);
	}

	public static TweenerCore<int, int, NoOptions> NumberTo(float delay, int nStart, int nEnd, DOSetter<int> callback = null)
	{
		int? __value = null;
		return DOTween.To(() => nStart, delegate(int v)
		{
			if (callback != null && (!__value.HasValue || __value.Value != v))
			{
				__value = v;
				callback(v);
			}
		}, nEnd, delay);
	}

	public static Tweener NumberTo(float delay, float fStart, float fEnd, DOSetter<float> callback = null)
	{
		return DOTween.To(delegate(float v)
		{
			if (callback != null)
			{
				callback(v);
			}
		}, fStart, fEnd, delay);
	}
}
