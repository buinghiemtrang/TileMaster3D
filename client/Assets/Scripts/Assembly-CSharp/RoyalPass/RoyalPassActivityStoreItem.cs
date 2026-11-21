using Festival;
using UnityEngine;
using UnityEngine.UI;

namespace RoyalPass
{
	public class RoyalPassActivityStoreItem : MonoBehaviour
	{
		[SerializeField]
		private Image medalImg;

		[SerializeField]
		private Image rewardBoxImg;

		[SerializeField]
		private Text energyUpLimitText;

		public void Init()
		{
			int[,] paidReward = Singleton<RoyalPassActivityMgr>.I.GetPaidReward();
			Sprite sprite = null;
			string text = null;
			for (int i = 0; i < paidReward.GetLength(0); i++)
			{
				if (paidReward[i, 0] == 23)
				{
					if ((bool)medalImg)
					{
						Global.GetContents(PropID.Medal, paidReward[i, 1], out sprite, out text);
						medalImg.sprite = sprite;
					}
				}
				else if (paidReward[i, 0] == 24)
				{
					if ((bool)energyUpLimitText)
					{
						energyUpLimitText.text = paidReward[i, 1].ToString();
					}
				}
				else if ((bool)rewardBoxImg)
				{
					rewardBoxImg.sprite = Singleton<FestivalMgr>.I.LoadAsset<Sprite>("Festival/RoyalPassActivity/{0}/RewardBox");
				}
			}
		}
	}
}
