using UnityEngine;
using UnityEngine.UI;

public class RewardItem : MonoBehaviour
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private Text valueTex;

	[SerializeField]
	private GameObject vipIcon;

	[SerializeField]
	private GameObject hookIcon;

	public void Init(bool isVipReward, int pType, int va, bool isLight)
	{
		if (vipIcon != null)
		{
			vipIcon.SetActive(isVipReward);
		}
		if (pType == 23)
		{
			icon.sprite = Resources.Load<Sprite>(string.Format("Props/medal{0}", va));
			valueTex.text = "x 1";
		}
		else
		{
			icon.sprite = Resources.Load<Sprite>(string.Format("Props/prop{0}", pType));
			if (pType == 21)
			{
				valueTex.text = string.Format("{0}h", va);
			}
			else
			{
				valueTex.text = string.Format("x{0}", va);
			}
		}
		icon.SetNativeSize();
	}

	public void SetInfo(int nType, int value, bool isGet = false)
	{
		_SetReward((PropID)nType, value);
		if (hookIcon != null)
		{
			hookIcon.SetActive(isGet);
		}
	}

	private void _SetReward(PropID pType, int value)
	{
		if (pType == PropID.Medal)
		{
			icon.sprite = Resources.Load<Sprite>(string.Format("Props/medal{0}", value));
			valueTex.text = "x 1";
		}
		else
		{
			icon.sprite = Resources.Load<Sprite>(string.Format("Props/prop{0}", (int)pType));
			if (pType == PropID.EnergyTime)
			{
				valueTex.text = string.Format("{0}h", value);
			}
			else
			{
				valueTex.text = ((value != 0) ? string.Format("x{0}", value) : "");
			}
		}
	}
}
