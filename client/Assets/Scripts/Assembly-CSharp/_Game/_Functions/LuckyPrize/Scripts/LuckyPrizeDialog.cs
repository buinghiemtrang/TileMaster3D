using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Game._Functions.LuckyPrize.Scripts
{
	public class LuckyPrizeDialog : MonoBehaviour
	{
		[SerializeField]
		private DialogShow _pDialog;

		[SerializeField]
		private Text _ttRefreshTime;

		[SerializeField]
		private LuckyPrizeScrollView _pScroll;

		[SerializeField]
		private Transform _tfGameRoot;

		[SerializeField]
		private Transform _tfRewardRoot;

		private LuckyPrizeGameControl _pGameCtrl;

		private void Awake()
		{
			if (_pGameCtrl == null)
			{
				_pGameCtrl = new LuckyPrizeGameControl(_tfGameRoot, _tfRewardRoot, _pScroll);
			}
		}

		private void OnDestroy()
		{
			_pGameCtrl.Destructor();
			_ttRefreshTime.DOKill();
			_pGameCtrl = null;
		}

		private void Start()
		{
			LogEvent.I.Track("LuckyPacks_Open").CommonAttr(true).Send(false);
			RefreshUI();
			_pGameCtrl.RefreshData();
		}

		private void RefreshUI()
		{
			Singleton<LuckyPrizeMgr>.I.CheckRefreshReward();
			int refreshTime = Singleton<LuckyPrizeMgr>.I.GetRefreshTime();
			_ttRefreshTime.DOCounter(refreshTime, 0, refreshTime, Util.Helper.FormatTimerD).OnComplete(Start);
			_pScroll.Initialized();
		}

		public static LuckyPrizeDialog Create()
		{
			return SceneMgr.AddTopNode<LuckyPrizeDialog>("Prefabs/LuckyPrizeDialog");
		}
	}
}
