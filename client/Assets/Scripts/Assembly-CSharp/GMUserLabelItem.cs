using System;
using UnityEngine;
using UnityEngine.UI;

public class GMUserLabelItem : MonoBehaviour
{
	[SerializeField]
	private Text _userLabelText;

	[SerializeField]
	private InputField _valueInputField;

	[SerializeField]
	private Button _changeButton;

	private string _userLabelKey;

	private string _valueString;

	private Action<string, string, bool> _changeAction;

	private void Awake()
	{
	}

	public void InitData(string key, object value, Action<string, string, bool> changeAction)
	{
	}

	private void OnChangeButtonClick()
	{
	}

	public void OnAllChangeClick()
	{
	}
}
