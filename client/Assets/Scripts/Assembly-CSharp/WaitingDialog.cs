using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WaitingDialog : DialogBase
{
	[SerializeField]
	private Text valueTex;

	[SerializeField]
	private GameObject valueObj;

	public Action OnIAPLoadingSuc;

	private bool isCanShow;

	private Tween checkTw;

	private Sequence showSeq;

	private bool isPause;

	public WaitingDialog()
	{
		this.fMaskAlpha = 0.8f;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (this.checkTw != null)
		{
			this.checkTw.Kill(false);
			this.checkTw = null;
		}
		if (this.showSeq != null)
		{
			this.showSeq.Kill(false);
			this.showSeq = null;
		}
	}

	private void _clearAinm()
	{
		if (this.checkTw != null)
		{
			this.checkTw.Kill(false);
			this.checkTw = null;
		}
		if (this.showSeq != null)
		{
			this.showSeq.Kill(false);
			this.showSeq = null;
		}
	}

	private void Update()
	{
		this.tfRoot.Rotate(Vector3.back * 100f * Time.deltaTime);
	}

	public void ShowIAPLoading(float fDestroyTime, float fShowTime, Action callback, bool b_destroy)
	{
		this.Show(fDestroyTime, fShowTime, callback, b_destroy);
		if (this.valueObj != null)
		{
			this.valueObj.SetActive(true);
			if (this.valueTex != null)
			{
				this.valueTex.text = "LOADING......";
				int num = (int)(fDestroyTime + fShowTime);
				this.checkTw = TweenMgr.NumberTo((float)num, num, 0, delegate
				{
					if (IAPManager.instance != null && IAPManager.instance.isInitialized)
					{
						this._destroy();
						if (this.OnIAPLoadingSuc != null)
						{
							this.OnIAPLoadingSuc();
						}
					}
				});
			}
		}
	}

	public void Show(float fDestroyTime, float fShowTime, Action callback, bool b_destroy)
	{
		this.isCanShow = false;
		if (this.valueObj != null)
		{
			this.valueObj.SetActive(false);
		}
		if (this.imgMask != null)
		{
			this.imgMask.SetAlpha(0f);
		}
		if (this.tfRoot != null && this.tfRoot.gameObject != null)
		{
			this.tfRoot.gameObject.SetActive(false);
		}
		base.gameObject.SetActive(true);
		this._clearAinm();
		this.showSeq = DOTween.Sequence().SetId("WaitingDialog_Show");
		if (fShowTime >= 0f)
		{
			if (fShowTime > 0f)
			{
				this.showSeq.AppendInterval(fShowTime);
			}
			this.showSeq.AppendCallback(delegate
			{
				this.isCanShow = true;
				if (this.tfRoot != null && this.tfRoot.gameObject != null)
				{
					this.tfRoot.gameObject.SetActive(true);
					this.ShowMask();
				}
			});
		}
		if (fDestroyTime > 0f)
		{
			this.showSeq.AppendInterval(fDestroyTime);
			this.showSeq.AppendCallback(delegate
			{
				if (!b_destroy)
				{
					base.gameObject.SetActive(false);
				}
				else
				{
					this._destroy();
				}
				if (callback != null)
				{
					callback();
				}
			});
		}
	}

	public void Progress(float va)
	{
		if (!this.isCanShow)
		{
			return;
		}
		if (this.valueObj != null)
		{
			if (!this.valueObj.activeSelf)
			{
				this.valueObj.SetActive(true);
			}
			if (this.valueTex != null)
			{
				int num = (int)(va * 100f);
				this.valueTex.text = string.Format("{0}%", num);
			}
		}
	}

	public void _destroy()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
