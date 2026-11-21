using UnityEngine;
using UnityEngine.UI;

public class GMButton : MonoBehaviour
{
	[SerializeField]
	private DialogShow _settingsDialog;

	private void Awake()
	{
		// [rebuild] 0x12ee4ac: icall GameObject.SetActive(false) — nút GM luôn ẩn ở bản phát hành
		gameObject.SetActive(false);
	}

	private void Start()
	{
		Button component = GetComponent<Button>();
		if (component != null)
		{
			component.onClick.AddListener(OnButtonClick);
		}
	}

	private void OnButtonClick()
	{
		GMDialog gMDialog = SceneMgr.AddTopNode<GMDialog>("GMDialog");
		gMDialog.SettingsDialog = _settingsDialog;
	}
}
