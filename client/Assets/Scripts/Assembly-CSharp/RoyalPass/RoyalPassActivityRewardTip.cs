using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoyalPass
{
	public class RoyalPassActivityRewardTip : MonoBehaviour
	{
		[SerializeField]
		private Transform itemParentTran;

		[SerializeField]
		private Transform contentsTran;

		[SerializeField]
		private GameObject itemObj;

		[SerializeField]
		private Transform nodeTran;

		[SerializeField]
		private GameObject upArrowTran;

		[SerializeField]
		private GameObject downArrowTran;

		[SerializeField]
		private Button closeButton;

		[HideInInspector]
		public float yOffset;

		[HideInInspector]
		public float xOffset;

		private List<GameObject> items;

		private void Awake()
		{
			itemObj.SetActive(false);
			closeButton.onClick.AddListener(delegate
			{
				Object.Destroy(base.gameObject);
			});
		}

		public void ShowWord(Transform target, string word)
		{
			itemParentTran.gameObject.SetActive(false);
			_SetPos(target, contentsTran);
			contentsTran.FindC<Text>("Word").text = word;
		}

		public void ShowReward(Transform target, int[,] reward)
		{
			if (reward != null)
			{
				_SetPos(target, itemParentTran);
				SetReward(reward);
			}
		}

		public void ShowReward(Transform target, int[,] reward, bool isUp)
		{
			if (reward != null)
			{
				Vector3 pos = base.transform.InverseTransformPoint(target.position);
				_SetPos(itemParentTran.RT(), pos, isUp, pos.x < 0f);
				SetReward(reward);
			}
		}

		private void SetReward(int[,] reward)
		{
			Sprite sprite = null;
			string text = null;
			contentsTran.gameObject.SetActive(false);
			if (items == null)
			{
				items = new List<GameObject>();
			}
			int length = reward.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				GameObject gameObject = Object.Instantiate(itemObj, itemParentTran);
				gameObject.SetActive(true);
				Global.GetContents((PropID)reward[i, 0], reward[i, 1], out sprite, out text);
				gameObject.transform.FindC<Image>("Icon").sprite = sprite;
				gameObject.transform.FindC<Text>("Value").text = text;
				gameObject.transform.Find("Add").gameObject.SetActive(i < length - 1);
			}
		}

		public void SetContentsOffPosition(float x, float y)
		{
			Vector3 localPosition = itemParentTran.localPosition;
			itemParentTran.localPosition = new Vector3(localPosition.x + x, localPosition.y + y, localPosition.z);
		}

		private void _SetPos(Transform target, Transform node)
		{
			Vector3 pos = base.transform.InverseTransformPoint(target.position);
			RectTransform rectTransform = node.RT();
			Vector2 sizeDelta = rectTransform.sizeDelta;
			Vector3 localPosition = rectTransform.localPosition;
			float num = yOffset + (pos.y - sizeDelta.y) + localPosition.y;
			float num2 = 0f - SceneMgr.heightH;
			_SetPos(rectTransform, pos, num2 < num, pos.x < 0f);
		}

		private void _SetPos(RectTransform rt, Vector3 pos, bool isUp, bool isLeft)
		{
			upArrowTran.SetActive(isUp);
			downArrowTran.SetActive(!isUp);
			rt.pivot = new Vector2(isLeft ? 0f : 1f, isUp ? 1f : 0f);
			Vector3 localPosition = rt.localPosition;
			rt.SetLocPosX(isLeft ? (0f - localPosition.x) : localPosition.x);
			rt.SetLocPosY(isUp ? localPosition.y : (0f - localPosition.y));
			nodeTran.localPosition = new Vector3(pos.x + (isLeft ? (0f - xOffset) : xOffset), pos.y + (isUp ? yOffset : (0f - yOffset)), pos.z);
		}

		public static RoyalPassActivityRewardTip Create()
		{
			return SceneMgr.AddTopNode<RoyalPassActivityRewardTip>("Prefabs/Activity/RoyalPass/RoyalPassActivityRewardTip");
		}
	}
}
