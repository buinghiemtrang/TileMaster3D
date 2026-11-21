using UnityEngine;
using UnityEngine.UI;

public class GiftPopItem : MonoBehaviour
{
	[SerializeField]
	private GameObject _root;

	[SerializeField]
	private Image iconIma;

	[SerializeField]
	private Text valueTex;

	public GameObject Root => _root;

	public void Init(int propId, int value)
	{
		Global.GetContents((PropID)propId, value, out Sprite sprite, out string text);
		if ((bool)valueTex)
		{
			valueTex.text = text;
		}
		if ((bool)sprite)
		{
			if ((bool)iconIma)
			{
				iconIma.sprite = sprite;
			}
		}
	}
}
