using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TreasureHunt
{
	public class TreasureHuntActivityFailureDialogTip : MonoBehaviour
	{
		[SerializeField]
		private Text timeTex;

		[SerializeField]
		private GameObject finishedWordObj;

		private Tweener endTimeTw;

		public bool Init()
		{
			bool flag = Singleton<TreasureHuntActivityMgr>.I.JudgeOpen();
			base.gameObject.SetActive(flag);
			if (flag)
			{
				_ShowEndTime();
			}
			return flag;
		}

		private void _ShowEndTime()
		{
			TweenMgr.OwnKill(endTimeTw);
			int num = Singleton<TreasureHuntActivityMgr>.I.GetEndTime();
			_ActivityEnd(num == 0);
			endTimeTw = TweenMgr.NumberTo((float)num, num, 0, delegate(int va)
			{
				timeTex.text = Util.Helper.FormatTimer(va, "hh:mm:ss");
			}).SetEase(Ease.Linear).SetId("TreasureHuntActivityFailureDialogUI_EndTime").OnComplete(delegate
			{
				_ActivityEnd(true);
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
			TweenMgr.OwnKill(endTimeTw);
		}
	}
}
