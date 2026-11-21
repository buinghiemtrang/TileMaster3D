using UnityEngine;
using UnityEngine.UI;

public class OrdinaryRewardItem : MonoBehaviour
{
	[SerializeField]
	private Text valueTex;

	[SerializeField]
	private Image iconImg;

	public void Init(PropID type, int value)
	{
		Sprite sprite;
		string text;
		Global.GetContents(type, value, out sprite, out text);
		valueTex.text = text;
		if (sprite != null)
		{
			iconImg.sprite = sprite;
		}
	}
}
