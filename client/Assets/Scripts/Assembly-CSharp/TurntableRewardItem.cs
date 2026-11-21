using UnityEngine;
using UnityEngine.UI;

public class TurntableRewardItem : MonoBehaviour
{
	public Image iconImg;

	[SerializeField]
	private GameObject rootObj;

	[SerializeField]
	private Text valueTex;

	public void Show(int type, int value)
	{
		rootObj.SetActive(true);
		iconImg.sprite = LoadMgr.LoadSprite(string.Format("Sprites/Icon/prop{0}", type));
		valueTex.text = string.Format("+{0}", value);
	}

	public void Hide()
	{
		rootObj.SetActive(false);
	}
}
