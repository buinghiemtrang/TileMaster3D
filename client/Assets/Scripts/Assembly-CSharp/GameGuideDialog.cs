using System;
using DG.Tweening;
using UnityEngine;

public class GameGuideDialog : DialogBase
{
	[SerializeField]
	private MButton closeButton;

	[SerializeField]
	private MButton trueButton;

	public Action CloseCallback;

	private Vector3 _endPosition;

	private bool _isNormalClose;

	public GameGuideDialog()
	{
		fMaskAlpha = 0.8f;
	}

	protected override void Awake()
	{
		base.Awake();
		closeButton.onClick.AddListener(_ClickClose);
		trueButton.onClick.AddListener(_ClickClose);
	}

	public void Show(Vector3 position)
	{
		_isNormalClose = false;
		_endPosition = base.transform.InverseTransformPoint(position);
		_Open();
	}

	public void Show()
	{
		_isNormalClose = true;
		_Open();
	}

	private void _ClickClose(MButton button)
	{
		if (_isNormalClose)
		{
			_Close();
			return;
		}
		HideMask();
		tfRoot.DOKill();
		tfRoot.DOScale(0.05f, 0.3f).SetEase(Ease.Linear);
		tfRoot.DOLocalMove(_endPosition, 0.3f).SetEase(Ease.Linear).OnComplete(delegate
		{
			SceneMgr.RemoveDialog(base.gameObject);
			this.DestroySelf();
			CloseCallback.InvokeSafe();
		});
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (tfRoot != null)
		{
			DOTween.Kill(tfRoot);
		}
	}
}
