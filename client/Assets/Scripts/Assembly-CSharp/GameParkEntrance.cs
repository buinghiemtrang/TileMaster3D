using FunctionEject;

public class GameParkEntrance : EjectItem
{
	public MButton button;

	private void Awake()
	{
		base.gameObject.SetActive(false);
		button.onClick.AddListener(_ButtonClick);
	}

	public override void Init()
	{
		base.gameObject.SetActive(DB.User.Maxlv >= cf_const.parkEnterUnlockLv);
	}

	public override bool Eject()
	{
		return false;
	}

	public override void ForceEject()
	{
		SceneMgr.AddTopNode<DialogShow>("Prefabs/GameParkEntranceDialog");
		LogEvent.I.HomeIconClick("SmallGames");
		LogEvent.I.Track("Home_Frame_Open").Add("Name", "SmallGames").Add("OpenType", "Click")
			.CommonAttr(true)
			.Send(false);
	}

	private void _ButtonClick(MButton btn)
	{
		ForceEject();
	}
}
