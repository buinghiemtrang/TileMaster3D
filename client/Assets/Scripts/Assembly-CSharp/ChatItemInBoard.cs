using System;
using FQDev.WordBlocker;
using GameTeam;
using UnityEngine;
using UnityEngine.UI;

public class ChatItemInBoard : ItemInBoard
{
	[Serializable]
	public class ChatItemUI
	{
		[SerializeField]
		private Text nameTex;

		[SerializeField]
		private FilteredText contentFilteredTex;

		[SerializeField]
		private Text contentTex;

		[SerializeField]
		private Text timeTex;

		public float Init(BoardsInfo.BoardInfo boardInfo)
		{
			nameTex.text = boardInfo.UserName;
			contentFilteredTex.Text = boardInfo.Content;
			timeTex.text = boardInfo.Created.ToString("yyyy-MM-dd HH:mm:ss");
			return contentTex.preferredHeight + 93f;
		}
	}

	[SerializeField]
	private ChatItemUI[] itemsUI;

	[SerializeField]
	private GameObject ownItem;

	[SerializeField]
	private GameObject otherItem;

	public override void Init(BoardsInfo.BoardInfo info)
	{
		if (info == null)
		{
			return;
		}
		base.Init(info);
		if (itemsUI.Length != 2)
		{
			return;
		}
		bool isOwn = info.UserId.Equals(DB.Account.Userid);
		ownItem.SetActive(isOwn);
		otherItem.SetActive(!isOwn);
		height = itemsUI[isOwn ? 0 : 1].Init(info);
	}
}
