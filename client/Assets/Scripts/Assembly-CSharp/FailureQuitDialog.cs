using UnityEngine;

public class FailureQuitDialog : DialogBase
{
	[SerializeField]
	private MButton restartBtn;

	[SerializeField]
	private MButton continueBtn;

	[SerializeField]
	private MButton quitBtn;

	[SerializeField]
	private GameObject timeOutObj;

	[SerializeField]
	private GameObject noSpaceObj;

	private bool _isTimeOut;

	public FailureQuitDialog()
	{
		fMaskAlpha = 0.8f;
	}

	private void Start()
	{
		restartBtn.onClick.AddListener(OnRestartClick);
		continueBtn.onClick.AddListener(OnQuitClick);
		quitBtn.onClick.AddListener(OnQuitClick);
		timeOutObj.SetActive(_isTimeOut);
		noSpaceObj.SetActive(!_isTimeOut);
	}

	private void OnRestartClick(MButton arg0)
	{
		if (!DB.Bag.JudgeHasEnergy())
		{
			EnergyDialog.Create(DoRestart, delegate
			{
				SceneMgr.AddTopNode<StoreDialog>("Prefabs/StorePopDialog");
			});
		}
		else
		{
			DoRestart();
		}
	}

	private void OnQuitClick(MButton arg0)
	{
		DoGoHome();
	}

	private void DoGoHome()
	{
		DB.User.BombModelProbability = DB.User.BombModelProbability + 20;
		BoosterMgr.I.SetGear(0, isDirect: true);
		Global.ShowLvInterstitial(delegate
		{
			_Close();
			BlackCutDialog.Show(delegate
			{
				AudioMgr.I.ResumeMusic();
				// ghi isShowPopGift = true là thân inline của GiftDataMgr.ResetShowPopStatus(bool) (asm 0x11c24dc: get_ins rồi ghi field)
				GiftDataMgr.ins.ResetShowPopStatus(true);
				SceneMgr.LoadScene("MainScene", "GameScene", delegate
				{
					BlackCutDialog.Hide();
				});
			});
		}, INTER_PLACE.AdInterstitial);
	}

	private void DoRestart()
	{
		Global.ShowLvInterstitial(delegate
		{
			_Close(delegate
			{
				GameScene.GameMgr.I.Init(isReset: true);
			});
		}, INTER_PLACE.AdInterstitial);
	}

	public static FailureQuitDialog Create(bool isTimeOut)
	{
		VictoriesActivityMgr.I.SetData(false);
		DB.Bag.ModifyEnergy(-1, true, "");
		FailureQuitDialog failureQuitDialog = SceneMgr.AddTopNode<FailureQuitDialog>("Prefabs/FailureQuitDialog");
		if (failureQuitDialog != null)
		{
			failureQuitDialog._isTimeOut = isTimeOut;
		}
		return failureQuitDialog;
	}
}
