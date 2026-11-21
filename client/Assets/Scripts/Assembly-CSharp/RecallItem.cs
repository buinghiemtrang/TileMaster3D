using UnityEngine;
using UnityEngine.UI;

public class RecallItem : MonoBehaviour
{
	[SerializeField]
	private Image iconIma;

	[SerializeField]
	private Text valueTex;

	public void Init(int propId, int value)
	{
		if (propId == (int)PropID.Medal)
		{
			string text = string.Format("Props/medal{0}", value);
			iconIma.sprite = LoadMgr.LoadSprite(text);
			valueTex.text = "x1";
		}
		else
		{
			string text2 = string.Format("Props/prop{0}", propId);
			iconIma.sprite = LoadMgr.LoadRes<Sprite>(text2);
			if (propId == (int)PropID.EnergyTime)
			{
				valueTex.text = string.Format("{0}h", value);
			}
			else
			{
				valueTex.text = string.Format("x{0}", value);
			}
		}
	}
}
