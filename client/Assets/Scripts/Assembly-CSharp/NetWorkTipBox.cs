using System;

public class NetWorkTipBox : DialogBase
{
	public MButton checkButton;

	public MButton closeButton;

	public MButton skipButton;

	private bool _showCloseButton;

	private int _settingTime;

	private static NetWorkTipBox Instance;

	private Action OnCloseCall { get; set; }

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}

	private void Start()
	{
		Instance = this;
		closeButton.gameObject.SetActive(_showCloseButton);
		skipButton.gameObject.SetActive(_showCloseButton);
		checkButton.onClick.AddListener(OnClickCheckButton);
		closeButton.onClick.AddListener(OnClickCloseButton);
		skipButton.onClick.AddListener(OnClickCloseButton);
	}

	private void OnClickCloseButton(MButton mButton)
	{
		_Close(OnCloseCall);
	}

	private void OnClickCheckButton(MButton mButton)
	{
		_settingTime++;
		AdvertisingProxy.Instance.JumpToSetting();
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus && (Singleton<NetworkInspectionManager>.I.IsHaveNetwork() || _settingTime >= 2))
		{
			_Close(OnCloseCall);
		}
	}

	private void Show(bool isDisableGames)
	{
		_showCloseButton = !isDisableGames;
		_Open();
	}

	public static void Create(bool isDisableGames, Action onClose)
	{
		if (!(Instance != null))
		{
			NetWorkTipBox netWorkTipBox = SceneMgr.AddTopNode<NetWorkTipBox>("Prefabs/NetWorkTipBox");
			netWorkTipBox.OnCloseCall = onClose;
			netWorkTipBox.Show(isDisableGames);
		}
	}
}
