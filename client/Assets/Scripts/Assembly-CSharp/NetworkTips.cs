using UnityEngine;

public class NetworkTips : MonoBehaviour
{
	private GameObject _gameObject;

	protected virtual void Awake()
	{
		_gameObject = base.gameObject;
		_gameObject.SetActive(value: false);
	}

	public void Show()
	{
		if (!_gameObject.activeSelf)
		{
			_gameObject.SetActive(value: true);
		}
	}

	public void Hide()
	{
		if (_gameObject.activeSelf)
		{
			_gameObject.SetActive(value: false);
		}
	}
}
