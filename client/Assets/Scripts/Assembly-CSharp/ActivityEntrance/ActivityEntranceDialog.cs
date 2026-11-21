using UnityEngine;

namespace ActivityEntrance
{
	public class ActivityEntranceDialog : DialogBase
	{
		[SerializeField]
		private MButton closeBtn;

		protected override void Awake()
		{
			base.Awake();
			closeBtn.onClick.AddListener(delegate
			{
				_Close();
			});
			DispatcherMgr.On("ActivityEntrance_ActivityEnterOtherPage", _ActivityEnterOtherPage);
		}

		public void Show()
		{
			_Open();
		}

		private void _ActivityEnterOtherPage()
		{
			_Close();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			DispatcherMgr.Off("ActivityEntrance_ActivityEnterOtherPage", _ActivityEnterOtherPage);
		}
	}
}
