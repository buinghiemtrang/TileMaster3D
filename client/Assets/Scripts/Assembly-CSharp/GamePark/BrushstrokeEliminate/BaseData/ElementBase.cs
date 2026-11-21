using UnityEngine;

namespace GamePark.BrushstrokeEliminate.BaseData
{
	public class ElementBase : MonoBehaviour
	{
		[SerializeField]
		private Transform tfModelNode;

		[SerializeField]
		private SpriteRenderer _tfFbx;

		[SerializeField]
		private SpriteRenderer _outline;

		public int nModelId;

		public Vector2Int loc;

		public bool IsSelect => _outline.gameObject.activeSelf;

		public void Init(int modelId, float size)
		{
			nModelId = modelId;
			CreateFbx(modelId);
			float num = (size - 10f) / 100f;
			if (modelId >= 93407 && modelId <= 93607)
			{
				num += 0.3f;
			}
			base.transform.SetScale3(num);
		}

		private void CreateFbx(int modelId)
		{
			Sprite sprite = LoadMgr.LoadSprite(string.Format("Activity/Textures/{0}", modelId));
			_outline.sprite = sprite;
			_tfFbx.sprite = sprite;
		}

		public void Clear()
		{
			_tfFbx.sprite = null;
			_outline.sprite = null;
			_outline.gameObject.SetActive(false);
		}

		public void UpdateSiblingIndex(int row)
		{
			base.transform.SetSiblingIndex(loc.x + loc.y * row);
		}

		public void SetSelect(bool select)
		{
			_outline.gameObject.SetActive(select);
		}
	}
}
