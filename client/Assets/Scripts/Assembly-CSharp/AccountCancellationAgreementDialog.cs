using System;
using TMPro;
using UnityEngine;

public class AccountCancellationAgreementDialog : DialogBase
{
	private class Agreement
	{
		public string Value;
	}

	[SerializeField]
	private MButton _agreeButton;

	[SerializeField]
	private MButton _closeButton;

	[SerializeField]
	private TextMeshProUGUI _contentsTex;

	private Action<bool> _closeCallback;

	public AccountCancellationAgreementDialog()
	{
		fMaskAlpha = 0.8f;
	}

	protected override void Awake()
	{
		base.Awake();
		_agreeButton.onClick.AddListener(OnClickAgree);
		_closeButton.onClick.AddListener(OnClickClose);
	}

	private void Start()
	{
		string text = LoadMgr.LoadText("Json/AccountCancellationAgreement", ".json");
		if (text != null)
		{
			text = text.Replace("\\u3000", "　").Replace("\\n", "\n");
			_contentsTex.text = text;
			_Open();
		}
	}

	private void OnClickAgree(MButton button)
	{
		_Close(delegate
		{
			_closeCallback?.Invoke(obj: true);
		});
	}

	private void OnClickClose(MButton button)
	{
		_Close(delegate
		{
			_closeCallback?.Invoke(obj: false);
		});
	}

	public static AccountCancellationAgreementDialog Create(Action<bool> closeCallback)
	{
		AccountCancellationAgreementDialog accountCancellationAgreementDialog = SceneMgr.AddTopNode<AccountCancellationAgreementDialog>("Prefabs/AccountCancellationAgreementDialog");
		accountCancellationAgreementDialog._closeCallback = closeCallback;
		return accountCancellationAgreementDialog;
	}
}
