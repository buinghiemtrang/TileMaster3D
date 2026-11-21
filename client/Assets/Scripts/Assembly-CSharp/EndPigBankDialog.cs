using System;
using System.Collections;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class EndPigBankDialog : DialogBase
{
	public Text addPigGoldText;

	public GameObject TextObj;

	public GameObject FullGoldObj;

	public Image progressPigImage;

	public SkeletonGraphic spineAnim;

	[SerializeField]
	private MButton btnSkip;

	[SerializeField]
	private Text textSkipTips;

	private int addGold;

	private int nowPigGold;

	private int endPigGold;

	private bool isInit;

	private Coroutine showGoldAnimIE;

	private Tweener goldTw;

	private Tweener barTw;

	private Action _onCompleted;

	private void Start()
	{
		if (isInit)
		{
			if (showGoldAnimIE != null)
			{
				StopCoroutine(showGoldAnimIE);
			}
			showGoldAnimIE = StartCoroutine(DoAddGoldAnimation());
		}
	}

	public void Show(int addGoldCount, Action onCompleted)
	{
		_onCompleted = onCompleted;
		addGold = addGoldCount;
		nowPigGold = PigBankData.Instance.NowGold;
		endPigGold = addGold + nowPigGold;
		endPigGold = PigBankData.Instance.SetPigBankGold(endPigGold);
		if (!isInit)
		{
			isInit = true;
		}
	}

	private IEnumerator DoAddGoldAnimation()
	{
		spineAnim.gameObject.SetActive(true);
		progressPigImage.transform.parent.gameObject.SetActive(true);
		float maxGold = pig_const.PigMaxGoldReward;
		float startf = (float)nowPigGold / maxGold;
		progressPigImage.fillAmount = startf;
		addPigGoldText.text = nowPigGold.ToString();
		float targetFill = (float)endPigGold / maxGold;
		FullGoldObj.SetActive(targetFill >= 1f);
		if (btnSkip != null)
		{
			btnSkip.interactable = targetFill >= 1f;
		}
		if (textSkipTips != null)
		{
			textSkipTips.gameObject.SetActive(targetFill >= 1f);
		}
		if (nowPigGold < endPigGold)
		{
			spineAnim.AnimationState.SetAnimation(0, "into", false);
			if (goldTw != null)
			{
				goldTw.Kill(false);
			}
			goldTw = TweenMgr.NumberTo(1.2f, nowPigGold, endPigGold, var =>
			{
				addPigGoldText.text = var.ToString();
			});
			if (barTw != null)
			{
				barTw.Kill(false);
			}
			barTw = DOTween.To(() => startf, x => progressPigImage.fillAmount = x, targetFill, 1.2f);
		}
		else
		{
			var trackEntry = spineAnim.AnimationState.SetAnimation(0, "out", false);
			trackEntry.Complete += a =>
			{
				spineAnim.AnimationState.SetAnimation(0, "out", false);
			};
			TextObj.SetActive(false);
		}
		yield return new WaitForSeconds(1.8f);
		_onCompleted.InvokeSafe();
		UnityEngine.Object.Destroy(gameObject);
	}

	public void PigOut()
	{
		TextObj.SetActive(false);
		spineAnim.AnimationState.SetAnimation(0, "out", false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (goldTw != null)
		{
			goldTw.Kill(false);
			goldTw = null;
		}
		if (barTw != null)
		{
			barTw.Kill(false);
			barTw = null;
		}
	}

	public void ClickBtnSkip(MButton button)
	{
		if (showGoldAnimIE != null)
		{
			StopCoroutine(showGoldAnimIE);
		}
		DB.User.IsSkipPigAnim = true;
		_onCompleted.InvokeSafe();
		this.DestroySelf();
	}
}
