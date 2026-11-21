using UnityEngine;
using UnityEngine.UI;

namespace GameScene_Guide
{
	public class ElementGuideView : MonoBehaviour
	{
		[SerializeField]
		private Text descTex;

		[SerializeField]
		private Transform rootNode;

		public void Init()
		{
			rootNode.SetActive(false);
		}

		public void Show(cfgModel config)
		{
			if (config.type == 1)
			{
				return;
			}
			rootNode.SetActive(true);
			string key;
			switch (config.type)
			{
			case 2:
				key = "Language438";
				break;
			case 3:
				key = "Language435";
				break;
			case 4:
				key = "Language436";
				break;
			case 5:
				key = "Language434";
				break;
			case 6:
				key = "Language440";
				break;
			case 7:
			case 8:
				key = "Language51";
				break;
			case 9:
				key = "Language443";
				break;
			default:
				return;
			}
			descTex.SetLKey(key);
		}

		public void Hide()
		{
			rootNode.SetActive(false);
		}
	}
}
