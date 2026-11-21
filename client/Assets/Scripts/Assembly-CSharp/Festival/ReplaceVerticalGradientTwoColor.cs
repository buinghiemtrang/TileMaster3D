using UnityEngine;

namespace Festival
{
	[RequireComponent(typeof(TextVerticalGradientTwoColor))]
	public class ReplaceVerticalGradientTwoColor : OperationBase
	{
		[SerializeField]
		private TextVerticalGradientTwoColor twoColor;

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void Refresh()
		{
			if (PointInfo != null)
			{
				string[] array = PointInfo.information.ToStr('|');
				if (array.Length == 2)
				{
					twoColor.colorTop = Util.Helper.ColorFromHex(array[0]);
					twoColor.colorBottom = Util.Helper.ColorFromHex(array[1]);
				}
			}
		}
	}
}
