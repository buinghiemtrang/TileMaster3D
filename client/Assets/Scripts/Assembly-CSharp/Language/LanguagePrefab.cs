using UnityEngine;

namespace Language
{
	[AddComponentMenu("Language/LanguagePrefab")]
	public class LanguagePrefab : LanguageBase
	{
		[Header("需要替换的预制体，必须是该脚本所在Gameobject的子节点。注意：该脚本所在Gameobjec只能有一个子节点")]
		public Transform targetTransform;

		protected override void OnSet()
		{
			GameObject prefab = LanguageManager.Instance.GetPrefab(key);
			if ((bool)prefab)
			{
				Transform transform = Object.Instantiate(prefab.transform);
				transform.SetParent(base.transform, worldPositionStays: false);
				Object.DestroyImmediate(targetTransform.gameObject);
				targetTransform = transform;
			}
		}
	}
}
