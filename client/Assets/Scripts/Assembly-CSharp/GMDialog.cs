using UnityEngine;
using UnityEngine.UI;

public class GMDialog : DialogBase
{
	[SerializeField]
	private GameObject _content;

	[SerializeField]
	private GameObject _inputFile;

	[SerializeField]
	private Text _des;

	[SerializeField]
	private InputField _input1;

	[SerializeField]
	private InputField _input2;

	[SerializeField]
	private GameObject _ordinaryRoot;

	[SerializeField]
	private GameObject _userLabelRoot;

	[SerializeField]
	private Text _title1;

	[SerializeField]
	private Text _title2;

	[SerializeField]
	private bool _isLabelChangedReloading;

	private GMInfo _gmItemInfo;

	private bool _userLabelChanged;

	public DialogShow SettingsDialog;

	protected override void Awake()
	{
	}

	protected override void OnDestroy()
	{
	}

	private void OnUserLabelChange()
	{
	}

	public void CloseDialog()
	{
	}

	private void UpdateViewInfo(int index)
	{
	}

	public void SetInfo()
	{
	}
}
