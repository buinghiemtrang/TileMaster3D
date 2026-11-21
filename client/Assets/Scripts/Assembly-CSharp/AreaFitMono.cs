using UnityEngine;

public class AreaFitMono : MonoBehaviour
{
	public bool bUpOffset = true;

	public bool bDownOffset;

	private void Awake()
	{
		RectTransform rectTransform = base.transform as RectTransform;
		float offSetY = GetOffSetY();
		if (bUpOffset && offSetY != 0f)
		{
			float num = rectTransform.offsetMax.x - (rectTransform.anchoredPosition.x + rectTransform.sizeDelta.x * (Vector2.one.x - rectTransform.pivot.x));
			float num2 = rectTransform.offsetMax.y - offSetY - (rectTransform.anchoredPosition.y + rectTransform.sizeDelta.y * (Vector2.one.y - rectTransform.pivot.y));
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x + num, rectTransform.sizeDelta.y + num2);
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x + num * rectTransform.pivot.x, rectTransform.anchoredPosition.y + num2 * rectTransform.pivot.y);
		}
	}

	private bool IsIPad()
	{
		return SystemInfo.deviceModel.ToLower().StartsWith("ipad");
	}

	private float GetOffSetY()
	{
		Camera main = Camera.main;
		int width = Screen.width;
		int height = Screen.height;
		Vector3 vector = main.ScreenToWorldPoint(new Vector3(width, height, 0f));
		Vector3 vector2 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.safeArea.width, Screen.safeArea.height, 0f));
		return (vector.z - vector2.z) * 100f;
	}
}
