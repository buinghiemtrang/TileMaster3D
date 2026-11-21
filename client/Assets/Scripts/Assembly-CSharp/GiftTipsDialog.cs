using UnityEngine;
using UnityEngine.UI;

public class GiftTipsDialog : MonoBehaviour
{
	[SerializeField]
	private Image ImaggeBg;

	[SerializeField]
	private Text TextTips;

	public void Show(Transform tf)
	{
		Vector3 vector = base.transform.InverseTransformPoint(tf.position);
		ImaggeBg.transform.localPosition = new Vector3(vector.x, vector.y + -60f, vector.z);
	}

	public void OnClickClose(MButton btn)
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
