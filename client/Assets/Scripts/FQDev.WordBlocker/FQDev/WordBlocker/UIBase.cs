using UnityEngine;

namespace FQDev.WordBlocker
{
	public abstract class UIBase : MonoBehaviour
	{
		[SerializeField]
		protected string _customFilter = string.Empty;

		protected TextFilter GetFilter()
		{
			if (string.IsNullOrEmpty(_customFilter))
			{
				return TextFilterCenter.Default;
			}
			return TextFilterCenter.GetFilter(_customFilter);
		}
	}
}
