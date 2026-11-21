using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Function_Reward_Video;
using Game.Activity.BakingMaster;
using GameScene_Element;
using GameScene_Function;
using GameScene_Guide;
using GameScene_Level;
using GameScene_Record;
using _FunctionsBlock.GameNotices;
using UnityEngine;

namespace GameScene
{
	public class GameMgr : MonoBehaviour
	{
		[SerializeField]
		private RootView rootView;

		[SerializeField]
		private FreezeView freezeView;

		[SerializeField]
		private ElementGuideView elementGuideView;

		[SerializeField]
		private UseRewardLayer _useRewardLayer;

		[SerializeField]
		private GameStageProgress _GameStageProgress;

		public TimeControl timeControl;

		public ModelChestControl chestControl;

		public PropControl propControl;

		public ComboControl comboControl;

		public RegionRankGameScoreLayer regionRankGameScoreLayer;

		public AddGoldControl AddGoldControl;

		public AddStarControl AddStarControl;

		public AddTimeControl AddTimeControl;

		public DestroyControl DestroyControl;

		public FillControl FillControl;

		public FreezeControl FreezeControl;

		public RandomHintControl RandomHintControl;

		public RefreshControl RefreshControl;

		private int _waitingNumber;

		private int _popDialogCount;

		public GameStatus GameStatus { get; private set; }

		public static GameMgr I { get; private set; }

		private void Awake()
		{
			I = this;
			GameStatus = GameStatus.None;
			AddGoldControl = new AddGoldControl();
			AddStarControl = new AddStarControl();
			AddTimeControl = new AddTimeControl();
			DestroyControl = new DestroyControl();
			FillControl = new FillControl();
			FreezeControl = new FreezeControl(freezeView);
			RandomHintControl = new RandomHintControl();
			RefreshControl = new RefreshControl();
			Singleton<RewardVideoManager>.I.AddListen();
			DispatcherMgr.On<bool, string>("GamePause", GamePause);
			DispatcherMgr.On("GameTime_End", OnGameTimeEnd);
			DispatcherMgr.On("GameScene_Platform_Fall", OnPlatformFall);
			DispatcherMgr.On("GameScene_Elements_DestroyAll", OnDestroyAllElement);
			DispatcherMgr.On<ElementType>("GameScene_Platform_Fuse_Completed", OnFuseCompleted);
		}

		private void OnDestroy()
		{
			RefreshControl.Destroy();
			DispatcherMgr.Off<bool, string>("GamePause", GamePause);
			DispatcherMgr.Off("GameScene_Platform_Fall", OnPlatformFall);
			DispatcherMgr.Off("GameScene_Elements_DestroyAll", OnDestroyAllElement);
			DispatcherMgr.Off("GameTime_End", OnGameTimeEnd);
			DispatcherMgr.Off<ElementType>("GameScene_Platform_Fuse_Completed", OnFuseCompleted);
			I = null;
		}

		public void NewLevel(LevelType levelType, int groupId, int level)
		{
			ReloadConfig();
			LevelMgr.SetLevelData(levelType, groupId, level);
			// Pseudo-C của NewLevel nối thân Init(false, false) (ReloadConfig đã gọi ở trên)
			Init(false, false);
		}

		private void ReloadConfig()
		{
			if (!Global.NeedReloadConfig || DB.User.Maxlv + 1 < 2)
			{
				return;
			}
			Global.NeedReloadConfig = false;
			UserLabelManager.Instance.Initialized();
			DataCfg.Init();
			SceneLoading.InitData();
			Global.UpdatePlayerDate();
			// thân StoresMgr.ResetStorePrice() được inline vào cuối hàm
			Global.pStores.ResetStorePrice();
		}

		public void Init(bool isReset, bool isLoadConfig = true)
		{
			if (isLoadConfig)
			{
				ReloadConfig();
			}
			NoticeHttpMgr.I.RequestNotice();
			if (isReset)
			{
				Singleton<RecordMgr>.I.SaveFixed();
				BoosterMgr.I.SetGear(0, true);
				LevelMgr.Data.Restart();
			}
			GameStatus = GameStatus.None;
			_waitingNumber = 0;
			_popDialogCount = 0;
			Global.ResetAdNum();
			Global.ShowBanner();
			LevelInterAd.Reset();
			DB.User.UseGoldReviveNumberTimes = 0;
			int newlv = DB.User.Newlv;
			int curlv = DB.User.Curlv;
			if (newlv != curlv)
			{
				DB.User.Newlv = DB.User.Curlv;
			}
			LevelMgr.Data.Difficulty.SetIsNewLvState(newlv != curlv);
			int totalTime = (Singleton<RecordMgr>.I.IsLoading ? Singleton<RecordMgr>.I.Fixed.Time : LevelMgr.Data.Time);
			timeControl.Init(isReset, totalTime, Singleton<RecordMgr>.I.IsLoading ? Singleton<RecordMgr>.I.Data.ResidueTime : LevelMgr.Data.Time);
			rootView.Init();
			chestControl.Init();
			propControl.Init();
			comboControl.Init();
			AddGoldControl.Init();
			AddStarControl.Init();
			AddTimeControl.Init();
			DestroyControl.Init();
			FillControl.Init();
			FreezeControl.Init();
			RandomHintControl.Init();
			RefreshControl.Init();
			ElementGuideMgr.I.Init(elementGuideView);
			BoundaryMgr.I.Init(rootView.gameBoundary);
			BackMgr.I.Init();
			PlatformMgr.I.Init();
			FingerTouchMgr.I.Init();
			OperationTip.I.Init();
			MagnetMgr.I.Init();
			MagicWandMgr.I.Init();
			// asm 0x10cae64: new Action(this, MD_GameMgr$$InitCompleted)
			ElementsMgr.I.Init(_GameStageProgress, !isReset, InitCompleted);
			SpecialBlockRewardManager specialBlockRewardManager = Singleton<SpecialBlockRewardManager>.I;
			specialBlockRewardManager.LevelInit(Singleton<RecordMgr>.I.JudgeHasRecord());
			if (_useRewardLayer)
			{
				_useRewardLayer.InitView();
			}
			LogEvent.I.Level_Start(LevelMgr.Data.Type, newlv == curlv, LevelMgr.Data.IsRegionRankLevel);
			LogEvent.I.AF_Level(LevelMgr.Data.Lv);
			if (regionRankGameScoreLayer != null)
			{
				// thân RegionRankGameScoreLayer.Init() được inline vào hàm (bật/tắt gameObject theo IsRegionRankLevel, text "x" + RegionRankScore)
				regionRankGameScoreLayer.Init();
			}
		}

		public void Revive(int time)
		{
			timeControl.AddTimeByAnim(time, delegate
			{
				GameStatus = GameStatus.Playing;
				DispatcherMgr.Emit("GameStatus_Change", GameStatus);
			});
		}

		private void OnGameTimeEnd()
		{
			// Thân Fail(true, true) được inline vào hàm
			Fail(true, true);
		}

		private void OnPlatformFall()
		{
			// Thân Fail(false, false) được inline vào hàm
			Fail(false, false);
		}

		private void OnDestroyAllElement()
		{
			// Thân Victory(false) được inline vào hàm
			Victory();
		}

		private void Fail(bool isCheck, bool isTimeOut)
		{
			if (isCheck && ElementsMgr.I.Data.JudgeWholePlaced())
			{
				return;
			}
			if (!ModifyGameStatus(GameStatus.Failure))
			{
				return;
			}
			Singleton<RecordMgr>.I.Clear(false);
			if (LevelMgr.Data.Type == LevelType.BakingMaster)
			{
				// Thân BakingMasterFailureDialog.Create(isTimeOut) được inline (AddTopNode + gán _bTimeOut)
				BakingMasterFailureDialog.Create(isTimeOut);
			}
			else
			{
				SceneMgr.AddTopNode<FailureControl>("Prefabs/FailureDialog").isTimeOut = isTimeOut;
			}
		}

		private void OnFuseCompleted(ElementType type)
		{
			if (LevelMgr.Data.Type == LevelType.BakingMaster)
			{
				return;
			}
			_popDialogCount++;
			UserUpgrade(delegate
			{
				SpecialBlockReward(type, delegate
				{
					_popDialogCount = Math.Max(0, _popDialogCount - 1);
				});
			});
		}

		public void Victory(bool force = false)
		{
			if (GameStatus == GameStatus.Victory || GameStatus == GameStatus.Failure)
			{
				return;
			}
			if (!force && ElementsMgr.I.Data.NeedClearNumber > 0)
			{
				return;
			}
			if (ModifyGameStatus(GameStatus.Victory))
			{
				GameTimeTriggerMono.I.EndTime("Level");
				Singleton<RecordMgr>.I.Clear(false);
				if (LevelMgr.Data.IsRegionRankLevel)
				{
					GSingleton<RegionRankActivityManager>.ins.LevelVictory(LevelMgr.Data.RegionRankScore);
				}
				StartCoroutine("IEShowVictoryDialog");
			}
		}

		private IEnumerator IEShowVictoryDialog()
		{
			yield return null;
			while (comboControl.animNumber > 0 || _popDialogCount > 0)
			{
				yield return null;
			}
			DispatcherMgr.Emit("Unload_Unused_Assets", true);
			int fTime = timeControl.TotalTime - timeControl.ResidueTime;
			timeControl.ClearTimeByStar(chestControl.tfStar.position, delegate
			{
				if (LevelMgr.Data.Type == LevelType.BakingMaster)
				{
					BakingMasterVictoryDialog.Create(fTime);
					return;
				}
				Queue<Action> endActions = new Queue<Action>();
				if (RateUsDialog.CanShow())
				{
					endActions.Enqueue(delegate
					{
						// thân RateUsDialog.ShowRateUs(closeCall) được inline vào lambda
						RateUsDialog.ShowRateUs(endActions.Dequeue());
					});
				}
				if (PigBankData.Instance.CanShow() && !DB.User.IsSkipPigAnim)
				{
					if (PigBankData.Instance.IsForceGoHome())
					{
						PigBankData.Instance.NeedDoAnimation = true;
						PigBankData.Instance.WinAddPigBankGold();
					}
					else
					{
						endActions.Enqueue(delegate
						{
							EndPigBankDialog endPigBankDialog = SceneMgr.AddTopNode<EndPigBankDialog>("Prefabs/EndPigBankDialog");
							endPigBankDialog.Show(PigBankData.Instance.PigReward, endActions.Dequeue());
						});
					}
				}
				if (VictoryRewardDialog.TryShow(LevelMgr.Data.Lv))
				{
					endActions.Enqueue(delegate
					{
						VictoryRewardDialog.Show(LevelMgr.Data.Lv, endActions.Dequeue());
					});
				}
				if (LevelUnlockProgressCom.HaveUnlock())
				{
					endActions.Enqueue(delegate
					{
						LevelUnlockProgress.Show(endActions.Dequeue());
					});
				}
				endActions.Enqueue(delegate
				{
					SceneMgr.AddTopNode<VictoryControl>("Prefabs/VictoryDialog");
				});
				endActions.Dequeue()();
			});
		}

		private void UserUpgrade(Action action)
		{
			Singleton<UserLevelRewardManager>.I.AddScore();
			if (!Singleton<UserLevelRewardManager>.I.CheckLevelUp())
			{
				action.InvokeSafe();
			}
			else
			{
				Singleton<UserLevelRewardManager>.I.LevelUp(action);
			}
		}

		private void SpecialBlockReward(ElementType type, Action action)
		{
			if (type == ElementType.Assimilation)
			{
				Singleton<SpecialBlockRewardManager>.I.OnSpecialBlockCollect(action);
			}
			else
			{
				Singleton<SpecialBlockRewardManager>.I.OnNormalBlockCollect();
				action.InvokeSafe();
			}
		}

		private void GamePause(bool pause, string param)
		{
			ModifyGameStatus(pause ? GameStatus.Waiting : GameStatus.Playing);
		}

		public void ClearGameStatus()
		{
			GameStatus = GameStatus.None;
		}

		public bool ModifyGameStatus(GameStatus status, bool isForce = false)
		{
			if (status == GameStatus.None)
			{
				return false;
			}
			if (!isForce)
			{
				if (GameStatus == GameStatus.Victory || GameStatus == GameStatus.Failure)
				{
					return false;
				}
				if (GameStatus == GameStatus.None)
				{
					return false;
				}
				if (GameStatus == GameStatus.Waiting)
				{
					if (status == GameStatus.Playing)
					{
						if (_waitingNumber > 0)
						{
							_waitingNumber = 0;
							return false;
						}
					}
					else if (status == GameStatus.Waiting)
					{
						_waitingNumber++;
						return false;
					}
				}
			}
			GameStatus = status;
			DispatcherMgr.Emit("GameStatus_Change", status);
			return true;
		}

		private void InitCompleted()
		{
			if (Singleton<RecordMgr>.I.IsLoading || LevelMgr.Data.Type != LevelType.Ordinary)
			{
				StartGame();
				return;
			}
			ShowBooster(delegate(bool isUseMagicWand, bool isUseMagnet)
			{
				DB.User.IsUseMagnet = isUseMagnet;
				if (!ShowElementWholeGuide(ExecuteBooster))
				{
					ShowGift(ExecuteBooster);
				}
				void ExecuteBooster()
				{
					if (isUseMagicWand)
					{
						ExecuteMagicWand(delegate
						{
							if (isUseMagnet)
							{
								ExecuteMagnet(StartGame);
							}
							else
							{
								StartGame();
							}
						});
					}
					else if (isUseMagnet)
					{
						ExecuteMagnet(StartGame);
					}
					else
					{
						StartGame();
					}
				}
			});
		}

		private void ShowBooster(Action<bool, bool> action)
		{
			if (LevelMgr.Data.Lv < cf_const.lvStartPageUnlockLv)
			{
				action.InvokeSafe(false, false);
				return;
			}
			LevelStartDialog levelStartDialog = SceneMgr.AddTopNode<LevelStartDialog>("Prefabs/LevelStartDialog");
			levelStartDialog.cback = action;
			levelStartDialog.Show();
		}

		private void ExecuteMagnet(Action action)
		{
			DB.Bag.Modify(PropID.Magnet, -1, true, "LevelStart");
			// thân MagnetMgr.Execute(action) được inline vào hàm
			MagnetMgr.I.Execute(action);
		}

		private void ExecuteMagicWand(Action action)
		{
			// thân MagicWandMgr.Execute(action) được inline vào hàm
			MagicWandMgr.I.Execute(action);
		}

		private bool ShowElementWholeGuide(Action action)
		{
			int showElementWholeGuideStatus = DB.User.ShowElementWholeGuideStatus;
			if (showElementWholeGuideStatus == 3 || showElementWholeGuideStatus == 1)
			{
				return false;
			}
			if (showElementWholeGuideStatus == 0)
			{
				DB.User.ShowElementWholeGuideStatus = ((LevelMgr.Data.Lv >= cf_const.ElementWholeGuideLevel) ? 2 : 1);
			}
			foreach (KeyValuePair<int, List<Element>> element in ElementsMgr.I.Data.Elements)
			{
				List<Element> value = element.Value;
				for (int i = 0; i < value.Count; i++)
				{
					if (value[i].Type != ElementType.Ordinary && value[i].Type != ElementType.Assimilation)
					{
						DB.User.ShowElementWholeGuideStatus = 3;
						GameGuideDialog gameGuideDialog = SceneMgr.AddTopNode<GameGuideDialog>("Prefabs/GameGuideDialog");
						gameGuideDialog.CloseCallback = action;
						gameGuideDialog.Show(rootView.pauseButton.transform.position);
						return true;
					}
				}
			}
			return false;
		}

		private void ShowGift(Action action)
		{
			Pop_Gift.GiftManager.Instance.TryAutoPopGift(Pop_Gift.GiftTriggerPosition.JustEnteredLevel, delegate(bool _, bool isHaveNoAdsGiftPopped)
			{
				if (isHaveNoAdsGiftPopped)
				{
					action.InvokeSafe();
				}
				else
				{
					Singleton<_Game._Functions.PropIceBreakingGift.PropIceBreakingGiftManager>.I.TryPopGift(action);
				}
			}, true);
		}

		private void StartGame()
		{
			if (!Singleton<RecordMgr>.I.IsLoading)
			{
				if (LevelMgr.Data.Type == LevelType.Ordinary)
				{
					bool flag = ElementGuideMgr.I.CheckGuide();
					FingerTouchMgr.I.SetRaycastMask(flag);
					if (!flag)
					{
						NoticeLEDDialog.Create();
					}
				}
				else
				{
					FingerTouchMgr.I.SetRaycastMask(false);
					NoticeLEDDialog.Create();
				}
			}
			else
			{
				chestControl.SetSumStar(Singleton<RecordMgr>.I.Data.StarNumber);
				PlatformMgr.I.RecoverData();
				FingerTouchMgr.I.SetRaycastMask(false);
				NoticeLEDDialog.Create();
			}
			propControl.ShowHand();
			ModifyGameStatus(GameStatus.Playing, isForce: true);
			Singleton<RecordMgr>.I.LoadEnd();
			if (ElementsMgr.I.Data.Elements.Count == 0)
			{
				Victory();
			}
			Singleton<RewardVideoManager>.I.ResetPreEnterData();
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
			{
				// thân RecordMgr.SaveData() được inline vào hàm
				Singleton<RecordMgr>.I.SaveData();
			}
		}
	}
}
