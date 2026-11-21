using UnityEngine;
using UnityEngine.UI;

public class TipWithProgress : MonoBehaviour
{
	private Image progressImage;

	private bool canCheckPosition;

	private float wight;

	private void Start()
	{
		progressImage = transform.parent.GetComponent<Image>();
		if (progressImage == null)
		{
			return;
		}
		canCheckPosition = true;
		wight = progressImage.GetComponent<RectTransform>().sizeDelta.x + 20f;
	}

	private void Update()
	{
		if (!canCheckPosition)
		{
			return;
		}
		RectTransform rectTransform = (RectTransform)transform;
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		rectTransform.anchoredPosition = new Vector2(progressImage.fillAmount * wight, anchoredPosition.y);
	}
}
