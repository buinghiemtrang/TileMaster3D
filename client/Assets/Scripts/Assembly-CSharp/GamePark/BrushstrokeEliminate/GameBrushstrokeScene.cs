using System.Linq;
using GamePark.BrushstrokeEliminate.BaseData;
using GameScene;
using UnityEngine;

namespace GamePark.BrushstrokeEliminate
{
	public class GameBrushstrokeScene : MonoBehaviour
	{
		[SerializeField]
		private ElementControl pElementControl;

		[SerializeField]
		private Transform tfMainRoot3D;

		public UpdateScoreControl pScore;

		public TimeProgress pTime;

		private void OnDestroy()
		{
			tfMainRoot3D.DestroySelf();
			ElementMgr.I.Destroy();
			DataMgr.I.Destroy();
			Global.HideBanner();
		}

		private void Awake()
		{
			DataMgr.I.pMain = this;
			tfMainRoot3D.SetParent(SceneMgr.tfMainRoot3D, false);
		}

		private void Start()
		{
			DataMgr.I.kGameStatus = GameStatus.None;
			DataMgr.I.Level.CreateLevel();
			pScore.SetScore(0);
			pElementControl.InitDataOne();
			LogEvent.I.SmallGames_Start_LinkDots(DataMgr.I.Level.TargetDatas);
			Global.ShowBanner();
			int num = DataMgr.I.Level.TargetDatas.Sum((TargetData data) => data.Count);
			int num2 = DataMgr.I.cfg.TotalTimeFactor * num;
			pTime.InitTime(num2, num2, delegate
			{
				GameFail("Time");
			});
			pTime.Play();
		}

		public void GameFail(string type)
		{
			if (DataMgr.I.kGameStatus != GameStatus.Failure && DataMgr.I.kGameStatus != GameStatus.Victory)
			{
				DataMgr.I.kGameStatus = GameStatus.Failure;
				pTime.Pause();
				LogEvent.I.SmallGames_Fail_LinkDots(DataMgr.I.Level.TargetDatas, DataMgr.I.Level.CurLayerIndex, type);
				GameParkFailureDialog.Create();
			}
		}

		public void GameVictory(string type)
		{
			if (DataMgr.I.kGameStatus != GameStatus.Failure && DataMgr.I.kGameStatus != GameStatus.Victory)
			{
				DataMgr.I.kGameStatus = GameStatus.Victory;
				pTime.Pause();
				LogEvent.I.SmallGames_Quit_LinkDots(DataMgr.I.Level.TargetDatas, DataMgr.I.Level.CurLayerIndex, type);
				GameParkVictoryDialog.Create(pScore.nScore);
			}
		}

		public void ClickBtnClose(MButton button)
		{
			if (!DataMgr.I.IsCanPause || DataMgr.I.kGameStatus != GameStatus.Playing)
			{
				return;
			}
			pTime.Pause();
			DataMgr.I.kGameStatus = GameStatus.Waiting;
			Global.CreateMsgBoxDialog("Language269", delegate(bool bSure)
			{
				if (bSure)
				{
					GameFail("Quit");
				}
				else
				{
					Global.ShowLvInterstitial(delegate
					{
						pTime.Play();
						DataMgr.I.kGameStatus = GameStatus.Playing;
					}, INTER_PLACE.AdInterstitial);
				}
			}, true, "");
		}
	}
}
