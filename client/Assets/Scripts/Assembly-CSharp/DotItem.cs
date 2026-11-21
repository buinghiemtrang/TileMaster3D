using UnityEngine;

public class DotItem : MonoBehaviour
{
	[SerializeField]
	private GameObject _onRoot;

	internal void SetShowState(bool isShow)
	{
		_onRoot.SetActive(isShow);
	}
}
