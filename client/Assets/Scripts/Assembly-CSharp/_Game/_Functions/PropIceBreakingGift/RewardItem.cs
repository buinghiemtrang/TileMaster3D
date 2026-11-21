using UnityEngine;
using UnityEngine.UI;

namespace _Game._Functions.PropIceBreakingGift
{
	public class RewardItem : MonoBehaviour
	{
		[SerializeField]
		private GameObject _root;

		[SerializeField]
		private Text _valueTex;

		[SerializeField]
		private Image _iconImg;

		internal GameObject Root => _root;

		public void Init(PropID type, int value)
		{
			Global.GetContents(type, value, out var sprite, out var text);
			_valueTex.text = text;
			_iconImg.sprite = sprite;
		}
	}
}
