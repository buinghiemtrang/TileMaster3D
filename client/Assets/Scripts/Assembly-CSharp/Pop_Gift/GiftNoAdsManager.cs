using System;
using System.Collections.Generic;
using DataCenter;

namespace Pop_Gift
{
	public class GiftNoAdsManager
	{
		private static GiftNoAdsManager _instance;

		public const string GiftNoAdsAutoPopEvent = "GiftNoAdsAutoPopEvent";

		private bool _isOutGameFirstEnterTriggerPositionPop;

		private bool _isInitialized;

		public static GiftNoAdsManager Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new GiftNoAdsManager();
				}
				return _instance;
			}
		}

		public void Init()
		{
			if (!_isInitialized)
			{
				_isInitialized = true;
				_isOutGameFirstEnterTriggerPositionPop = DB.User.Maxlv + 1 > cf_gift_no_ads_const.OutGameAutoPopConditionUnlocklv;
			}
		}

		public void RefreshData()
		{
			if (!string.IsNullOrEmpty(DB.GiftNoAds.LastRefreshTime))
			{
				DateTime dateTime = GetDateTime(DB.GiftNoAds.LastRefreshTime);
				if (DateTime.Now.TotalDays() - dateTime.TotalDays() < 1)
				{
					return;
				}
			}
			DB.GiftNoAds.LastRefreshTime = DateTime.Now.ToStringFull();
			DB.GiftNoAds.AutoPopCount = 0;
			DB.GiftNoAds.PlayInterstitialSuccessCount = 0;
			DB.GiftNoAds.IsCloseAutoPop = 0;
		}

		public bool TryAutoPopGift(GiftTriggerPosition position)
		{
			return TryAutoPopGift(position, delegate { }, false);
		}

		public bool TryAutoPopGift(GiftTriggerPosition position, Action<bool, bool> callback, bool popFailTriggerCallback)
		{
			GiftData popGift = GetPopGift(false);
			if (popGift == null)
			{
				if (popFailTriggerCallback)
				{
					callback?.Invoke(false, false);
				}
				return false;
			}
			if (!cf_gift_no_ads_const.TriggerPositions.Contains((int)position))
			{
				if (popFailTriggerCallback)
				{
					callback?.Invoke(false, false);
				}
				return false;
			}
			bool isOutGameFirstEnterTriggerPositionPop = _isOutGameFirstEnterTriggerPositionPop;
			_isOutGameFirstEnterTriggerPositionPop = false;
			if (cf_gift_no_ads_const.IsUseFollowConfig == 1 && DB.GiftNoAds.AutoPopCount < cf_gift_no_ads_const.DailyAutoPopMaxCount)
			{
				if (isOutGameFirstEnterTriggerPositionPop)
				{
					DB.GiftNoAds.AutoPopCount++;
					DB.GiftNoAds.PlayInterstitialSuccessCount = 0;
					GiftManager.Instance.CreateDialog(true, popGift, delegate(bool isSuccess)
					{
						callback?.Invoke(isSuccess, true);
					});
					DispatcherMgr.Emit("GiftNoAdsAutoPopEvent", popGift);
					return true;
				}
				int[,] array = cf_gift_no_ads_const.AutoPopPlayInterstitialCountCondition.ToInt(2, '|', ':');
				int length = array.GetLength(0);
				if (length > 0)
				{
					bool flag = false;
					int num = DB.User.Maxlv + 1;
					for (int i = 0; i < length; i++)
					{
						if (num > array[i, 0] && (i + 1 >= length || num <= array[i + 1, 0]))
						{
							flag = DB.GiftNoAds.PlayInterstitialSuccessCount >= array[i, 1];
						}
					}
					if (flag)
					{
						DB.GiftNoAds.AutoPopCount++;
						DB.GiftNoAds.PlayInterstitialSuccessCount = 0;
						GiftManager.Instance.CreateDialog(true, popGift, delegate(bool isSuccess)
						{
							callback?.Invoke(isSuccess, true);
						});
						DispatcherMgr.Emit("GiftNoAdsAutoPopEvent", popGift);
						return true;
					}
				}
			}
			if (popFailTriggerCallback)
			{
				callback?.Invoke(false, false);
			}
			return false;
		}

		public GiftData GetPopGift(bool isCheckIapInitSuccess = true)
		{
			if (!DB.User.AdFree && DB.User.Maxlv + 1 > cf_gift_no_ads_const.UnlockNoadsLv)
			{
				if (isCheckIapInitSuccess && (IAPManager.instance == null || !IAPManager.instance.isInitialized))
				{
					return null;
				}
				List<GiftData> fixedTypeGiftsData = GiftManager.Instance.GetFixedTypeGiftsData(GiftTypeInGift.NoAds);
				if (fixedTypeGiftsData != null && fixedTypeGiftsData.Count > 0)
				{
					return fixedTypeGiftsData[0];
				}
			}
			return null;
		}

		public bool PopGift(Action<bool> callback)
		{
			GiftData popGift = GetPopGift();
			if (popGift == null)
			{
				callback?.Invoke(false);
			}
			else
			{
				PopGift(popGift, callback);
			}
			return popGift != null;
		}

		public void PopGift(GiftData data, Action<bool> callback)
		{
			GiftManager.Instance.CreateDialog(false, data, callback);
		}

		public void AddInterstitialPlayCount(int count)
		{
			DB.GiftNoAds.PlayInterstitialSuccessCount += count;
		}

		public bool CloseOrOpenAutoPop()
		{
			DB.GiftNoAds.IsCloseAutoPop = ((DB.GiftNoAds.IsCloseAutoPop == 0) ? 1 : 0);
			return DB.GiftNoAds.IsCloseAutoPop == 1;
		}

		private DateTime GetDateTime(string time)
		{
			return DateTime.Now.ParseFull(time);
		}
	}
}
