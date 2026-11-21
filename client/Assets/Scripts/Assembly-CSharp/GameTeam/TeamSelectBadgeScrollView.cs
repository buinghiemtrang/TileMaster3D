using System.Collections.Generic;
using Scroll;

namespace GameTeam
{
	public class TeamSelectBadgeScrollView : ScrollBase<SelectBadgeHuntPwp, SelectBadgeHuntIvh>
	{
		private List<cfg_TeamBadge> _badges;

		protected override void Start()
		{
			base.Start();
			_Params.DefaultItemHeight = _Params.ItemPrefab.sizeDelta.y;
			_badges = DataCfg.cfg_TeamBadges;
			ExecuteRefresh();
		}

		protected override SelectBadgeHuntIvh CreateViewsHolder(int itemIndex)
		{
			SelectBadgeHuntIvh selectBadgeHuntIvh = new SelectBadgeHuntIvh();
			selectBadgeHuntIvh.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return selectBadgeHuntIvh;
		}

		protected override void UpdateViewsHolder(SelectBadgeHuntIvh newOrRecycled)
		{
			int startIdx = newOrRecycled.ItemIndex * _Params.cellInclusionNum;
			int count = _Params.cellInclusionNum;
			if (startIdx + count > _badges.Count)
			{
				count = _badges.Count - startIdx;
			}
			cfg_TeamBadge[] array = new cfg_TeamBadge[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = _badges[startIdx + i];
			}
			newOrRecycled.PItem.Init(array);
		}

		protected override void Reset()
		{
			int num = _badges.Count / _Params.cellInclusionNum;
			if (_badges.Count % _Params.cellInclusionNum > 0)
			{
				num++;
			}
			ResetItems(num, false, false);
		}
	}
}
