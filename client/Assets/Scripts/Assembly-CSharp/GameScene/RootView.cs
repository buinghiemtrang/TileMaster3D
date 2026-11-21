using GameScene_Level;
using Pop_Gift;
using UnityEngine;

namespace GameScene
{
	public class RootView : MonoBehaviour
	{
		[SerializeField]
		private MButton noAdsButton;

		public MButton pauseButton;

		public Transform gameBoundary;

		private cfbt_Stores _noAdsConfig;

		private void Awake()
		{
			noAdsButton.onClick.AddListener(_ClickNoAds);
			pauseButton.onClick.AddListener(_ClickPause);
			DispatcherMgr.On("IAPInitFinishedCallBack", _IAPInitSuccess);
			DispatcherMgr.On<GiftType>("BuySuccessCallBack", _IAPBuySuccess);
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off("IAPInitFinishedCallBack", _IAPInitSuccess);
			DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _IAPBuySuccess);
		}

		public void Init()
		{
			// Init (0x10cb860), _IAPInitSuccess (0x10cec68), _IAPBuySuccess (0x10cec6c) đều là nhánh 'b _ShowUI' (đọc từ assembly)
			_ShowUI();
		}

		private void _IAPInitSuccess()
		{
			_ShowUI();
		}

		private void _IAPBuySuccess(GiftType type)
		{
			_ShowUI();
		}

		private void _ShowUI()
		{
			pauseButton.SetActive(LevelMgr.Data.Type != LevelType.Ordinary || LevelMgr.Data.Lv > 2);
			var popGift = GiftNoAdsManager.Instance.GetPopGift();
			_noAdsConfig = popGift?.StoresConfig;
			noAdsButton.gameObject.SetActive(_noAdsConfig != null);
		}

		private void _ClickNoAds(MButton button)
		{
			if (GameMgr.I.GameStatus != GameStatus.Playing || _noAdsConfig == null)
			{
				return;
			}
			SceneMgr.OpenWaiting(30f);
			if (!IAPManager.instance.Buy(_noAdsConfig))
			{
				SceneMgr.CloseWaiting();
			}
			LogEvent.I.Track("NoAds_Open").Add("Type", "Tips").CommonAttr(true).Send();
		}

		private void _ClickPause(MButton button)
		{
			if (GameMgr.I.GameStatus != GameStatus.Playing)
			{
				return;
			}
			GameMgr.I.ModifyGameStatus(GameStatus.Waiting);
			PauseControl pauseControl = SceneMgr.AddTopNode<PauseControl>("Prefabs/PauseDialog");
			pauseControl.func_close = delegate
			{
				GameMgr.I.ModifyGameStatus(GameStatus.Playing);
			};
			pauseControl.func_restart = delegate
			{
				GameMgr.I.Init(true);
			};
		}
	}
}
