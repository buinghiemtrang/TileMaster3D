using GameTeam;
using UnityEngine;
using UnityEngine.UI;

public class JoinOrLeaveItemInBoard : ItemInBoard
{
	[SerializeField]
	private Text msgTex;

	[SerializeField]
	private Image imageBg;

	[SerializeField]
	private Sprite[] spriteBgs;

	[SerializeField]
	private Color[] textColors;

	public override void Init(BoardsInfo.BoardInfo info)
	{
		if (info == null)
		{
			return;
		}
		base.Init(info);
		if (infoType == BoardInfoType.Join)
		{
			msgTex.SetLKey("Language208", boardInfo.UserName);
			imageBg.sprite = spriteBgs[0];
			msgTex.color = textColors[0];
			return;
		}
		if (boardInfo.GetInfoType() != BoardInfoType.Leave)
		{
			return;
		}
		msgTex.SetLKey("Language209", boardInfo.UserName);
		imageBg.sprite = spriteBgs[1];
		msgTex.color = textColors[1];
	}
}
