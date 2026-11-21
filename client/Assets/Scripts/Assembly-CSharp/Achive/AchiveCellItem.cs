using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Achive
{
	public class AchiveCellItem : MonoBehaviour
	{
		public Image ImageIcon;

		public Image IamgeBarBg;

		public Image IamgeBar;

		public Image ImagePropIcon;

		public Text TextName;

		public Text TextDesc;

		public Text Textbar;

		public Text TextReceived;

		public Text TextPropNum;

		public GameObject gameObjectUnreceived;

		public MButton BtnGet;

		public AchiveStarItem cellItem;

		public Transform tfStarRoot;

		private AchiveStatus _status;

		private cfg_achive _cfg;

		private int _iLevelCount;

		private int _iCurLevel;

		private StringBuilder rewardsSB;

		public void InitData(int index, cfg_achive cfg)
		{
			_cfg = cfg;
			UpdateData();
		}

		public void UpdateData()
		{
			TextName.SetLKey(_cfg.TitleLangId);
			ImageIcon.sprite = LoadMgr.LoadRes<Sprite>("AchiveIcon/" + _cfg.Icon);
			ImageIcon.SetNativeSize();
			int[] array = _cfg.Values.ToInt('|');
			_iLevelCount = array.Length;
			_iCurLevel = AchiveMgr.I.GetAchieveLevel(_cfg.id);
			int reachCount = AchiveMgr.I.GetReachCount((AchieveType)_cfg.Type);
			int index = (_iCurLevel < _iLevelCount) ? _iCurLevel : (_iCurLevel - 1);
			int totalCount = array[index];
			TextDesc.SetLKey(_cfg.DescLangId, totalCount);
			int num = (_iCurLevel < _iLevelCount) ? 2 : 0;
			if (reachCount < totalCount)
			{
				num = 1;
			}
			_status = (AchiveStatus)num;
			UpdateStar(_iLevelCount, _iCurLevel);
			UpdateStatusUI(reachCount, totalCount);
			UpdateReward();
		}

		protected void UpdateStar(int levelCount, int curLevel)
		{
			int childCount = tfStarRoot.childCount;
			for (int num = childCount - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(tfStarRoot.GetChild(num).gameObject);
			}
			for (int i = 1; i <= levelCount; i++)
			{
				AchiveStarItem achiveStarItem = UnityEngine.Object.Instantiate(cellItem, tfStarRoot);
				achiveStarItem.gameObject.SetActive(value: true);
				achiveStarItem.SetShow(i <= curLevel);
			}
		}

		protected void UpdateStatusUI(int reachCount, int totalCount)
		{
			gameObjectUnreceived.SetActive(_status != AchiveStatus.Received);
			TextReceived.gameObject.SetActive(_status == AchiveStatus.Received);
			BtnGet.gameObject.SetActive(_status == AchiveStatus.Finished);
			IamgeBarBg.gameObject.SetActive(_status == AchiveStatus.Unfinished);
			if (_status == AchiveStatus.Unfinished)
			{
				Textbar.text = string.Format("{0}/{1}", reachCount, totalCount);
				IamgeBar.fillAmount = (float)reachCount / (float)totalCount;
			}
		}

		protected void UpdateReward()
		{
			if (_status == AchiveStatus.Received)
			{
				return;
			}
			string[] array = _cfg.RewardTypesAndroid.Split('|');
			string[] array2 = _cfg.RewardAndroid.Split('|');
			int num = _iCurLevel - ((_iCurLevel >= _iLevelCount) ? 1 : 0);
			int[] array3 = array[num].ToInt(',');
			int[] array4 = array2[num].ToInt(',');
			int num2 = array4[0];
			int num3 = array3[0];
			ImagePropIcon.gameObject.SetActive(num3 != -1);
			TextPropNum.gameObject.SetActive(num3 != -1);
			if (num3 != -1)
			{
				ImagePropIcon.sprite = LoadMgr.LoadRes<Sprite>(string.Format("Props/prop{0}", num3));
				TextPropNum.text = ((num3 == 0) ? num2.ToString() : string.Format("x{0}", num2));
				rewardsSB = new StringBuilder();
				for (int i = 0; i < array3.Length; i++)
				{
					rewardsSB.AppendFormat("{0}|{1}", array3[i], array4[i]);
					if (i != array3.Length - 1)
					{
						rewardsSB.Append(",");
					}
				}
			}
		}

		public void OnClick(MButton btn)
		{
			int[] array = _cfg.Values.ToInt('|');
			int reachCount = AchiveMgr.I.GetReachCount((AchieveType)_cfg.Type);
			int num = (_iCurLevel < _iLevelCount) ? _iCurLevel : (_iCurLevel - 1);
			if (_status == AchiveStatus.Finished)
			{
				if (_iLevelCount <= _iCurLevel)
				{
					return;
				}
				AchiveMgr.I.SetAchieveLevel(_cfg.id, _iCurLevel + 1);
				AchiveMgr.I.UpdateCanReceiveData(_cfg);
				UpdateData();
				AchiveMgr.I.SortAchieveCfg();
				int num2 = (_iCurLevel < _iLevelCount) ? array[_iCurLevel] : array[_iCurLevel - 1];
				AchiveMgr.I.SetAchiveTipsData(_cfg.id, num2 <= reachCount);
				DispatcherMgr.Emit("Show_Move_Star", tfStarRoot.GetChild(num));
				LogEvent.I.Track("Archievement_Receive").Add("ArchievementType", _cfg.Type).Add("ArchievementLevel", _iCurLevel).Add("ArchievementNum", AchiveMgr.I.GetCurAchieveReachCount()).CommonAttr(true).Send(false);
				LogEvent.I.Track("").Add("ArchievementNum", AchiveMgr.I.GetCurAchieveReachCount()).UserSet();
			}
			else if (_status == AchiveStatus.Unfinished)
			{
				AchiveUnFinished achiveUnFinished = SceneMgr.AddTopNode<AchiveUnFinished>("Prefabs/Achive/AchiveUnFinished");
				achiveUnFinished.InitInfo(_cfg, _status, _iCurLevel);
				achiveUnFinished.finish_callback = JumpToGame;
			}
		}

		public void JumpToGame()
		{
			Action action = delegate
			{
				BlackCutDialog.EnterGame();
			};
			if (DB.Bag.JudgeHasEnergy())
			{
				action();
				return;
			}
			Action goldInsufficientCall = delegate
			{
				SceneMainBottom.instance.btnShop.ClickSelf();
			};
			EnergyDialog energyDialog = SceneMgr.AddTopNode<EnergyDialog>("Prefabs/EnergyDialog");
			energyDialog.cback = action;
			energyDialog.goldInsufficientCall = goldInsufficientCall;
		}

		public void ShowRewardDialog()
		{
			if (rewardsSB != null)
			{
				Global.CreateReward(rewardsSB.ToString());
			}
		}
	}
}
