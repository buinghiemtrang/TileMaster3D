using System;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.DataHelpers;

namespace Com.TheFallenGames.OSA.CustomParams
{
	[Serializable]
	public abstract class BaseParamsWithPrefabAndLazyData<TData> : BaseParamsWithPrefab
	{
		public LazyList<TData> Data { get; set; }

		public Func<int, TData> NewModelCreator { get; set; }

		public override void InitIfNeeded(IOSA iAdapter)
		{
			base.InitIfNeeded(iAdapter);
			if (Data == null)
			{
				Data = new LazyList<TData>(NewModelCreator, 0);
			}
		}
	}
}
