public class TeamNetworkTip : NetworkTips
{
	public static TeamNetworkTip I;

	protected override void Awake()
	{
		base.Awake();
		I = this;
	}
}
