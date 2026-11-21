using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MedalFinishedTips : MonoBehaviour
{
	public Image ImageIcon;

	public Image ImageEffect;

	public Text TextDesc;

	public Transform tfRoot;

	public Action completeCallback;

	private Tween twAnim;

	public void InitInfo(cfg_medal cfg)
	{
		ImageIcon.sprite = LoadMgr.LoadRes<Sprite>("MedalIcon/" + cfg.Icon);
		ImageIcon.SetNativeSize();
		ShowAnim();
	}

	private void ShowAnim()
	{
		Material effectMat = ImageEffect.material;
		Vector2 offset = effectMat.mainTextureOffset;
		Vector3 localPosition = tfRoot.localPosition;
		Vector3 localPosition2 = tfRoot.localPosition;
		tfRoot.localPosition = new Vector3(localPosition2.x, localPosition.y + 160f, 0f);
		tfRoot.localScale = new Vector3(0.1f, 1f, 1f);
		twAnim = DOTween.Sequence().AppendInterval(0.1f).Append(tfRoot.DOLocalMove(new Vector3(localPosition2.x, localPosition.y, 0f), 0.2f)).Append(tfRoot.DOScale(new Vector3(1f, 1f, 1f), 0.2f))
			.Append(TweenMgr.NumberTo(0.6f, 0.7f, -0.7f, delegate(float va)
			{
				offset.x = va;
				effectMat.mainTextureOffset = offset;
			}))
			.AppendInterval(1.2f)
			.Append(tfRoot.transform.DOLocalMove(new Vector3(localPosition2.x, localPosition.y + 160f, 0f), 0.3f).SetEase(Ease.InOutQuad))
			.OnComplete(delegate
			{
				SceneMgr.RemoveDialog(base.gameObject);
				this.DestroySelf();
				if (completeCallback != null)
				{
					completeCallback();
				}
			});
	}

	protected void OnDestroy()
	{
		if (twAnim != null)
		{
			twAnim.Kill(false);
			twAnim = null;
		}
	}
}
