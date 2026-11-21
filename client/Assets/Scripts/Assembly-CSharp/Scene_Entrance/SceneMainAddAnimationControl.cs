using UnityEngine;

namespace Scene_Entrance
{
	public class SceneMainAddAnimationControl : MonoBehaviour
	{
		[SerializeField]
		private GameObject _leftAddEffectPrefab;

		[SerializeField]
		private GameObject _rightAddEffectPrefab;

		[SerializeField]
		private Transform _parentNode;

		private void Start()
		{
			_leftAddEffectPrefab.gameObject.SetActive(false);
			_rightAddEffectPrefab.gameObject.SetActive(false);
		}

		public AddAnimationItem GetAddAnimation(EntranceDirection direction)
		{
			GameObject original = ((direction == EntranceDirection.Left) ? _leftAddEffectPrefab : _rightAddEffectPrefab);
			GameObject instance = Object.Instantiate(original, _parentNode);
			return instance.GetComponent<AddAnimationItem>();
		}
	}
}
