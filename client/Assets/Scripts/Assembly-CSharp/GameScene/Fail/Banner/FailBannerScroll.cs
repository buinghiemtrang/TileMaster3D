using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Com.TheFallenGames.OSA.Core;

namespace GameScene.Fail.Banner
{
	public class FailBannerScroll : OSA<FailBannerPwp, FailBannerIvh>
	{
		[CompilerGenerated]
		private Action<int> m_OnSelectedValueChanged;

		private int _selectedValue;

		private List<FailBanner> _banners;

		internal int SelectedValue => _selectedValue;

		internal event Action<int> OnSelectedValueChanged
		{
			[CompilerGenerated]
			add
			{
				m_OnSelectedValueChanged = (Action<int>)Delegate.Combine(m_OnSelectedValueChanged, value);
			}
			[CompilerGenerated]
			remove
			{
				m_OnSelectedValueChanged = (Action<int>)Delegate.Remove(m_OnSelectedValueChanged, value);
			}
		}

		internal void RefreshItems(List<FailBanner> banners)
		{
			base.Start();
			_banners = banners;
			ResetItems(banners.Count);
		}

		protected override void Start()
		{
			base.Start();
			_selectedValue = -1;
		}

		protected override void Update()
		{
			base.Update();
			if (IsInitialized && VisibleItemsCount != 0)
			{
				FailBannerIvh itemViewsHolder = GetItemViewsHolder(VisibleItemsCount / 2);
				if (_selectedValue != itemViewsHolder.ItemIndex && m_OnSelectedValueChanged != null)
				{
					int itemIndex = itemViewsHolder.ItemIndex;
					_selectedValue = itemIndex;
					m_OnSelectedValueChanged(itemIndex);
				}
			}
		}

		protected override FailBannerIvh CreateViewsHolder(int itemIndex)
		{
			FailBannerIvh failBannerIvh = new FailBannerIvh();
			FailBanner banner = _banners[itemIndex];
			failBannerIvh.Init(_Params.GetPrefab(banner), _Params.Content, itemIndex);
			return failBannerIvh;
		}

		protected override void UpdateViewsHolder(FailBannerIvh newOrRecycled)
		{
			newOrRecycled.UpdateViewsHolder(_banners[newOrRecycled.ItemIndex]);
		}

		protected override bool IsRecyclable(FailBannerIvh potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, double sizeOfItemThatWillBecomeVisible)
		{
			return potentiallyRecyclable.CanRecyclable(_banners[indexOfItemThatWillBecomeVisible]);
		}

		internal void ScrollToNextPage()
		{
			if (_banners != null && _banners.Count > 0)
			{
				int num = _selectedValue + 1;
				if (num >= _banners.Count)
				{
					num = 0;
				}
				SmoothScrollTo(num, 0.7f, 0.5f, 0.5f);
			}
		}
	}
}
