using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples.Helpers.SelectorUI
{
	public sealed class ExampleListItem : MonoBehaviour
	{
		[SerializeField]
		private Text _text;

		public SampleSelectorUI ParentUI { get; private set; }

		public SampleBase ExamplePrefab { get; private set; }

		public void Setup(SampleSelectorUI parentUI, SampleBase prefab)
		{
		}

		public void OnButton()
		{
		}
	}
}
