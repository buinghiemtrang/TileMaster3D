using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;
using DG.Tweening;

namespace GameTeam
{
	public class TeamJoinDialogScrollView : OSA<TeamJoinDialogPWP, TeamJoinDialogIVH>
	{
		private List<MemberInfo> datas;

		protected override void Start()
		{
			base.Start();
			_Params.DefaultItemHeight = _Params.ItemPrefab.sizeDelta.y;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			DOTween.Kill("DelayCall_RefreshItems_TeamJoinDialog");
		}

		public void RefreshItems(List<MemberInfo> _datas)
		{
			datas = _datas;
			DOTween.Kill("DelayCall_RefreshItems_TeamJoinDialog");
			TweenMgr.DelayCall(0.1f, delegate
			{
				ResetItems(datas.Count, false, false);
			}).SetId("DelayCall_RefreshItems_TeamJoinDialog");
		}

		protected override TeamJoinDialogIVH CreateViewsHolder(int itemIndex)
		{
			TeamJoinDialogIVH teamJoinDialogIVH = new TeamJoinDialogIVH();
			teamJoinDialogIVH.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return teamJoinDialogIVH;
		}

		protected override void UpdateViewsHolder(TeamJoinDialogIVH newOrRecycled)
		{
			newOrRecycled.pItem.InitData(newOrRecycled.ItemIndex, datas[newOrRecycled.ItemIndex]);
		}
	}
}
