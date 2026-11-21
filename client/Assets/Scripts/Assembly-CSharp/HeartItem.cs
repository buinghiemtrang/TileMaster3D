using UnityEngine;

public class HeartItem : MonoBehaviour
{
	public GameObject ImageIcon;

	public GameObject ImageBg;

	public void SetShow(bool bShow)
	{
		if (ImageIcon != null)
		{
			ImageIcon.SetActive(bShow);
		}
		if (ImageBg == null)
		{
			return;
		}
		ImageBg.SetActive(!bShow);
	}
}
