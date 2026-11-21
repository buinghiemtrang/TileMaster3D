public class GetEnergyNetworkTip : NetworkTips
{
	public static GetEnergyNetworkTip I;

	protected override void Awake()
	{
		base.Awake();
		I = this;
	}
}
