using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace TreasureHunt
{
	public class TreasureHuntActivityVictoryDialogTip : MonoBehaviour
	{
		[SerializeField]
		private SkeletonGraphic chestAnim;

		[SerializeField]
		private Text timeTex;

		[SerializeField]
		private GameObject finishedWordObj;

		[SerializeField]
		private Text scoreTex;

		private Sequence scoreSeq;

		private Tweener endTimeTw;

		private void Awake()
		{
			base.gameObject.SetActive(false);
		}

		public void Init(int modelNum)
		{
			bool flag = Singleton<TreasureHuntActivityMgr>.I.JudgeOpen();
			base.gameObject.SetActive(flag);
			if (flag)
			{
				_ShowEndTime();
			}
			int num = Singleton<TreasureHuntActivityMgr>.I.SetScore(modelNum);
			if (num >= 1)
			{
				scoreTex.text = string.Format("+{0}", num);
				scoreSeq = DOTween.Sequence();
				scoreSeq.Append(scoreTex.DOFade(1f, 1.5f));
				scoreSeq.Join(scoreTex.transform.DOLocalMoveY(300f, 1.5f));
				scoreSeq.SetEase(Ease.Linear);
				scoreSeq.SetId("TreasureHuntActivityVictoryDialogTip_Score");
				scoreSeq.OnComplete(delegate
				{
					scoreTex.gameObject.SetActive(false);
				});
				chestAnim.AnimationState.SetAnimation(0, "fall", false);
				chestAnim.AnimationState.AddAnimation(0, "icon", true, 0f);
			}
		}

		private void _ShowEndTime()
		{
			endTimeTw.OwnKill();
			int endTime = Singleton<TreasureHuntActivityMgr>.I.GetEndTime();
			_ActivityEnd(endTime == 0);
			endTimeTw = TweenMgr.NumberTo((float)endTime, endTime, 0, delegate(int va)
			{
				timeTex.text = Util.Helper.FormatTimer(va, "hh:mm:ss", false);
			}).SetEase(Ease.Linear).SetId("TreasureHuntActivityVictoryDialogTip_EndTime").OnComplete(delegate
			{
				if (!finishedWordObj.activeSelf)
				{
					finishedWordObj.SetActive(true);
				}
				if (timeTex.gameObject.activeSelf)
				{
					timeTex.gameObject.SetActive(false);
				}
			});
		}

		private void _ActivityEnd(bool isEnd)
		{
			if (finishedWordObj.activeSelf != isEnd)
			{
				finishedWordObj.SetActive(isEnd);
			}
			if (timeTex.gameObject.activeSelf == isEnd)
			{
				timeTex.gameObject.SetActive(!isEnd);
			}
		}

		private void OnDestroy()
		{
			endTimeTw.OwnKill();
			if (scoreSeq != null)
			{
				scoreSeq.Kill();
				scoreSeq = null;
			}
		}
	}
}
