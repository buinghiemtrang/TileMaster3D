using UnityEngine;

public class AchiveStarItem : MonoBehaviour
{
	public GameObject starIcon;

	public void SetShow(bool bShow)
	{
		starIcon.SetActive(bShow);
	}
}
