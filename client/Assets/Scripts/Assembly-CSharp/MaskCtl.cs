using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MaskCtl : MonoBehaviour
{
	[SerializeField]
	private Image bgImg;

	[SerializeField]
	private GameObject iconObj;

	private Sequence showSeq;

	private GameObject selfObj;

	protected virtual void Awake()
	{
		selfObj = base.gameObject;
		selfObj.SetActive(value: false);
	}

	public void Show(float fShowTime = 1f, bool isNeedMask = true)
	{
		Hide(isHide: false);
		iconObj.SetActive(value: false);
		SetMaskRaycast(isNeedMask);
		_Clear();
		showSeq = DOTween.Sequence().SetId("MaskCtl_Show");
		if (!(fShowTime < 0f))
		{
			if (fShowTime > 0f)
			{
				showSeq.AppendInterval(fShowTime);
			}
			showSeq.AppendCallback(delegate
			{
				iconObj.SetActive(value: true);
			});
		}
	}

	public void Hide(bool isHide = true)
	{
		_Clear();
		if (selfObj.activeSelf == isHide)
		{
			selfObj.SetActive(!isHide);
		}
	}

	private void _Clear()
	{
		if (showSeq != null)
		{
			showSeq.Kill();
			showSeq = null;
		}
	}

	protected virtual void OnDestroy()
	{
		_Clear();
	}

	public void SetMaskRaycast(bool isNeedRaycast = false)
	{
		if (bgImg != null)
		{
			bgImg.raycastTarget = isNeedRaycast;
		}
	}
}
