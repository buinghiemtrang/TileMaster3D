using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UpdateScoreControl : MonoBehaviour
{
	public Text textui;

	public Transform tfIco;

	public Transform tfEndPos;

	public Transform tfParent;

	public int nScore;

	public void UpdateText(int value)
	{
		textui.text = value.ToString();
	}

	public void UpdateText()
	{
		textui.text = nScore.ToString();
	}

	public void SetScore(int value)
	{
		nScore = value;
		textui.text = value.ToString();
	}

	public int ModifyScore(int value)
	{
		int result = nScore;
		nScore += value;
		return result;
	}

	public void ModifyScoreShow(int value, Vector3 startPos)
	{
		for (int i = 0; i < value; i++)
		{
			Transform tf = Object.Instantiate(tfIco, tfParent);
			tf.gameObject.SetActive(value: true);
			Vector3 vector = tfParent.InverseTransformPoint(startPos);
			Vector3 vector2 = tfParent.InverseTransformPoint(tfEndPos.position);
			tf.localPosition = vector;
			tf.SetScale(0f);
			tf.DOScale(1f, 0.2f);
			Vector3 p2 = Utils.Vec3(vector.x + (vector2.x - vector.x) * 0.5f, vector2.y, 0f);
			tf.DOLocalBezier(0.8f, vector, p2, vector2).SetDelay((float)(i + 1) * 0.1f).OnComplete(delegate
			{
				UpdateText(nScore);
				Object.Destroy(tf.gameObject);
				tfEndPos.localScale = TweenMgr.SetVec3(1.1f, 1.1f, 1f);
				Sequence sequence = DOTween.Sequence();
				sequence.Append(tfEndPos.DOScale(TweenMgr.SetVec3(1.2f, 1.2f, 1f), 0.05f));
				sequence.AppendInterval(0.1f);
				sequence.Append(tfEndPos.DOScale(TweenMgr.SetVec3(1f, 1f, 1f), 0.2f));
			});
		}
	}
}
