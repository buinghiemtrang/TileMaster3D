using UnityEngine;

namespace ActivityEntrance
{
	public class ActivityTotalEntrance : MonoBehaviour
	{
		[SerializeField]
		private MButton entranceBtn;

		private void Awake()
		{
			entranceBtn.onClick.AddListener(_ClickEntrance);
		}

		private void _ClickEntrance(MButton btn)
		{
			SceneMgr.AddTopNode<ActivityEntranceDialog>("Prefabs/Activity/Entrance/ActivityEntranceDialog")._Open();
		}
	}
}
