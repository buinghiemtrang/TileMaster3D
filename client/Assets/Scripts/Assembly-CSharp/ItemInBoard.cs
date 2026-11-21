using GameTeam;
using UnityEngine;

public class ItemInBoard : MonoBehaviour
{
	[HideInInspector]
	public BoardInfoType infoType;

	public float height = -1f;

	protected BoardsInfo.BoardInfo boardInfo;

	public virtual void Init(BoardsInfo.BoardInfo info)
	{
		boardInfo = info;
		infoType = info.GetInfoType();
	}
}
