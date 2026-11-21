using UnityEngine;

public class GoldControl : UpdateProp
{
	[SerializeField]
	private Transform addEff;

	public Transform goldIcon;

	public void Add()
	{
		if (!addEff.gameObject.activeSelf)
		{
			addEff.gameObject.SetActive(true);
		}
		addEff.GetComponent<Animator>().Play("target-ani", -1, 0f);
	}
}
