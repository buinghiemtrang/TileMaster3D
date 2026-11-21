using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Game._Functions.RoadToRiches.Scripts
{
	public class RoadToRichesDialog : MonoBehaviour
	{
		[SerializeField]
		private MButton _btnStart;

		[SerializeField]
		private Text _ttTime;

		[SerializeField]
		private Image _imgBar;

		[SerializeField]
		private Transform[] _ayPos;

		private void Awake()
		{
			if (_btnStart != null)
			{
				_btnStart.onClick.AddListener(ClickGoGame);
			}
			RoadToRichesMgr.I.Initialized();
			Initialized();
		}

		private void OnDestroy()
		{
			_imgBar.DOKill(false);
			DOTween.Kill("DoTween_Road_To_Riches_Image_DORade");
			DOTween.Kill("DoTween_Road_To_Riches_Delay_Victory_NextFunction");
			if (_ttTime != null)
			{
				DOTween.Kill(_ttTime);
			}
		}

		public void Initialized()
		{
			if (_ttTime != null)
			{
				DateTime endTime = RoadToRichesMgr.I.EndTime;
				int seconds = (int)(endTime - DateTime.Now).TotalSeconds;
				_ttTime.DOCounter(seconds, 0, seconds, Util.Helper.FormatTimerD).OnComplete(() =>
				{
					RoadToRichesMgr.I.Initialized();
					Initialized();
				});
			}
			int count = DB.Other.RoadToRichesCount;
			int index = count % 3;
			int startLv = (count - index) + 1;
			_ayPos.ForEach((i, tf) =>
			{
				tf.FindC<Text>("Text").text = (startLv + i).ToString();
				tf.FindC<Image>("Image").gameObject.SetActive(i < index);
			});
			if (index == 0)
			{
				_imgBar.fillAmount = 0f;
			}
			else
			{
				_imgBar.fillAmount = _ayPos[index - 1].RT().anchoredPosition.x / _imgBar.transform.Rect().width;
			}
		}

		public void LevelUp()
		{
			int count = DB.Other.RoadToRichesCount;
			int index = count % 3;
			int startLv = (count - index) + 1;
			DB.Other.RoadToRichesCount++;
			if (index == 0)
			{
				_ayPos.ForEach((i, tf) =>
				{
					tf.FindC<Text>("Text").text = (startLv + i).ToString();
					tf.FindC<Image>("Image").gameObject.SetActive(false);
				});
			}
			float start = _imgBar.fillAmount;
			float end = _ayPos[index].RT().anchoredPosition.x / _imgBar.transform.Rect().width;
			_imgBar.DOCounter(start, end, 1f, v => _imgBar.fillAmount = v).OnComplete(() =>
			{
				if (index == 2)
				{
					RoadToRichesMgr.I.ClaimReward(() =>
					{
						if (VictoryControl.instance != null)
						{
							VictoryControl.instance.NextFunction();
						}
					});
				}
				else
				{
					TweenMgr.DelayCall(0.3f, () =>
					{
						if (VictoryControl.instance != null)
						{
							VictoryControl.instance.NextFunction();
						}
					}).SetId("DoTween_Road_To_Riches_Delay_Victory_NextFunction");
				}
				Image image = _ayPos[index].FindC<Image>("Image");
				if (image != null)
				{
					image.gameObject.SetActive(true);
					image.SetAlpha(0f);
					image.DOFade(1f, 0.3f).SetId("DoTween_Road_To_Riches_Image_DORade");
				}
			});
		}

		private void ClickGoGame(MButton button)
		{
			BlackCutDialog.EnterGame();
		}

		public static RoadToRichesDialog Create()
		{
			return SceneMgr.AddTopNode<RoadToRichesDialog>("Prefabs/Activity/RoadToRichesDialog");
		}
	}
}
