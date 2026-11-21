using System;
using DG.Tweening;
using FunctionEject;
using UnityEngine;
using UnityEngine.UI;

namespace Turntable
{
	public class TurntableEntrance : EjectItem
	{
		[SerializeField]
		private Text _timeTxt;

		[SerializeField]
		private MButton _entranceButton;

		private void Start()
		{
			_entranceButton.onClick.AddListener(ClickEntrance);
		}

		public override void Init()
		{
			Singleton<TurntableManager>.I.RefreshData();
			if (Singleton<TurntableManager>.I.IsOpen && Singleton<TurntableManager>.I.LeftCount > 0)
			{
				base.gameObject.SetActive(true);
				int num = (int)(Singleton<TurntableManager>.I.EndTime - DateTime.Now).TotalSeconds;
				if (num > 0)
				{
					_timeTxt.DOCounter(num, 0, (float)num, Util.Helper.FormatTimerD).OnComplete(Init);
				}
				else
				{
					base.gameObject.SetActive(false);
				}
			}
			else
			{
				base.gameObject.SetActive(false);
			}
		}

		public override bool Eject()
		{
			if (!DB.Turntable.HasDailyShowPop && Singleton<TurntableManager>.I.IsOpen && Singleton<TurntableManager>.I.LeftCount >= 1)
			{
				DB.Turntable.HasDailyShowPop = true;
				Singleton<TurntableManager>.I.ShowTurntable(RefreshEntrance);
				return true;
			}
			return false;
		}

		public override void ForceEject()
		{
		}

		private void ClickEntrance(MButton button)
		{
			Singleton<TurntableManager>.I.ShowTurntable(RefreshEntrance);
		}

		private void RefreshEntrance(bool isWatch)
		{
			if (isWatch)
			{
				Init();
			}
		}

		private void OnDestroy()
		{
			_timeTxt.DOKill();
		}
	}
}
