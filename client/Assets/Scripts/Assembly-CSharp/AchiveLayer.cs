using System;
using Achive;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AchiveLayer : MonoBehaviour
{
	[SerializeField]
	private Image ImageIcon;

	[SerializeField]
	private Image ImageStar;

	[SerializeField]
	private Image IamgeBar;

	[SerializeField]
	private Text TextName;

	[SerializeField]
	private Text Textbar;

	[SerializeField]
	private AchiveScrollView pScroll;

	private Sequence StarSeq;

	private void Start()
	{
		DispatcherMgr.On<Transform>("Show_Move_Star", ShowStarAnim);
		LogEvent.I.Track("Archievement_Open").CommonAttr(true).Send(false);
		AchiveMgr.I.SortAchieveCfg();
		InitAchiveInfo();
		pScroll.RefreshItems();
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off<Transform>("Show_Move_Star", ShowStarAnim);
		AchiveMgr.I.UpdateCurScoreCfg();
		if (StarSeq != null)
		{
			StarSeq.Kill();
			StarSeq = null;
		}
	}

	private void UpdateUi()
	{
		InitAchiveInfo();
		pScroll.RefreshItems();
	}

	private void InitAchiveInfo()
	{
		int curAchieveReachCount = AchiveMgr.I.GetCurAchieveReachCount();
		cfg_achiveScore achiveScoreCfgByReachCount = AchiveMgr.I.GetAchiveScoreCfgByReachCount(curAchieveReachCount);
		if (achiveScoreCfgByReachCount == null)
		{
			return;
		}
		ImageIcon.sprite = LoadMgr.LoadRes<Sprite>("AchiveIcon/" + achiveScoreCfgByReachCount.Icon);
		ImageIcon.SetNativeSize();
		Logs.Common.Error(string.Format("cfgid:{0}", achiveScoreCfgByReachCount.id));
		Logs.Common.Error(string.Format("iReachCount:{0}", curAchieveReachCount));
		if (achiveScoreCfgByReachCount.id < DataCfg.cfg_achiveScores.Count)
		{
			cfg_achiveScore achiveScoreCfg = AchiveMgr.I.GetAchiveScoreCfg(achiveScoreCfgByReachCount.id + 1);
			if (achiveScoreCfg != null)
			{
				int num = curAchieveReachCount - achiveScoreCfgByReachCount.Count;
				int num2 = achiveScoreCfg.Count - achiveScoreCfgByReachCount.Count;
				Textbar.text = string.Format("{0}/{1}", num, num2);
				IamgeBar.fillAmount = (float)num / (float)num2;
			}
		}
		else
		{
			Textbar.text = "0/0";
			IamgeBar.fillAmount = 0f;
		}
	}

	private void ShowStarAnim(Transform tf)
	{
		Vector3 vector = base.transform.InverseTransformPoint(tf.position);
		ImageStar.transform.SetScale(0f);
		ImageStar.transform.localPosition = vector;
		Vector3 endValue = base.transform.InverseTransformPoint(ImageIcon.transform.position);
		StarSeq = DOTween.Sequence();
		StarSeq.Append(ImageStar.transform.DOScale(3f, 0.2f));
		StarSeq.Append(ImageStar.transform.DOLocalMove(endValue, 0.5f));
		StarSeq.AppendInterval(0.1f);
		StarSeq.Append(ImageStar.transform.DOScale(0f, 0.2f));
		StarSeq.AppendCallback(UpdateUi);
	}
}
