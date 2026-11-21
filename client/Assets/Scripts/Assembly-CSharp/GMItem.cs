using UnityEngine;
using UnityEngine.UI;

public class GMItem : MonoBehaviour
{
	[SerializeField]
	private Text _title;

	[SerializeField]
	private GameObject _mask;

	[SerializeField]
	private Toggle _toggle;

	[SerializeField]
	private GMType _gMType;

	private GMInfo _gmInfo;

	private int _index;

	private void Start()
	{
	}

	public void SetInfo(GMType gMType)
	{
	}

	private void OnSelected(bool isOn)
	{
	}
}
