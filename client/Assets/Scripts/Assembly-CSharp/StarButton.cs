using System;
using UnityEngine;
using UnityEngine.UI;

public class StarButton : MonoBehaviour
{
	public int index;

	public Action<int> callback;

	private Image starImage;

	private void Start()
	{
		GetComponent<MButton>().onClick.AddListener(OnButtonClick);
		starImage = transform.Find("Image").GetComponent<Image>();
	}

	private void OnButtonClick(MButton mButton)
	{
		callback?.Invoke(index);
	}

	public void ShowStar(Sprite sprite)
	{
		if (sprite != null)
		{
			starImage.sprite = sprite;
		}
	}
}
