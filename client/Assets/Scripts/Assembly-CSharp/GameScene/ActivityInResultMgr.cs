using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GameScene
{
	public class ActivityInResultMgr : MonoBehaviour
	{
		[SerializeField]
		private ActivityInResult[] _allActivityInResults;

		[SerializeField]
		private GameObject _mulRootObj;

		[SerializeField]
		private GameObject _singelRootObj;

		[SerializeField]
		private Image _imageSingleIcon;

		[SerializeField]
		private Text _textSingleScore;

		[HideInInspector]
		public bool bIsShow;

		public bool bIsVictory = true;

		public static ActivityInResultMgr I;

		private bool bInitialized;

		private void Awake()
		{
			I = this;
		}

		private void OnDestroy()
		{
			I = null;
		}

		private void Start()
		{
			Initialized();
		}

		public void Initialized()
		{
			if (bInitialized)
			{
				return;
			}
			bInitialized = true;
			List<ActivityInResult> list = new List<ActivityInResult>();
			for (int i = 0; i < _allActivityInResults.Length; i++)
			{
				ActivityInResult activityInResult = _allActivityInResults[i];
				activityInResult.Init();
				if (activityInResult.IsOpen())
				{
					list.Add(activityInResult);
				}
			}
			int count = list.Count;
			bIsShow = count > 0;
			if (_mulRootObj != null)
			{
				_mulRootObj.SetActive(bIsShow && count != 1);
			}
			if (_singelRootObj != null)
			{
				_singelRootObj.SetActive(count == 1);
			}
			if (count != 1)
			{
				return;
			}
			ActivityInResult activityInResult2 = list.First();
			Sprite iconSprite = activityInResult2.GetIconSprite();
			int score = activityInResult2.GetScore();
			if (_imageSingleIcon != null && iconSprite != null)
			{
				_imageSingleIcon.sprite = iconSprite;
			}
			if (_textSingleScore == null)
			{
				return;
			}
			bool flag = bIsVictory;
			_textSingleScore.gameObject.SetActive(score != 0 && flag);
			if (score != 0 && flag)
			{
				_textSingleScore.text = string.Format("x{0}", score);
			}
		}

		private void ShowMoveAnim()
		{
			Transform parent = base.transform.parent;
			Vector3 localPosition = parent.localPosition;
			localPosition.x += -532f;
			parent.DOLocalMove(localPosition, 0.5f);
		}
	}
}
