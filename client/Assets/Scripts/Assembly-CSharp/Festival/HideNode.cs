namespace Festival
{
	public class HideNode : OperationBase
	{
		protected override void Awake()
		{
			base.Awake();
			if (PointInfo != null)
			{
				gameObject.SetActive(PointInfo.information.Equals("1"));
			}
		}
	}
}
