using UnityEngine;
using UnityEngine.UI;

namespace GamePark.BrushstrokeEliminate.EditorShape
{
	[ExecuteInEditMode]
	public class ShapeGizmosOne : MonoBehaviour
	{
		private readonly Color[] _colors;

		[SerializeField]
		public SlotType kType;

		[SerializeField]
		private int nIndex;

		[SerializeField]
		public Vector2Int vLoc;

		private Image _imageUi;

		public int Index
		{
			get
			{
				return nIndex;
			}
			set
			{
				nIndex = value;
			}
		}

		public ShapeGizmosOne()
		{
			_colors = new Color[] { Color.gray, Color.white };
			vLoc = Vector2Int.zero;
		}

		private void Awake()
		{
			_imageUi = transform.GetComponent<Image>();
		}

		public void Init(int index, int x, int y)
		{
			_imageUi = gameObject.AddComponent<Image>();
			nIndex = index + 1;
			vLoc = new Vector2Int(x, y);
			gameObject.name = string.Format("Item_{0}", nIndex);
		}

		public void SetSize(int w, int h)
		{
			(_imageUi.transform as RectTransform).sizeDelta = new Vector2(w, h);
		}
	}
}
