using UnityEngine;
using UnityEngine.UI;

namespace UGUI_Selected
{
	public class SelectAddOutline : SelectBase
	{
		[SerializeField]
		private Color color = Color.gray;

		[SerializeField]
		private Vector2 distance = new Vector2(1f, -1f);

		private Outline _outline;

		public override void Select(bool isSelected)
		{
			if (isSelected)
			{
				if (_outline == null)
				{
					_outline = gameObject.GetComponent<Outline>();
					if (_outline == null)
					{
						_outline = gameObject.AddComponent<Outline>();
					}
				}
				_outline.enabled = true;
				_outline.effectColor = color;
				_outline.effectDistance = distance;
			}
			else if (_outline != null)
			{
				_outline.enabled = false;
			}
		}
	}
}
