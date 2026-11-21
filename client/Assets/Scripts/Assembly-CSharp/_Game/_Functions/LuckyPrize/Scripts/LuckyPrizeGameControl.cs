using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Game._Functions.LuckyPrize.Scripts
{
	public class LuckyPrizeGameControl
	{
		public readonly Transform tfRoot;

		public readonly Transform tfRewardClaim;

		public readonly Transform tfRewardPay;

		private LuckyPrizeGameContent _pContent;

		private LuckyPrizeGamePanel _pPanel;

		public GameStatus kGameStatus;

		private readonly LuckyPrizeScrollView _pScroll;

		public int GroupCount
		{
			get
			{
				return _pContent.GroupCount;
			}
			set
			{
				_pContent.GroupCount = value;
			}
		}

		public LuckyPrizeGameControl(Transform root, Transform rewardRoot, LuckyPrizeScrollView scrollView)
		{
			tfRoot = root;
			_pScroll = scrollView;
			tfRewardClaim = rewardRoot.Find("ClaimNode");
			tfRewardClaim.FindC<MButton>("Button").onClick.AddListener(ClickBtnVictory);
			tfRewardPay = rewardRoot.Find("PayNode");
			tfRewardPay.FindC<MButton>("Button").onClick.AddListener(ClickBtnVictory);
			if (_pContent == null)
			{
				_pContent = new LuckyPrizeGameContent(root, ClickBtnItem);
			}
			if (_pPanel == null)
			{
				_pPanel = new LuckyPrizeGamePanel(root, this);
			}
			DispatcherMgr.On<GiftType>("BuySuccessCallBack", _iAPBuySuc);
		}

		public void Destructor()
		{
			DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _iAPBuySuc);
			_pContent.Destructor();
			_pPanel.Destructor();
			_pContent = null;
			_pPanel = null;
		}

		public void RefreshData()
		{
			_pContent.RefreshItem();
			_pPanel.RefreshData();
			tfRewardClaim.gameObject.SetActive(false);
			tfRewardPay.gameObject.SetActive(false);
			tfRoot.gameObject.SetActive(DB.Other.LuckyPrizeRewardIndex < DataCfg.cfgLuckyPrizes.Count);
			TweenMgr.ScaleMinToMax(tfRoot, 0f, 1f, 0.2f);
			SetGameStatus(GameStatus.Playing);
		}

		public void SetGameStatus(GameStatus status)
		{
			if (kGameStatus != status)
			{
				kGameStatus = status;
				if (status == GameStatus.Victory || status == GameStatus.Failure)
				{
					TweenMgr.ScaleMaxToMin(tfRoot, 1f, 0f, 0.2f).OnComplete(delegate
					{
						if (status == GameStatus.Victory)
						{
							LogEvent.I.Track("LuckyPacks_Win").Add("Challenge_ID", DB.Other.LuckyPrizeRewardIndex).CommonAttr(true).Send(false);
							tfRoot.gameObject.SetActive(false);
							cfgLuckyPrize cfg = DataCfg.cfgLuckyPrizes[DB.Other.LuckyPrizeRewardIndex];
							MButton button = tfRewardPay.FindC<MButton>("Button");
							if (cfg.storeIndex == -1)
							{
								tfRewardClaim.gameObject.SetActive(true);
								TweenMgr.ScaleMinToMax(tfRewardClaim, 0f, 1f, 0.2f);
								int[] array = cfg.rewards.ToInt('|');
								Global.GetContents((PropID)array[0], array[1], out Sprite sprite, out string text);
								tfRewardClaim.FindC<Image>("Icon").sprite = sprite;
								tfRewardClaim.FindC<Text>("Icon/Text").text = text;
								button.interactable = true;
							}
							else
							{
								cfbt_Stores store = DataCfg.cfbt_Storess.Find(c => c.index == cfg.storeIndex);
								if (store != null)
								{
									tfRewardPay.gameObject.SetActive(true);
									TweenMgr.ScaleMinToMax(tfRewardPay, 0f, 1f, 0.2f);
									if (store.price == "")
									{
										button.interactable = false;
										button.FindC<Text>("Text").text = string.Format("${0}", store.usd);
									}
									else
									{
										button.interactable = true;
										button.FindC<Text>("Text").text = store.price;
									}
								}
							}
						}
						else if (status == GameStatus.Failure)
						{
							RefreshData();
							TweenMgr.ScaleMinToMax(tfRoot, 0f, 1f, 0.2f);
							LogEvent.I.Track("LuckyPacks_Fail").Add("Challenge_ID", DB.Other.LuckyPrizeRewardIndex).CommonAttr(true).Send(false);
						}
					});
				}
			}
		}

		private void ClickBtnItem(LuckyPrizeGameBlockItem item)
		{
			if (kGameStatus == GameStatus.Playing && item.IsActive)
			{
				if (_pPanel.AddBlock(item))
				{
					if (item.nIndex % 3 != 2)
					{
						_pContent.SetBlockItem(item.nIndex + 1);
					}
				}
			}
		}

		private void ClickBtnVictory(MButton button)
		{
			cfgLuckyPrize cfg = DataCfg.cfgLuckyPrizes[DB.Other.LuckyPrizeRewardIndex];
			if (cfg.storeIndex == -1)
			{
				int[] array = cfg.rewards.ToInt('|');
				DB.Bag.Modify((PropID)array[0], array[1], "LuckyPacks");
				NextGameOne();
				LogEvent.I.Track("LuckyPacks_Receive").Add("Reward", cfg.rewards).CommonAttr(true).Send(false);
			}
			else
			{
				cfbt_Stores store = DataCfg.cfbt_Storess.Find(c => c.index == cfg.storeIndex);
				if (store != null)
				{
					SceneMgr.OpenWaiting(30f);
					string productId = Global.pStores.ProductId(store.id);
					LogEvent.I.Track("LuckyPacks_Buy")
						.Add("Challenge_ID", DB.Other.LuckyPrizeRewardIndex)
						.Add("Commodity_ID", productId)
						.CommonAttr(true)
						.Send(false);
					if (!IAPManager.instance.Buy(store))
					{
						SceneMgr.CloseWaiting();
					}
				}
			}
		}

		private void _iAPBuySuc(GiftType gType)
		{
			NextGameOne();
		}

		private void NextGameOne()
		{
			DB.Other.LuckyPrizeRewardIndex++;
			if (DB.Other.LuckyPrizeRewardIndex < DataCfg.cfgLuckyPrizes.Count)
			{
				DB.Other.LuckyPrizeKind.Clear();
				_pScroll.NextItemOne();
				RefreshData();
			}
			else
			{
				_pScroll.SetLastStatus();
				tfRewardClaim.gameObject.SetActive(false);
				tfRewardPay.gameObject.SetActive(false);
				tfRoot.gameObject.SetActive(false);
			}
		}
	}
}
