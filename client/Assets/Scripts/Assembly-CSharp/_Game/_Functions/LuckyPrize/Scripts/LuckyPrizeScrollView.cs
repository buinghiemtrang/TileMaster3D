using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace _Game._Functions.LuckyPrize.Scripts
{
	public class LuckyPrizeScrollView : MonoBehaviour
	{
		[SerializeField]
		private Transform _tfContent;

		private Transform[] _ayPos;

		[SerializeField]
		private Transform _tfItemPrefab;

		[SerializeField]
		private int _nStartIndex;

		[SerializeField]
		private Sprite[] aySprite;

		private List<LuckyPrizeRewardItem> _ayItem;

		private Stack<LuckyPrizeRewardItem> _ayItemPool;

		private void Awake()
		{
			if (_ayItem == null)
			{
				_ayItem = new List<LuckyPrizeRewardItem>();
			}
			if (_ayItemPool == null)
			{
				_ayItemPool = new Stack<LuckyPrizeRewardItem>();
			}
			if (_ayPos == null)
			{
				_ayPos = new Transform[_tfContent.childCount];
			}
			int childCount = _tfContent.childCount;
			for (int i = 0; i < childCount; i++)
			{
				_ayPos[i] = _tfContent.GetChild(i);
			}
		}

		private LuckyPrizeRewardItem PopItem(bool insert)
		{
			LuckyPrizeRewardItem luckyPrizeRewardItem;
			if (_ayItemPool.Count == 0)
			{
				Transform transform = Object.Instantiate(_tfItemPrefab, _tfContent, false);
				luckyPrizeRewardItem = new LuckyPrizeRewardItem(transform);
			}
			else
			{
				luckyPrizeRewardItem = _ayItemPool.Pop();
			}
			if (insert)
			{
				_ayItem.Insert(0, luckyPrizeRewardItem);
			}
			else
			{
				_ayItem.Add(luckyPrizeRewardItem);
			}
			return luckyPrizeRewardItem;
		}

		private void PushItem(LuckyPrizeRewardItem item)
		{
			_ayItemPool.Push(item);
			item.Destroy();
			_ayItem.Remove(item);
		}

		public void Initialized()
		{
			_ayItem.ForEach(item =>
			{
				_ayItemPool.Push(item);
				item.Destroy();
			});
			_ayItem.Clear();
			int rewardIndex = DB.Other.LuckyPrizeRewardIndex;
			int count = DataCfg.cfgLuckyPrizes.Count;
			if (count <= rewardIndex)
			{
				_set_item(_nStartIndex, count - 1, 1, false);
				for (int i = _nStartIndex - 1, index = count - 2; i >= 0 && index >= 0; i--, index--)
				{
					_set_item(i, index, 1, true);
				}
			}
			else
			{
				int i = _nStartIndex;
				while (i < _ayPos.Length && rewardIndex < DataCfg.cfgLuckyPrizes.Count)
				{
					int status = (i == _nStartIndex) ? 0 : -1;
					_set_item(i, rewardIndex, status, false);
					i++;
					rewardIndex++;
				}
				rewardIndex = DB.Other.LuckyPrizeRewardIndex;
				for (int j = _nStartIndex - 1, index = rewardIndex - 1; j >= 0 && index >= 0; j--, index--)
				{
					_set_item(j, index, 1, true);
				}
			}
		}

		private LuckyPrizeRewardItem _set_item(int i, int index, int status, bool insert)
		{
			Transform targetPos = _ayPos[i];
			cfgLuckyPrize cfg = DataCfg.cfgLuckyPrizes[index];
			LuckyPrizeRewardItem item = PopItem(insert);
			item.nPosIndex = i;
			item.nCfgIndex = index;
			item.tfRoot.localPosition = targetPos.localPosition;
			item.tfRoot.localScale = targetPos.localScale;
			item.Initialized(cfg, status, aySprite);
			return item;
		}

		public void NextItemOne()
		{
			if (_ayItem[0].nPosIndex == 0)
			{
				LuckyPrizeRewardItem item = _ayItem.Pop(0);
				PushItem(item);
			}
			for (int i = 0; i < _ayItem.Count; i++)
			{
				LuckyPrizeRewardItem item2 = _ayItem[i];
				item2.nPosIndex--;
				int status = (item2.nPosIndex < _nStartIndex) ? 1 : -1;
				if (item2.nPosIndex == _nStartIndex)
				{
					status = 0;
				}
				item2.SetStatus(status);
				Transform targetPos = _ayPos[item2.nPosIndex];
				item2.tfRoot.DOLocalMove(targetPos.localPosition, 0.3f);
				item2.tfRoot.DOScale(targetPos.localScale, 0.3f);
			}
			LuckyPrizeRewardItem lastItem = _ayItem.Last();
			if (lastItem.nCfgIndex + 1 < DataCfg.cfgLuckyPrizes.Count)
			{
				_set_item(lastItem.nPosIndex + 1, lastItem.nCfgIndex + 1, -1, false);
			}
		}

		public void SetLastStatus()
		{
			LuckyPrizeRewardItem lastItem = _ayItem.Last();
			lastItem.SetStatus(1);
		}
	}
}
