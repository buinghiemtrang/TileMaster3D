using UnityEngine;

public class QuestionBlockItem : MonoBehaviour
{
	[SerializeField]
	private GameObject _particleSystem;

	private void OnEnable()
	{
		GTimer.ins.addDelay(UpdateShowInfo, 1f);
	}

	private void UpdateShowInfo()
	{
		_particleSystem.SetActive(value: true);
	}

	private void OnDestroy()
	{
		GTimer.ins.removeDelay(UpdateShowInfo);
	}
}
