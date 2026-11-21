public class RankMaskCtl : MaskCtl
{
	public static RankMaskCtl I;

	protected override void Awake()
	{
		base.Awake();
		I = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		I = null;
	}
}
