using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class LimitStringLengthByByteCount : MonoBehaviour
{
	public int maxLen = 79;

	private InputField inf;

	private void Awake()
	{
		inf = GetComponent<InputField>();
		inf.onValueChanged.AddListener(_ValueChanged);
	}

	private void _ValueChanged(string va)
	{
		string text = va.TrimStart();
		if (!string.IsNullOrEmpty(text))
		{
			char[] array = text.ToCharArray();
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			for (int i = 0; i < array.Length; i++)
			{
				int byteCount = Encoding.UTF8.GetByteCount(array, i, 1);
				int num2 = ((byteCount != 3) ? byteCount : 2);
				num += num2;
				if (num > maxLen)
				{
					break;
				}
				stringBuilder.Append(array[i]);
			}
			text = stringBuilder.ToString();
		}
		inf.text = text;
	}
}
