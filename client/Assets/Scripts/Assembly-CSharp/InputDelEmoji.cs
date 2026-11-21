using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class InputDelEmoji : MonoBehaviour
{
	private List<string> patterns = new List<string>();

	private InputField inputField;

	private void Awake()
	{
		patterns.Add("\\p{Cs}");
		patterns.Add("\\p{Co}");
		patterns.Add("\\p{Cn}");
		patterns.Add("[\\u2702-\\u27B0]");
		inputField = GetComponent<InputField>();
		inputField.onValidateInput = OnValidateInput;
		inputField.onEndEdit.AddListener(delegate(string arg0)
		{
			inputField.textComponent.text = arg0;
		});
		inputField.onValueChanged.AddListener(delegate(string arg0)
		{
			inputField.textComponent.text = arg0;
		});
	}

	private char OnValidateInput(string text, int charIndex, char addedChar)
	{
		if (patterns.Count > 0 && IsEmoji(addedChar.ToString()))
		{
			return '\0';
		}
		return addedChar;
	}

	private bool IsEmoji(string str)
	{
		for (int i = 0; i < patterns.Count; i++)
		{
			if (Regex.IsMatch(str, patterns[i]))
			{
				return true;
			}
		}
		return false;
	}
}
