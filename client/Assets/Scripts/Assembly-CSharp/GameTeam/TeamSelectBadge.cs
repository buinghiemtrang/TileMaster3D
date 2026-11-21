namespace GameTeam
{
	public class TeamSelectBadge : DialogBase
	{
		protected override void Awake()
		{
			base.Awake();
			DispatcherMgr.On<string>("TeamSelectedBadge", _Close);
		}

		public void Show()
		{
			_Open();
		}

		public void ClickClose(MButton btn)
		{
			_Close();
		}

		private void _Close(string badgeName)
		{
			_Close();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			DispatcherMgr.Off<string>("TeamSelectedBadge", _Close);
		}
	}
}
