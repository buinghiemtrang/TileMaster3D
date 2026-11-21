using DG.Tweening;
using GameMagicCube;
using GameScene;
using UnityEngine;

public class GameMagicCubeScene : MonoBehaviour
{
	public CubeControl pCubeCtrl;

	public UpdateScoreControl pScore;

	public TimeProgress pTimeBar;

	public GameStatus kGameStatus;

	public bool isCanPause;

	private void OnDestroy()
	{
		if (DataMgr.I != null)
		{
			DataMgr.I.Destroy();
		}
		Global.HideBanner();
	}

	private void Awake()
	{
		isCanPause = true;
		DataMgr.pMain = this;
		DataMgr.pCubeCtrl = pCubeCtrl;
		pCubeCtrl.transform.SetParent(SceneMgr.tfMainRoot3D, false);
	}

	private void Start()
	{
		DataMgr.I.CreateLevel();
		InitLevelData();
		LogEvent.I.SmallGames_Start_RubikCube(DataMgr.I.pCfg.allModeCount);
		Global.ShowBanner();
	}

	public void InitLevelData()
	{
		kGameStatus = GameStatus.Waiting;
		pCubeCtrl.RestartData();
	}

	public void SetGameStatus(GameStatus _status)
	{
		kGameStatus = _status;
	}

	public void GameFail(string type)
	{
		if (kGameStatus == GameStatus.Victory)
		{
			return;
		}
		LogEvent.I.SmallGames_Fail_RubikCube(DataMgr.I.pCfg.allModeCount, DataMgr.pCubeCtrl.nCubeCount * 3, type);
		kGameStatus = GameStatus.Failure;
		GameParkFailureDialog.Create();
	}

	public void GameVictory(string type)
	{
		LogEvent.I.SmallGames_Quit_RubikCube(DataMgr.I.pCfg.allModeCount, DataMgr.pCubeCtrl.nCubeCount * 3, type);
		GameParkVictoryDialog.Create(pScore.nScore);
	}

	public void ClickClose(MButton btn)
	{
		if (isCanPause && kGameStatus == GameStatus.Playing)
		{
			DOTween.PauseAll();
			kGameStatus = GameStatus.Waiting;
			Global.CreateMsgBoxDialog("Language269", delegate(bool b)
			{
				if (b)
				{
					GameFail("Quit");
				}
				else
				{
					Debug.Log("ShowLvInterstitial");
					Global.ShowLvInterstitial(delegate
					{
						DOTween.PlayAll();
						kGameStatus = GameStatus.Playing;
					}, INTER_PLACE.AdInterstitial);
				}
			}, true, "");
		}
	}
}
