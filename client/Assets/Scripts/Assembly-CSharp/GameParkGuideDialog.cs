using UnityEngine;
using UnityEngine.UI;

public class GameParkGuideDialog : DialogBase
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private MButton playBtn;

	[SerializeField]
	private Text titleTex;

	[SerializeField]
	private Text descTex;

	protected override void Awake()
	{
		base.Awake();
		playBtn.onClick.AddListener(_ClickPlay);
	}

	public static void Create(GameParkType type)
	{
		GameParkTickets parkTickets = DB.Other.GetParkTickets(type);
		if (parkTickets.playFirst)
		{
			parkTickets.playFirst = false;
			DB.Other.SetParkTickets(type.ToString(), parkTickets);
			SceneMgr.AddTopNode<GameParkGuideDialog>("Prefabs/GameParkGuideDialog").Show(type);
		}
	}

	public void Show(GameParkType type)
	{
		icon.sprite = LoadMgr.LoadSprite("GuideIcon/" + type);
		titleTex.SetLKey("ParkGame" + type);
		descTex.SetLKey("ParkGame" + type + "_Desc");
		_Open();
	}

	private void _ClickPlay(MButton btn)
	{
		_Close();
	}
}
