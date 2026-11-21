using TMPro;
using UnityEngine;

namespace FQDev.WordBlocker
{
	[RequireComponent(typeof(TMP_InputField))]
	public class BlockedTMPInput : BlockedInputBase
	{
		private TMP_InputField _inputField;

		private void Awake()
		{
			_inputField = GetComponent<TMP_InputField>();
			_inputField.onEndEdit.AddListener(CheckValid); // [rebuild] sửa lỗi dịch: gốc là onEndEdit (offset 0x1a0 = TMP_InputField.m_OnEndEdit), không phải onValueChanged
		}
	}
}
