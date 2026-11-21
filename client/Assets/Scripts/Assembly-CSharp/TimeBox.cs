using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TimeBox : MonoBehaviour
{
	[SerializeField]
	private Text _timeTxt;

	private Action _callback;

	public void StartCD(int cd, Action callback)
	{
		_callback = callback;
		_timeTxt.DOCounter(cd, 0, cd, Util.Helper.FormatTimerD).OnComplete(OnComplete);
	}

	private void OnComplete()
	{
		if (_callback != null)
		{
			_callback();
		}
	}

	private void OnDestroy()
	{
		if (_timeTxt != null)
		{
			DOTween.Kill(_timeTxt);
		}
	}
}
