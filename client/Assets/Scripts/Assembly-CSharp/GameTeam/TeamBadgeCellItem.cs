using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameTeam
{
	public class TeamBadgeCellItem : MonoBehaviour
	{
		[Serializable]
		private class ItemInfo
		{
			public Image iconImg;

			public MButton button;

			public GameObject obj;
		}

		[SerializeField]
		private ItemInfo[] infos;

		private cfg_TeamBadge[] _badges;

		private void Awake()
		{
			if (infos != null)
			{
				for (int i = 0; i < infos.Length; i++)
				{
					infos[i].button.onClick.AddListener(_ClickSelect);
					infos[i].button.nTag = i;
				}
			}
		}

		public void Init(cfg_TeamBadge[] badges)
		{
			if (badges == null)
			{
				return;
			}
			_badges = badges;
			for (int i = 0; i < infos.Length; i++)
			{
				bool flag = i < badges.Length;
				infos[i].obj.SetActive(flag);
				if (flag)
				{
					infos[i].iconImg.sprite = LoadMgr.LoadSprite("TeamIcon/" + badges[i].iconName);
				}
			}
		}

		private void _ClickSelect(MButton btn)
		{
			DispatcherMgr.Emit("TeamSelectedBadge", _badges[btn.nTag].iconName);
		}
	}
}
