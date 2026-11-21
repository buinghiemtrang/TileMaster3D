namespace UGUI_Selected
{
	public class SelectActivateNode : SelectBase
	{
		public override void Select(bool isSelected)
		{
			gameObject.SetActive(isSelected);
		}
	}
}
