using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AchiveFinishedTips : MonoBehaviour
{
	public Image ImageIcon;

	public Text TextName;

	public Text TextDesc;

	public Transform tfRoot;

	public Action completeCallback;

	private Tween twAnim;

	public void InitInfo(cfg_achive cfg, int iLevel, int iLevelCount)
	{
		TextName.SetLKey(cfg.TitleLangId);
		int[] array = cfg.Values.ToInt('|');
		int num = (iLevel < iLevelCount) ? iLevel : (iLevel - 1);
		TextDesc.SetLKey(cfg.DescLangId, array[num]);
		ImageIcon.sprite = LoadMgr.LoadRes<Sprite>("AchiveIcon/" + cfg.Icon);
		ImageIcon.SetNativeSize();
		ShowAnim();
		LogEvent.I.Track("Archievement_SuC").Add("ArchievementType", cfg.Type).Add("ArchievementLevel", iLevel).CommonAttr(true).Send(false);
	}

	private void ShowAnim()
	{
		Vector3 localPosition = tfRoot.localPosition;
		Vector3 localPosition2 = tfRoot.localPosition;
		tfRoot.localPosition = new Vector3(localPosition2.x, localPosition.y + 160f, 0f);
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval(0.1f);
		sequence.Append(tfRoot.DOLocalMove(new Vector3(localPosition2.x, localPosition.y, 0f), 0.3f));
		sequence.AppendInterval(1.2f);
		sequence.Append(tfRoot.DOLocalMove(new Vector3(localPosition2.x, localPosition.y + 160f, 0f), 0.3f).SetEase(Ease.InOutQuad));
		twAnim = sequence.OnComplete(delegate
		{
			SceneMgr.RemoveDialog(base.gameObject);
			this.DestroySelf();
			completeCallback?.Invoke();
		});
	}

	protected void OnDestroy()
	{
		if (twAnim != null)
		{
			twAnim.Kill();
			twAnim = null;
		}
	}
}
