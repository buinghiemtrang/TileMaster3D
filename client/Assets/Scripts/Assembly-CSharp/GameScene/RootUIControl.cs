using GameScene_Level;
using Pop_Gift;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene
{
	public class RootUIControl : MonoBehaviour
	{
		public Text ttLevel;

		public GameObject noAdsBtn;

		public GameObject pauseBtn;

		public Transform gameRect;

		private cfbt_Stores _noAdsConfig;

		private void Awake()
		{
			DispatcherMgr.On("IAPInitFinishedCallBack", _iAPInitSuc);
			DispatcherMgr.On<GiftType>("BuySuccessCallBack", _iAPBuySuc);
		}

		public void Init()
		{
			_showUI();
			ttLevel.SetLBy(DB.User.Curlv + 1);
		}

		public void ClickPause(MButton button)
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

		public void NoAdBtn(MButton btn)
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

		private void _iAPBuySuc(GiftType type)
		{
			// RVA 0x10d6458 và 0x10d645c là nhánh 'b _showUI' (cùng thân)
			_showUI();
		}

		private void _iAPInitSuc()
		{
			_showUI();
		}

		private void _showUI()
		{
			pauseBtn.SetActive(LevelMgr.Data.Type != LevelType.Ordinary || DB.User.Curlv > 1);
			var popGift = GiftNoAdsManager.Instance.GetPopGift();
			_noAdsConfig = popGift?.StoresConfig;
			noAdsBtn.SetActive(_noAdsConfig != null);
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off("IAPInitFinishedCallBack", _iAPInitSuc);
			DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _iAPBuySuc);
		}
	}
}
