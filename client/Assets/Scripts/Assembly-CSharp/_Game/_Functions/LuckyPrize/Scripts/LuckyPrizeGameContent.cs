using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Game._Functions.LuckyPrize.Scripts
{
	public class LuckyPrizeGameContent
	{
		public readonly Transform tfRoot;

		private readonly List<LuckyPrizeGameBlockItem> _ayItems;

		private readonly Transform _tfItemPrefab;

		private readonly Sprite _spOrigin;

		private readonly Vector3 _v3OriginPosition;

		public int GroupCount;

		public LuckyPrizeGameContent(Transform parent, Action<LuckyPrizeGameBlockItem> callback)
		{
			tfRoot = parent.Find("ObjContent");
			_tfItemPrefab = parent.Find("ObjItem");
			_spOrigin = _tfItemPrefab.FindC<Image>("Icon").sprite;
			Transform pos1 = tfRoot.Find("Pos1");
			_v3OriginPosition = pos1.localPosition;
			pos1.DestroySelf();
			_ayItems = new List<LuckyPrizeGameBlockItem>();
			_tfItemPrefab.gameObject.SetActive(false);
			for (int i = 0; i < 9; i++)
			{
				Transform transform = UnityEngine.Object.Instantiate(_tfItemPrefab, tfRoot);
				LuckyPrizeGameBlockItem item = new LuckyPrizeGameBlockItem(transform, i);
				transform.SetAsFirstSibling();
				transform.gameObject.SetActive(true);
				transform.GetComponent<MButton>().onClick.AddListener(delegate
				{
					callback(item);
				});
				_ayItems.Add(item);
			}
		}

		public void Destructor()
		{
		}

		private void InitKinds()
		{
			if (DB.Other.LuckyPrizeKind.Count == 9)
			{
				return;
			}
			List<int> list = DB.Other.LuckyPrizeKind.Get();
			list.Clear();
			int num = MMath.Random(1, 30);
			int num2 = MMath.Random(1, 30);
			while (num == num2)
			{
				num2 = MMath.Random(1, 30);
			}
			for (int i = 0; i < 3; i++)
			{
				list.Add(num);
			}
			for (int j = 0; j < 6; j++)
			{
				list.Add(num2);
			}
			list.SortR();
			DB.Other.LuckyPrizeKind.Save();
		}

		public void RefreshItem()
		{
			InitKinds();
			GroupCount = 3;
			Rect rect = _tfItemPrefab.Rect();
			List<int> list = DB.Other.LuckyPrizeKind.Get();
			float x = _v3OriginPosition.x;
			float y = _v3OriginPosition.y;
			int num = 0;
			for (int i = 0; i < _ayItems.Count; i++)
			{
				LuckyPrizeGameBlockItem luckyPrizeGameBlockItem = _ayItems[i];
				if (luckyPrizeGameBlockItem.tfRoot.parent != tfRoot)
				{
					luckyPrizeGameBlockItem.tfRoot.SetParent(tfRoot, false);
				}
				luckyPrizeGameBlockItem.tfRoot.SetAsFirstSibling();
				luckyPrizeGameBlockItem.tfRoot.gameObject.SetActive(true);
				luckyPrizeGameBlockItem.tfRoot.localPosition = new Vector3(x, y + rect.height * 0.4f * (float)num, 0f);
				luckyPrizeGameBlockItem.Initialized(num == 0, _spOrigin, list[i]);
				num++;
				if (num == 3)
				{
					x += rect.width + 15f;
					y = _v3OriginPosition.y;
					num = 0;
				}
			}
		}

		public void SetBlockItem(int index)
		{
			LuckyPrizeGameBlockItem luckyPrizeGameBlockItem = _ayItems[index];
			luckyPrizeGameBlockItem.IsActive = true;
			luckyPrizeGameBlockItem.imgIcon.gameObject.SetActive(true);
			luckyPrizeGameBlockItem.tfMask.gameObject.SetActive(false);
		}
	}
}
