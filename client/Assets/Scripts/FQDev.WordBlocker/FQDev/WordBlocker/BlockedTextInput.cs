using UnityEngine;
using UnityEngine.UI;

namespace FQDev.WordBlocker
{
	[RequireComponent(typeof(InputField))]
	public class BlockedTextInput : BlockedInputBase
	{
		private InputField _inputField;

		private void Awake()
		{
			_inputField = GetComponent<InputField>();
			_inputField.onEndEdit.AddListener(CheckValid); // [rebuild] sửa lỗi dịch: gốc là onEndEdit (offset 0x140 = InputField.m_OnDidEndEdit), không phải onValueChanged
		}
	}
}
