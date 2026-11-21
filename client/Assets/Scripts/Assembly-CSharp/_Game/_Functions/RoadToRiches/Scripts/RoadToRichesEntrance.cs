using System;
using DG.Tweening;
using FunctionEject;
using UnityEngine;
using UnityEngine.UI;

namespace _Game._Functions.RoadToRiches.Scripts
{
	public class RoadToRichesEntrance : EjectItem
	{
		[SerializeField]
		private Text _ttTime;

		[SerializeField]
		private Text _ttCount;

		[SerializeField]
		private MButton _btnEntrance;

		private void Awake()
		{
			Init();
			_btnEntrance.onClick.AddListener(ClickEntrance);
			DispatcherMgr.On("Update_UserData", Init);
		}

		private void OnDestroy()
		{
			_ttTime.DOKill(false);
			DispatcherMgr.Off("Update_UserData", Init);
		}

		public override void Init()
		{
			RoadToRichesMgr.I.Initialized();
			DateTime endTime = RoadToRichesMgr.I.EndTime;
			if (endTime == default(DateTime))
			{
				gameObject.SetActive(false);
			}
			else
			{
				gameObject.SetActive(true);
				_ttCount.text = DB.Other.RoadToRichesCount.ToString();
				int start = (int)(endTime - DateTime.Now).TotalSeconds;
				_ttTime.DOCounter(start, 0, start, Util.Helper.FormatTimerD).OnComplete(Init);
			}
		}

		public override bool Eject()
		{
			if (gameObject.activeSelf)
			{
				DateTime endTime = RoadToRichesMgr.I.EndTime;
				if (endTime != default(DateTime))
				{
					RoadToRichesDialog.Create();
					return true;
				}
			}
			return false;
		}

		public override void ForceEject()
		{
			RoadToRichesDialog.Create();
		}

		private void ClickEntrance(MButton button)
		{
			RoadToRichesDialog.Create();
		}
	}
}
