using Festival;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace RoyalPass
{
	public class RoyalPassActivityOrdinaryItemRewardUI : MonoBehaviour
	{
		[SerializeField]
		private bool isFree;

		[SerializeField]
		private GameObject paidRewardsObj;

		[SerializeField]
		private Image medalImg;

		[SerializeField]
		private Image rewardBoxImg;

		[SerializeField]
		private Text energyUpLimitText;

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
		private SkeletonGraphic lockAnim;

		[SerializeField]
		private Sprite coinSprite;

		[SerializeField]
		private Image chestImg;

		private RoyalPassActivityRewardInfo info;

		private bool isReceived;

		private bool isCanReceive;

		private bool isHasChest;

		private int chestIcon;

		private void Awake()
		{
			DispatcherMgr.On("RoyalPassActivity_Unlock", _OnUnlock);
		}

		public void Init(RoyalPassActivityRewardInfo info)
		{
			this.info = info;
			if ((bool)paidRewardsObj)
			{
				paidRewardsObj.SetActive(info.cfg.grade == 0);
			}
			if (info.cfg.grade == 0)
			{
				Singleton<RoyalPassActivityMgr>.I.SetPaidRewardUI(medalImg, energyUpLimitText, rewardBoxImg);
			}
			if (!isFree)
			{
				chestIcon = info.cfg.paidChestIcon;
				isReceived = info.state == RewardState.ReceivedPaid || info.state == RewardState.ReceivedAll;
				isCanReceive = !isReceived && info.isSatisfyCondition && info.isUnlockPaidReward;
				if ((bool)lockAnim)
				{
					lockAnim.gameObject.SetActive(!info.isUnlockPaidReward);
					if (!info.isUnlockPaidReward)
					{
						lockAnim.AnimationState.SetAnimation(0, "lock_idle", true);
					}
				}
			}
			else
			{
				chestIcon = info.cfg.freeChestIcon;
				isReceived = info.state == RewardState.ReceivedFree || info.state == RewardState.ReceivedAll;
				isCanReceive = !isReceived && info.isSatisfyCondition;
			}
			isHasChest = chestIcon != -1;
			rewardObj.SetActive((isFree || info.cfg.grade != 0) && !isHasChest);
			chestImg.gameObject.SetActive((isFree || info.cfg.grade != 0) && isHasChest);
			hookObj.SetActive(isReceived);
			receiveBtn.SetActive(isCanReceive);
			int[,] array;
			if (!isFree)
			{
				if (info.cfg.grade == 0)
				{
					return;
				}
				if (isHasChest)
				{
					chestImg.sprite = FestivalMgr.I.LoadAsset<Sprite>("Festival/RoyalPassActivity/{0}/Chest" + chestIcon);
					return;
				}
				array = info.cfg.paidReward;
			}
			else
			{
				if (isHasChest)
				{
					chestImg.sprite = FestivalMgr.I.LoadAsset<Sprite>("Festival/RoyalPassActivity/{0}/Chest" + chestIcon);
					return;
				}
				array = info.cfg.freeReward;
			}
			if (array.GetLength(0) != 1)
			{
				Debug.LogError("royal pass activity reward cfg error");
				return;
			}
			int num = array[0, 0];
			Sprite sprite;
			string text;
			Global.GetContents((PropID)num, array[0, 1], out sprite, out text);
			iconImg.sprite = ((num == 0) ? coinSprite : sprite);
			numTex.text = text;
		}

		private void _OnUnlock()
		{
			if (base.gameObject.activeSelf && info != null)
			{
				info.isUnlockPaidReward = true;
				if ((bool)lockAnim)
				{
					lockAnim.AnimationState.SetAnimation(0, "unlock", false);
					lockAnim.AnimationState.Complete += delegate
					{
						Init(info);
					};
				}
			}
		}

		public void ClickReceive(MButton btn)
		{
			if (isReceived)
			{
				Language.LanguageData languageDataByKey = Language.LanguageManager.Instance.GetLanguageDataByKey("Language358");
				if (languageDataByKey != null)
				{
					_CreateTip().ShowWord(base.transform, languageDataByKey.value);
				}
				return;
			}
			int[,] array = (isFree ? info.cfg.freeReward : info.cfg.paidReward);
			if (isCanReceive)
			{
				btn.interactable = false;
				RewardState state = Singleton<RoyalPassActivityMgr>.I.Receive(RewardItemType.Ordinary, info.cfg.id, isFree);
				Global.CreateReward(array, isHasChest ? RewardType.RoyalPassActivity : RewardType.None, REWARD_PLACE.None, isFree ? "PassFreeReward" : "PassBuyReward").SetChestSkin(isHasChest ? string.Format("0{0}", chestIcon) : string.Empty).SetComplete(delegate
				{
					info.state = state;
					Init(info);
					btn.interactable = true;
				});
			}
			else if (info.cfg.grade < 1)
			{
				SceneMgr.AddTopNode<RoyalPassActivityActivateDialog>("Prefabs/Activity/RoyalPass/{0}_RoyalPassActivityActivateDialog").Show();
			}
			else if (isHasChest)
			{
				_CreateTip().ShowReward(base.transform, array);
			}
			else
			{
				Language.LanguageData languageDataByKey2 = Language.LanguageManager.Instance.GetLanguageDataByKey("Language360");
				if (languageDataByKey2 != null)
				{
					_CreateTip().ShowWord(base.transform, languageDataByKey2.value);
				}
			}
		}

		private RoyalPassActivityRewardTip _CreateTip()
		{
			RoyalPassActivityRewardTip royalPassActivityRewardTip = SceneMgr.AddTopNode<RoyalPassActivityRewardTip>("Prefabs/Activity/RoyalPass/RoyalPassActivityRewardTip");
			royalPassActivityRewardTip.yOffset = -50f;
			royalPassActivityRewardTip.xOffset = 30f;
			return royalPassActivityRewardTip;
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off("RoyalPassActivity_Unlock", _OnUnlock);
		}
	}
}
