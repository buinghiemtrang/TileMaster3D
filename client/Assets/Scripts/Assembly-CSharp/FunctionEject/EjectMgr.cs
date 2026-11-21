using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FunctionEject
{
	public class EjectMgr : QueueMgr<EjectItem>
	{
		public static EjectMgr I;

		[SerializeField]
		private List<EjectItem> items;

		private int _maxShowNum;

		protected override void Awake()
		{
		base.Awake();
		I = this;
	}

		private void OnDestroy()
		{
		I = null;
	}

		public void Init()
		{
		items.ForEach(delegate(EjectItem fe)
		{
			fe.Init();
		});
	}

		public void Refresh(bool isCheck)
		{
		Init();
		if (SceneMgr.showingDialogs.Count != 0 || !isCheck)
		{
			return;
		}
		string text = (SceneMgr.GetData() ?? string.Empty) as string;
		for (int i = 0; i < items.Count; i++)
		{
			if (text.Equals("Open" + items[i].functionType))
			{
				items[i].ForceEject();
				return;
			}
		}
		if (DB.Login.IsCostInGameScene && Singleton<LoginMgr>.I.ShowLoginGuide())
		{
			return;
		}
		_maxShowNum = cf_const.autoEjectDialogMaxNum;
		cf_const.autoEjectDialogOrder.ToIntList('|', ',').ForEach(delegate(List<int> fe)
		{
			fe.ForEach(delegate(int e)
			{
				EjectItem ejectItem = items.SingleOrDefault((EjectItem sd) => sd.functionType == (FunctionType)e);
				if ((bool)ejectItem)
				{
					Add(ejectItem);
				}
			});
		});
		Next(EjectType.Continue);
	}

		public void Next(EjectType type)
		{
		if (type != EjectType.Break)
		{
			while (_maxShowNum > 0)
			{
				EjectItem ejectItem = Execute();
				if (!ejectItem)
				{
					return;
				}
				if (ejectItem.Eject())
				{
					_maxShowNum--;
					return;
				}
			}
		}
		Clear();
	}
	}
}
