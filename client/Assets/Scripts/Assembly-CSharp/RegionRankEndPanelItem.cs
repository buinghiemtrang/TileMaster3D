public class RegionRankEndPanelItem : RegionRankPanelItem
{
	public override bool IsSelf()
	{
		// asm 0x11fbdcc: ldr x8,[x0,#0x88]; cmp x8,#0; cset → so con trỏ thô
		return (object)selfBg != null;
	}
}
