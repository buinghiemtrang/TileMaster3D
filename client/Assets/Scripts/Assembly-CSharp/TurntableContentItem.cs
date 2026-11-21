using UnityEngine;
using UnityEngine.UI;

public class TurntableContentItem : MonoBehaviour
{
	[SerializeField]
	private Transform nodeTf;

	[SerializeField]
	private Text valueTex;

	[SerializeField]
	public Image iconImg;

	public void Init(TurntableItemInfo itemInfo)
	{
		iconImg.sprite = LoadMgr.LoadSprite(string.Format("Props/prop{0}", itemInfo.reward[0, 0]));
		valueTex.text = string.Format("+{0}", itemInfo.reward[0, 1]);
		if (itemInfo.reward[0, 0] == 21)
		{
			valueTex.text += "h";
		}
	}
}
