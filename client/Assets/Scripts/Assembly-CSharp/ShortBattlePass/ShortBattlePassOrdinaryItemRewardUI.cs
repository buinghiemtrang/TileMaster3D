using Festival;
using UnityEngine;
using UnityEngine.UI;

namespace ShortBattlePass
{
	public class ShortBattlePassOrdinaryItemRewardUI : MonoBehaviour
	{
		[SerializeField]
		private bool isFree;

		[SerializeField]
		private GameObject rewardObj;

		[SerializeField]
		private Image iconImg;

		[SerializeField]
		private Text numTex;

		[SerializeField]
		private GameObject receiveBtn;

		[SerializeField]
		private GameObject hookObj;

		[SerializeField]
		private GameObject lockObj;

		[SerializeField]
		private Image chestImg;

		private RewardInfo _info;

		private bool _isReceived;

		private bool _isCanReceive;

		private bool _isHasChest;

		private int _chestName;

		private void Awake()
		{
			DispatcherMgr.On("ShortBattlePass_Unlock", _Unlock);
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off("ShortBattlePass_Unlock", _Unlock);
		}

		public void Init(RewardInfo info)
		{
			_info = info;
			if (!isFree)
			{
				_chestName = info.Config.CostBox;
				_isReceived = info.State == RewardState.Cost || info.State == RewardState.All;
				_isCanReceive = !_isReceived && info.IsSatisfyCondition && info.IsPaid;
				lockObj.SetActive(!info.IsPaid);
			}
			else
			{
				_chestName = info.Config.FreeBox;
				_isReceived = info.State == RewardState.Free || info.State == RewardState.All;
				_isCanReceive = !_isReceived && info.IsSatisfyCondition;
			}
			_isHasChest = _chestName != -1;
			rewardObj.SetActive(_chestName == -1);
			chestImg.gameObject.SetActive(_isHasChest);
			hookObj.SetActive(_isReceived);
			receiveBtn.SetActive(_isCanReceive);
			if (!_isHasChest)
			{
				int[,] array = (isFree ? info.Config.freeReward : info.Config.CostReward);
				if (array.GetLength(0) == 1)
				{
					Global.GetContents((PropID)array[0, 0], array[0, 1], out Sprite sprite, out string text);
					iconImg.sprite = sprite;
					numTex.text = text;
				}
			}
			else
			{
				chestImg.sprite = Singleton<FestivalMgr>.I.LoadAsset<Sprite>("Festival/RoyalPassActivity/{0}/Chest" + _chestName);
			}
		}

		private void _Unlock()
		{
			if (base.gameObject.activeSelf && _info != null && !isFree)
			{
				_info.IsPaid = true;
				Init(_info);
			}
		}

		public void ClickReceive(MButton btn)
		{
			if (_isReceived)
			{
				Language.LanguageData languageDataByKey = Language.LanguageManager.Instance.GetLanguageDataByKey("Language358");
				if (languageDataByKey != null)
				{
					_CreateTip().ShowWord(base.transform, languageDataByKey.value);
				}
				return;
			}
			int[,] array = (isFree ? _info.Config.freeReward : _info.Config.CostReward);
			if (_isCanReceive)
			{
				btn.interactable = false;
				RewardState state = Singleton<ShortBattlePassMgr>.I.Receive(RewardItemType.Ordinary, _info.Config.id, isFree);
				Global.CreateReward(array, _isHasChest ? RewardType.RoyalPassActivity : RewardType.None, REWARD_PLACE.None, isFree ? "LittlePassFreeReward" : "LittlePassBuyReward").SetChestSkin(_isHasChest ? string.Format("0{0}", _chestName) : string.Empty).SetComplete(delegate
				{
					_info.State = state;
					Init(_info);
					btn.interactable = true;
				});
			}
			else if (_isHasChest)
			{
				_CreateTip().ShowReward(base.transform, array);
			}
			else
			{
				Language.LanguageData languageDataByKey2 = Language.LanguageManager.Instance.GetLanguageDataByKey("Language471");
				if (languageDataByKey2 != null)
				{
					_CreateTip().ShowWord(base.transform, languageDataByKey2.value);
				}
			}
		}

		private ShortBattlePassRewardTip _CreateTip()
		{
			ShortBattlePassRewardTip shortBattlePassRewardTip = SceneMgr.AddTopNode<ShortBattlePassRewardTip>("Prefabs/Activity/ShortBattlePass/ShortBattlePassRewardTip");
			shortBattlePassRewardTip.yOffset = -50f;
			shortBattlePassRewardTip.xOffset = 30f;
			return shortBattlePassRewardTip;
		}
	}
}
