using UnityEngine;
using UnityEngine.UI;

namespace Festival
{
	[RequireComponent(typeof(Text))]
	public class ReplaceOutlineAndShadowColor : OperationBase
	{
		private Text text;

		[SerializeField]
		private Outline outline;

		[SerializeField]
		private Shadow shadow;

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void Refresh()
		{
			if (PointInfo == null)
			{
				return;
			}
			string[,] array = PointInfo.information.ToStr(2, ',', '|');
			if (array == null)
			{
				return;
			}
			int length = array.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				string a = array[i, 0];
				if (a.Equals("0"))
				{
					text = GetComponent<Text>();
					if (text != null)
					{
						text.color = Util.Helper.ColorFromHex(array[i, 1]);
					}
				}
				else if (a.Equals("1"))
				{
					if (outline != null)
					{
						outline.effectColor = Util.Helper.ColorFromHex(array[i, 1]);
					}
				}
				else if (a.Equals("2"))
				{
					if (shadow != null)
					{
						shadow.effectColor = Util.Helper.ColorFromHex(array[i, 1]);
					}
				}
			}
		}
	}
}
