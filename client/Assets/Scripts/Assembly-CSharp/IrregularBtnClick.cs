using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class IrregularBtnClick : MonoBehaviour
{
	[Header("透明度低于这个值点击无效")]
	public float alpha;

	private void Awake()
	{
		GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
	}
}
