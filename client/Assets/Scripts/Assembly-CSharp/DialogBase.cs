using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DialogBase : MonoBehaviour
{
	protected RawImage imgMask;

	public Transform tfRoot;

	protected Vector3 vecOrign;

	[HideInInspector]
	public CanvasGroup canvasGroup;

	private Tween twMaskShow;

	private Tween twMaskHide;

	private Tween twOpen;

	private Tween twClose;

	[Range(0f, 1f)]
	public float fMaskAlpha = 0.8f;

	protected virtual void Awake()
	{
		CreateMaskLayer();
		canvasGroup = base.transform.GetComponent<CanvasGroup>();
		if (!canvasGroup)
		{
			canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
		}
	}

	protected void CreateMaskLayer()
	{
		RawImage component = base.transform.GetComponent<RawImage>();
		if (component == null)
		{
			component = base.gameObject.AddComponent<RawImage>();
			component.color = new Color(0f, 0f, 0f, 1f);
		}
		SetMaskLayer(component);
	}

	private void _CreateCanvasGroup()
	{
		canvasGroup = base.transform.GetComponent<CanvasGroup>();
		if (!canvasGroup)
		{
			canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
		}
	}

	protected void SetMaskLayer(RawImage image)
	{
		imgMask = image;
		if (imgMask != null)
		{
			imgMask.SetAlpha(fMaskAlpha);
		}
		if (!(tfRoot == null))
		{
			vecOrign = tfRoot.transform.localPosition;
		}
	}

	public void ShowMask()
	{
		if (!(imgMask == null))
		{
			imgMask.SetAlpha(0f);
			if (twMaskShow != null)
			{
				twMaskShow.Kill();
				twMaskShow = null;
			}
			twMaskShow = imgMask.DOFade(fMaskAlpha, 0.3f).SetId("Dialog_Base_Show");
		}
	}

	public void HideMask()
	{
		if (!(imgMask == null))
		{
			if (twMaskHide != null)
			{
				twMaskHide.Kill();
				twMaskHide = null;
			}
			twMaskHide = imgMask.DOFade(0f, 0.3f).SetId("Dialog_Base_Hide");
		}
	}

	public void CloseOperation()
	{
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false; // UNVERIFIED: icall 0x8bc521 assumed to be set_blocksRaycasts
	}

	public void OpenOperation()
	{
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true; // UNVERIFIED: icall 0x8bc521 assumed to be set_blocksRaycasts
	}

	public void _Open(TweenCallback callback = null)
	{
		ShowMask();
		if (tfRoot == null)
		{
			callback?.Invoke();
			return;
		}
		if (twOpen != null)
		{
			twOpen.Kill();
			twOpen = null;
		}
		twOpen = TweenMgr.DialogIn(tfRoot, vecOrign).OnComplete(callback).SetId("Dialog_Base_Open");
	}

	public void _Close(Action callback = null)
	{
		HideMask();
		TweenCallback tweenCallback = delegate
		{
			if ((bool)this && (bool)base.gameObject)
			{
				SceneMgr.RemoveDialog(base.gameObject);
				this.DestroySelf();
				callback?.Invoke();
			}
		};
		if (tfRoot == null)
		{
			tweenCallback();
			return;
		}
		CloseOperation();
		if (twClose != null)
		{
			twClose.Kill();
			twClose = null;
		}
		twClose = TweenMgr.DialogOut(tfRoot, vecOrign).OnComplete(tweenCallback).SetId("Dialog_Base_Close");
	}

	protected virtual void OnDestroy()
	{
		if (twMaskShow != null)
		{
			twMaskShow.Kill();
			twMaskShow = null;
		}
		if (twMaskHide != null)
		{
			twMaskHide.Kill();
			twMaskHide = null;
		}
		if (twOpen != null)
		{
			twOpen.Kill();
			twOpen = null;
		}
		if (twClose != null)
		{
			twClose.Kill();
			twClose = null;
		}
	}
}
