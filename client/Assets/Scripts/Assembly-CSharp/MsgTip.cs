using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MsgTip : MonoBehaviour
{
	[SerializeField]
	private Text _text;

	[SerializeField]
	private CanvasGroup _group;

	private Sequence seq;

	public void Show(string content)
	{
		_text.text = content;
		seq = DOTween.Sequence();
		seq.AppendInterval(2.5f).Append(_group.DOFade(0f, 0.2f)).AppendCallback(() => Destroy(gameObject));
	}

	private void OnDestroy()
	{
		if (seq != null)
		{
			seq.Kill(false);
			seq = null;
		}
	}
}
