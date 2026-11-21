public class TeamMaskCtl : MaskCtl
{
	public static TeamMaskCtl I;

	protected override void Awake()
	{
		base.Awake();
		I = this;
	}
}
