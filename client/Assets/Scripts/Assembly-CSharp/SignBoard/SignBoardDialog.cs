using System;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace SignBoard
{
	public class SignBoardDialog : DialogBase
	{
		[Serializable]
		private class RewardItem
		{
			public GameObject obj;

			public Image iconImg;

			public Text valueTex;
		}

		[SerializeField]
		private SkeletonGraphic chestAnim;

		[SerializeField]
		private GameObject openBtnObj;

		[SerializeField]
		private GameObject receiveBtnObj;

		[SerializeField]
		private GameObject titleObj;

		[SerializeField]
		private GameObject describeObj;

		[SerializeField]
		private Transform decorativeTran;

		[SerializeField]
		private Transform itemsTran;

		[SerializeField]
		private HorizontalLayoutGroup itemsHorizontalLayout;

		[SerializeField]
		private RewardItem[] items;

		[SerializeField]
		private MButton openBtn;

		[SerializeField]
		private MButton receiveBtn;

		public Action CloseCallback;

		private cfgSignBoardReward _cfg;

		private Sequence _seq;

		protected override void Awake()
		{
			base.Awake();
			openBtn.onClick.AddListener(_ClickOpen);
			receiveBtn.onClick.AddListener(_ClickReceive);
		}

		public void Show()
		{
			openBtnObj.SetActive(true);
			titleObj.SetActive(true);
			receiveBtnObj.SetActive(false);
			describeObj.SetActive(false);
			itemsTran.SetLocPosY(-75f);
			itemsTran.SetScale(0f);
			itemsHorizontalLayout.spacing = -135f;
			decorativeTran.SetLocPosY(13f);
			decorativeTran.SetScale(1f);
			chestAnim.AnimationState.SetAnimation(0, "close_idle", true);
			_cfg = SignBoardMgr.I.GetRewardCfg();
			int length = _cfg.reward.GetLength(0);
			for (int i = 0; i < items.Length; i++)
			{
				RewardItem rewardItem = items[i];
				if (i < length)
				{
					rewardItem.obj.SetActive(true);
					int id = _cfg.reward[i, 0];
					int num = _cfg.reward[i, 1];
					if (id == 23)
					{
						rewardItem.iconImg.sprite = Resources.Load<Sprite>(string.Format("Props/medal{0}", num));
						rewardItem.valueTex.text = "x1";
					}
					else
					{
						rewardItem.iconImg.sprite = Resources.Load<Sprite>(string.Format("Props/prop{0}", id));
						if (id == 21)
						{
							int hours = num / 60;
							int minutes = num % 60;
							if (num < 60)
							{
								rewardItem.valueTex.text = string.Format("{0}m", minutes);
							}
							else if (minutes == 0)
							{
								rewardItem.valueTex.text = string.Format("{0}h", hours);
							}
							else
							{
								rewardItem.valueTex.text = string.Format("{0}h{1}m", hours, minutes);
							}
						}
						else
						{
							rewardItem.valueTex.text = string.Format("x{0}", num);
						}
					}
				}
				else
				{
					rewardItem.obj.SetActive(false);
				}
			}
			_Open();
			LogEvent.I.Track("Autumn_Gifts_Open").CommonAttr(true).Send(false);
		}

		private void _ClickOpen(MButton btn)
		{
			openBtnObj.SetActive(false);
			titleObj.SetActive(false);
			_seq = DOTween.Sequence();
			_seq.Append(decorativeTran.DOLocalMove(Vector3.up * -59f, 0.2f));
			_seq.Join(decorativeTran.DOScale(Vector3.one * 0.9f, 0.2f));
			_seq.AppendCallback(delegate
			{
				chestAnim.AnimationState.SetAnimation(0, "open", false);
				chestAnim.AnimationState.AddAnimation(0, "open_idle", true, 0f);
			});
			_seq.AppendInterval(0.2f);
			_seq.Append(itemsTran.DOLocalMove(Vector3.up * 261f, 0.2f));
			_seq.Join(itemsTran.DOScale(Vector3.one, 0.2f));
			_seq.Join(TweenMgr.NumberTo(0.2f, -135f, 15f));
			_seq.AppendCallback(delegate
			{
				receiveBtnObj.SetActive(true);
				describeObj.SetActive(true);
			});
			_seq.SetEase(Ease.Linear);
		}

		private void _ClickReceive(MButton btn)
		{
			SignBoardMgr.I.ResetData();
			string text = string.Empty;
			int length = _cfg.reward.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				int propId = _cfg.reward[i, 0];
				int count = _cfg.reward[i, 1];
				if (propId == 21)
				{
					DB.Bag.AddEnergyUnlimitedTime((float)(count * 60));
				}
				else
				{
					DB.Bag.Modify((PropID)propId, count, true, "AutumnGifts");
				}
				text += string.Format("{0}|{1}", propId, count);
				if (i < length - 1)
				{
					text += ",";
				}
			}
			_Close(CloseCallback);
			LogEvent.I.Track("Autumn_Gifts_Reward").Add("Reward", text).CommonAttr(true).Send(false);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (_seq != null)
			{
				_seq.Kill();
			}
		}
	}
}
