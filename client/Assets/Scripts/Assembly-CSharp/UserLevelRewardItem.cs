using UnityEngine;
using UnityEngine.UI;

public class UserLevelRewardItem : MonoBehaviour
{
	[SerializeField]
	private Image _iconImg;

	[SerializeField]
	private Text _valueTex;

	public void Init(int type, int value)
	{
		Global.GetContents((PropID)type, value, "+", out var sprite, out var text);
		_iconImg.sprite = sprite;
		_valueTex.text = text;
	}
}
