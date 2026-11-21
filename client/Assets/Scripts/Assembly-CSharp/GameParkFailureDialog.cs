using UnityEngine.UI;

public class GameParkFailureDialog : DialogBase
{
	public Text goldTxt;

	public MButton okBtn;

	public static GameParkFailureDialog Create()
	{
		return SceneMgr.AddTopNode<GameParkFailureDialog>("Prefabs/GameParkFailureDialog");
	}

	private void Start()
	{
		_Open();
		goldTxt.text = "+0";
		okBtn.onClick.AddListener(okBtnClick);
	}

	public void okBtnClick(MButton button)
	{
		button.interactable = false;
		_Close();
		_home();
	}

	private void _home()
	{
		Global.ShowLvInterstitial(delegate
		{
			BlackCutDialog.Show(delegate
			{
				AudioMgr.I.ResumeMusic();
				GiftDataMgr.ins.ResetShowPopStatus(true);
				SceneMgr.LoadScene("MainScene", "GameMagicCubeScene", delegate
				{
					BlackCutDialog.Hide();
				});
			});
		}, INTER_PLACE.AdInterstitial);
	}
}
