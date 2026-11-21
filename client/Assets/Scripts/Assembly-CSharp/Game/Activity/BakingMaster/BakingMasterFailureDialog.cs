using GameScene_Element;
using GameScene_Level;
using UnityEngine;

namespace Game.Activity.BakingMaster
{
	public class BakingMasterFailureDialog : MonoBehaviour
	{
		private DialogShow _pDialog;

		[SerializeField]
		private GameObject objControlTile;

		[SerializeField]
		private GameObject objTimeTile;

		private bool _bTimeOut;

		private void Start()
		{
			_pDialog = GetComponent<DialogShow>();
			objTimeTile.SetActive(_bTimeOut);
			objControlTile.SetActive(!_bTimeOut);
			ActivityHttpMgr.I.PostActivityGrade(0, 0);
			AudioMgr.I.PauseMusic();
			int totalNumber = ElementsMgr.I.Data.TotalNumber;
			int remainingNumber = ElementsMgr.I.Data.RemainingNumber;
			LogEvent.I.Level_Fail(LevelMgr.Data.Type, remainingNumber, totalNumber, remainingNumber, _bTimeOut, false);
		}

		public void ClickBtnClose(MButton button)
		{
			BlackCutDialog.Show(delegate
			{
				AudioMgr.I.ResumeMusic();
				GiftDataMgr.ins.ResetShowPopStatus(true);
				SceneMgr.LoadScene("MainScene", "GameScene", delegate
				{
					BlackCutDialog.Hide();
					SceneMgr.SetData("BakingMasterActivity");
				});
			});
			_pDialog.ClickClose();
		}

		public static BakingMasterFailureDialog Create(bool bTimeOut)
		{
			BakingMasterFailureDialog bakingMasterFailureDialog = SceneMgr.AddTopNode<BakingMasterFailureDialog>("Prefabs/Activity/BakingMaster/ActivityFailureDialog");
			bakingMasterFailureDialog._bTimeOut = bTimeOut;
			return bakingMasterFailureDialog;
		}
	}
}
