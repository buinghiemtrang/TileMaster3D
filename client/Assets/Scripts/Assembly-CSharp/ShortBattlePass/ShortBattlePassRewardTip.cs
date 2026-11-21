using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShortBattlePass
{
	public class ShortBattlePassRewardTip : MonoBehaviour
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

		[HideInInspector]
		public float yOffset;

		[HideInInspector]
		public float xOffset;

		private List<GameObject> _items;

		private void Awake()
		{
			itemObj.SetActive(value: false);
		}

		private void Update()
		{
			if (Input.touchCount == 0)
			{
				return;
			}
			for (int i = 0; i < Input.touchCount; i++)
			{
				if (Input.GetTouch(i).phase == TouchPhase.Began)
				{
					Object.Destroy(base.gameObject);
					break;
				}
			}
		}

		public void ShowWord(Transform target, string word)
		{
			itemParentTran.gameObject.SetActive(value: false);
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
				Vector3 vector = base.transform.InverseTransformPoint(target.position);
				_SetPos(itemParentTran.RT(), vector, isUp, vector.x < 0f);
				SetReward(reward);
			}
		}

		private void SetReward(int[,] reward)
		{
			contentsTran.gameObject.SetActive(value: false);
			if (_items == null)
			{
				_items = new List<GameObject>();
			}
			int length = reward.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				GameObject gameObject = Object.Instantiate(itemObj, itemParentTran);
				gameObject.SetActive(value: true);
				Global.GetContents((PropID)reward[i, 0], reward[i, 1], out Sprite sprite, out string text);
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

		private void _SetPos(Transform target, Component node)
		{
			Vector3 vector = base.transform.InverseTransformPoint(target.position);
			RectTransform rectTransform = node.RT();
			Vector2 sizeDelta = rectTransform.sizeDelta;
			Vector3 localPosition = rectTransform.localPosition;
			_SetPos(rectTransform, vector, yOffset + (vector.y - sizeDelta.y + localPosition.y) > 0f - SceneMgr.heightH, vector.x < 0f);
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
	}
}
