using System;
using GameScene_Level;
using GameScene_Record;
using UnityEngine;

namespace GameScene
{
	public class PauseControl : MonoBehaviour
	{
		[SerializeField]
		private DialogShow pDialog;

		[SerializeField]
		private MButton restartButton;

		[SerializeField]
		private MButton howToPlayButton;

		public Action func_restart;

		public Action func_close;

		private void Awake()
		{
			DispatcherMgr.Emit("GamePause", true, "Pause");
			pDialog.func_callback_close = delegate
			{
				DispatcherMgr.Emit("GamePause", false, "Pause");
				func_close?.Invoke();
			};
			LogEvent.I.Level_GamePause();
		}

		private void Start()
		{
			howToPlayButton.gameObject.SetActive(LevelMgr.Data.Type == LevelType.Ordinary);
			restartButton.gameObject.SetActive(LevelMgr.Data.Type == LevelType.Ordinary);
		}

		public void ClickHowToPlay(MButton button)
		{
			// Show() (0x1188d18) là nhánh đuôi của hàm, đọc từ assembly
			SceneMgr.AddTopNode<GameGuideDialog>("Prefabs/GameGuideDialog").Show();
		}

		public void restartClick(MButton button)
		{
			// <restartClick>b__8_0 và b__8_2 có thân giống hệt nhau
			Action restart = delegate
			{
				Singleton<RecordMgr>.I.Clear(false);
				VictoriesActivityMgr.I.SetData(false);
				DB.Bag.ModifyEnergy(-1, true, "");
				LogEvent.I.Level_GamePauseClose("Replay");
				pDialog._Close(func_restart);
			};
			if (!DB.Bag.JudgeHasEnergy())
			{
				EnergyDialog.Create(restart, delegate
				{
					SceneMgr.AddTopNode<StoreDialog>("Prefabs/StorePopDialog");
				});
			}
			else
			{
				Global.ShowLvInterstitial(delegate
				{
					restart();
				}, INTER_PLACE.AdInterstitial);
			}
		}

		public void homeClick(MButton button)
		{
			Global.ShowLvInterstitial(delegate
			{
				Singleton<RecordMgr>.I.Clear(false);
				VictoriesActivityMgr.I.SetData(false);
				DB.Bag.ModifyEnergy(-1, true, "");
				BoosterMgr.I.SetGear(0, true);
				pDialog._Close(delegate
				{
					BlackCutDialog.Show(delegate
					{
						// ghi isShowPopGift = true là thân inline của GiftDataMgr.ResetShowPopStatus(bool) (strb w1,[x0,#0x20] tại RVA 0x11971e4)
						GiftDataMgr.ins.ResetShowPopStatus(true);
						SceneMgr.LoadScene("MainScene", "GameScene", delegate
						{
							BlackCutDialog.Hide();
						});
					});
					LogEvent.I.Level_Quit(LevelMgr.Data.Type, LevelMgr.Data.IsRegionRankLevel);
					LogEvent.I.Track("Game_Pause_Close").Add("Type", "Home").CommonAttr().Send();
				});
			}, INTER_PLACE.AdInterstitial);
		}

		public void closeClick(MButton button)
		{
			Global.ShowLvInterstitial(delegate
			{
				pDialog.ClickClose(button);
				LogEvent.I.Track("Game_Pause_Close").Add("Type", "Back").CommonAttr().Send();
			}, INTER_PLACE.AdInterstitial);
		}
	}
}
