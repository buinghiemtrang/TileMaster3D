using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BubbleTips : MonoBehaviour
{
	[SerializeField]
	private Text _textTips;

	[SerializeField]
	private Image _imageBg;

	[SerializeField]
	private CanvasGroup _group;

	private Sequence seq;

	public void Show(string tips, Transform tfTarget)
	{
		_textTips.text = tips;
		float num = _textTips.preferredHeight;
		_imageBg.transform.SetHeight(num + 62f);
		if (tfTarget != null)
		{
			Rect rect = tfTarget.Rect();
			Vector3 position = tfTarget.position;
			Vector3 localPosition = _imageBg.transform.parent.InverseTransformPoint(position);
			localPosition.y -= (num + 62f) * 0.5f + rect.height * 0.5f;
			localPosition.z = 0f;
			_imageBg.transform.localPosition = localPosition;
		}
		seq = DOTween.Sequence();
		seq.AppendInterval(1f);
		seq.Append(_group.DOFade(0f, 0.2f));
		seq.AppendCallback(delegate
		{
			Object.Destroy(gameObject);
		});
	}

	private void OnDestroy()
	{
		if (seq != null)
		{
			seq.Kill();
			seq = null;
		}
	}
}
