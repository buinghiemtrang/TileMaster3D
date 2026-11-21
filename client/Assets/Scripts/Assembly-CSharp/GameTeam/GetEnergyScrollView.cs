using System;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;

namespace GameTeam
{
	public class GetEnergyScrollView : OSA<GetEnergyPWP, GetEnergyIVH>
	{
		public Action onReceiveSuccess;

		private List<EnergysInfo.EnergyInfo> infos;

		protected override void Start()
		{
			base.Start();
			_Params.DefaultItemHeight = _Params.ItemPrefab.sizeDelta.y;
		}

		public void SetInfos(List<EnergysInfo.EnergyInfo> infos)
		{
			this.infos = infos;
			ResetItems(infos.Count, false, true);
		}

		protected override GetEnergyIVH CreateViewsHolder(int itemIndex)
		{
			GetEnergyIVH getEnergyIVH = new GetEnergyIVH();
			getEnergyIVH.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return getEnergyIVH;
		}

		protected override void UpdateViewsHolder(GetEnergyIVH newOrRecycled)
		{
			newOrRecycled.pItem.onReceiveSuccess = (guid) =>
			{
				int num = infos.FindIndex((EnergysInfo.EnergyInfo f) => f.GUID == guid);
				if (num != -1)
				{
					infos.RemoveAt(num);
					ResetItems(infos.Count, false, true);
				}
				if (onReceiveSuccess != null)
				{
					onReceiveSuccess();
				}
			};
			newOrRecycled.pItem.InitData(infos[newOrRecycled.ItemIndex]);
		}
	}
}
