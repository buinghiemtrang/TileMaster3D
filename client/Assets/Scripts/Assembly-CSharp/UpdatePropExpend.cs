using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UpdatePropExpend : UpdateProp
{
	public GameObject objAd;

	public GameObject objUnlock;

	public GameObject objLock;

	public GameObject objFree;

	public Transform icon;

	public Transform iconNode;

	public Text lvTex;

	public Image imgMask;

	protected MButton pBtn;

	[HideInInspector]
	public bool isLock;

	private Tween tw;

	private Sequence tipSeq;

	protected override void Awake()
	{
		base.Awake();
		pBtn = GetComponent<MButton>();
		DispatcherMgr.On<bool, string>("GamePause", SetGamePause);
	}

	private void SetGamePause(bool isPause, string param)
	{
		if (tw != null)
		{
			if (isPause)
			{
				tw.Pause();
			}
			else
			{
				tw.Play();
			}
		}
	}

	protected override void Start()
	{
	}

	public void Init(bool b_lock, int lv)
	{
		isLock = b_lock;
		DB.Bag.UseNum[kProp.ToString()] = 0;
		lvTex.SetLBy(lv);
		if (imgMask != null)
		{
			imgMask.gameObject.SetActive(value: false);
		}
		pBtn.interactable = true;
		if (tw != null)
		{
			tw.Kill();
			tw = null;
		}
		switch (kProp)
		{
		case PropID.Hint:
			isFree = DB.User.Curlv + 1 == cf_const.unlockHintLv;
			break;
		case PropID.Freeze:
			isFree = DB.User.Curlv + 1 == cf_const.unlockFreezeLv;
			break;
		case PropID.Back:
			isFree = DB.User.Curlv + 1 == cf_const.unlockBackLv;
			break;
		case PropID.Refresh:
			isFree = DB.User.Curlv + 1 == cf_const.unlockRefreshLv;
			break;
		default:
			isFree = false;
			break;
		}
		Refresh();
	}

	private void SetActive(GameObject obj, bool b_active)
	{
		if (!(obj == null))
		{
			obj.SetActive(b_active);
		}
	}

	protected override void UpdateBtnBuy(int count)
	{
		SetActive(objAd, b_active: false);
		SetActive(objUnlock, !isLock);
		SetActive(objLock, isLock);
		objFree.SetActive(isFree);
		SetActive(objBtnBuy, count == 0 && !isFree);
	}

	public void SetCoolDown(float time)
	{
		if (imgMask == null)
		{
			return;
		}
		pBtn.interactable = false;
		imgMask.gameObject.SetActive(value: true);
		if (tw != null)
		{
			tw.Kill();
		}
		tw = TweenMgr.NumberTo(time, 1f, 0f, delegate(float value)
		{
			imgMask.fillAmount = value;
		}).SetEase(Ease.Linear).OnComplete(delegate
		{
			pBtn.interactable = true;
			imgMask.gameObject.SetActive(value: false);
		});
	}

	public void SetState(bool isLock)
	{
		if (pBtn != null)
		{
			if (pBtn.interactable != isLock)
			{
				return;
			}
			pBtn.interactable = !isLock;
			if (imgMask != null)
			{
				imgMask.gameObject.SetActive(isLock);
			}
		}
	}

	public void Tip(bool isOpen)
	{
		if (tipSeq != null)
		{
			tipSeq.Kill();
			tipSeq = null;
		}
		if (isOpen)
		{
			tipSeq = DOTween.Sequence();
			tipSeq.Append(icon.DOBlendableLocalMoveBy(Vector3.up * 20f, 0.4f));
			tipSeq.AppendInterval(0.1f);
			tipSeq.Append(icon.DOBlendableLocalMoveBy(Vector3.down * 20f, 0.4f));
			tipSeq.SetLoops(-1);
		}
		else
		{
			icon.SetLocPos(0f, 0f);
		}
	}

	private bool IsWatchAdv()
	{
		return Singleton<AdManager>.I.IsRewardVideoOk(isCheckLoadState: false);
	}

	private void OnDestroy()
	{
		if (tw != null)
		{
			tw.Kill();
			tw = null;
		}
		if (tipSeq != null)
		{
			tipSeq.Kill();
			tipSeq = null;
		}
		DispatcherMgr.Off<bool, string>("GamePause", SetGamePause);
	}
}
