using System;
using DG.Tweening;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class RecallDialog : DialogBase
{
	[SerializeField]
	private RecallItem cellItem;

	[SerializeField]
	private Text descTex;

	[SerializeField]
	private MButton btnOpen;

	[SerializeField]
	private MButton btnRecieve;

	[SerializeField]
	private SkeletonGraphic spineAnim;

	[SerializeField]
	private Transform tfReward;

	[SerializeField]
	private Transform tfItemsRoot;

	[SerializeField]
	private Transform[] tfItems;

	private cfgRecallReward cfg;

	public Action closeCallback;

	private Tween twAnim;

	private void Start()
	{
		if (tfReward != null)
		{
			tfReward.gameObject.SetActive(false);
		}
		if (btnOpen != null)
		{
			btnOpen.gameObject.SetActive(false);
		}
		if (btnRecieve != null)
		{
			btnRecieve.gameObject.SetActive(false);
		}
		if (cellItem != null)
		{
			cellItem.gameObject.SetActive(false);
		}
		if (spineAnim == null)
		{
			return;
		}
		spineAnim.gameObject.SetActive(true);
		spineAnim.AnimationState.SetAnimation(0, "up", false);
		ClearTwAnim();
		twAnim = DOTween.Sequence()
			.AppendInterval(1.5f)
			.AppendCallback(delegate
			{
				spineAnim.AnimationState.SetAnimation(0, "loop", true);
			})
			.AppendInterval(0.6f)
			.AppendCallback(delegate
			{
				if (btnOpen != null)
				{
					btnOpen.gameObject.SetActive(true);
				}
			});
	}

	private void ClearTwAnim()
	{
		if (twAnim != null)
		{
			twAnim.Kill(false);
			twAnim = null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (twAnim != null)
		{
			twAnim.Kill(false);
			twAnim = null;
		}
	}

	public void ClickBtnOpen(MButton button)
	{
		if (btnOpen != null)
		{
			btnOpen.gameObject.SetActive(false);
		}
		if (spineAnim != null)
		{
			spineAnim.AnimationState.SetAnimation(0, "click", false).Complete += delegate
			{
				spineAnim.AnimationState.SetAnimation(0, "click_loop", true);
			};
			ClearTwAnim();
			twAnim = DOTween.Sequence()
				.AppendInterval(0.5f)
				.AppendCallback(ShowReward)
				.AppendInterval(0.6f)
				.AppendCallback(delegate
				{
					if (btnRecieve != null)
					{
						btnRecieve.gameObject.SetActive(true);
					}
				});
		}
	}

	private void ShowReward()
	{
		if (tfReward != null)
		{
			tfReward.gameObject.SetActive(true);
		}
		if (cfg == null || tfItems == null)
		{
			return;
		}
		if (cfg.reward != null)
		{
			int length = cfg.reward.GetLength(0);
			int num = Mathf.Min(length, tfItems.Length);
			if (num < 1)
			{
				return;
			}
			for (int i = 0; i < num; i++)
			{
				RecallItem recallItem = UnityEngine.Object.Instantiate(cellItem);
				recallItem.gameObject.SetActive(true);
				recallItem.Init(cfg.reward[i, 0], cfg.reward[i, 1]);
				recallItem.transform.SetParent(tfItemsRoot, false);
				CanvasGroup target = recallItem.gameObject.AddComponent<CanvasGroup>();
				Sequence sequence = DOTween.Sequence();
				sequence.Append(target.DOFade(1f, 0.6f));
				sequence.Join(recallItem.transform.DOScale(Vector3.one, 0.6f));
				sequence.Join(recallItem.transform.DOLocalMove(tfItems[i].localPosition, 0.6f, false));
				sequence.SetEase(Ease.OutQuad);
			}
		}
	}

	public void Show()
	{
		cfg = RecallMgr.I.GetRewards();
		if (cfg == null)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		descTex.SetLBy(cfg.condition);
		LogEvent.I.Track("Recall_Open").Add("RecallTime", cfg.condition).Add("Level_ID", DB.User.Maxlv + 1).Send(false);
	}

	public void ClickReceive(MButton btn)
	{
		if (btn != null)
		{
			btn.interactable = false;
			RecallMgr.I.Receive();
			if (cfg != null)
			{
				int length = cfg.reward.GetLength(0);
				for (int i = 0; i < length; i++)
				{
					int id = cfg.reward[i, 0];
					int count = cfg.reward[i, 1];
					if (id == (int)PropID.EnergyTime)
					{
						DB.Bag.AddEnergyUnlimitedTime((float)(count * 3600));
					}
					else
					{
						DB.Bag.Modify((PropID)id, count, true, "Recall");
					}
				}
				LogEvent.I.Track("Recall_Reward").Add("RecallTime", cfg.condition).Add("Level_ID", DB.User.Maxlv + 1).Send(false);
			}
			_Close(closeCallback);
		}
	}
}
