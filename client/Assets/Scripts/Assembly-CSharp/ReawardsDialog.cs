using System;
using System.Collections.Generic;
using DG.Tweening;
using Festival;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class ReawardsDialog : DialogBase
{
	private class rewInfo
	{
		public bool isVipReward;

		public int propId;

		public int value;
	}

	public Transform allItem;

	public Transform allPos;

	public MButton adBtn;

	public MButton receiveBtn;

	public MButton adRecriveBtn;

	public Transform chestParentTran;

	public Action<bool> cback;

	private RewardItem[] allItems = new RewardItem[9];

	public Text textTitle;

	private List<rewInfo> rews;

	private List<int> iNums;

	private REWARD_PLACE place;

	private RewardType type;

	private string skinName = "";

	private string sSign = "";

	private bool isAd;

	private int itemWidth = 83;

	private float DOLocalMoveTime = 0.3f;

	private int testNum = 1;

	private int[,] tempRews;

	private SkeletonGraphic anim;

	private Spine.AnimationState.TrackEntryDelegate ac;

	private bool isPause;

	protected override void Awake()
	{
		base.Awake();
		rews = new List<rewInfo>();
		iNums = new List<int>();
		receiveBtn.onClick.AddListener(_ordinary);
		adRecriveBtn.onClick.AddListener(_ordinary);
		adBtn.onClick.AddListener(_ad);
		if ((bool)textTitle)
		{
			textTitle.gameObject.SetActive(value: false);
		}
	}

	private void Start()
	{
	}

	public void onInit()
	{
		for (int i = 0; i < allItem.childCount; i++)
		{
			allItem.GetChild(i).localPosition = new Vector3(0f, -246f, 0f);
		}
		testNum++;
	}

	public void onOpen()
	{
		// UNVERIFIED: the original passes a constant int[4,2] built by RuntimeHelpers.InitializeArray from <PrivateImplementationDetails> data (RVA 0x11bc188); the blob values were not extracted, so the call is not reproduced.
	}

	public void SetChestSkin(string skinName)
	{
		this.skinName = skinName;
	}

	private void Init()
	{
		for (int i = 0; i < allItem.childCount; i++)
		{
			allItems[i] = allItem.GetChild(i).GetComponent<RewardItem>();
		}
	}

	private void HandleEvent()
	{
		string name = anim.AnimationState.Tracks.Items[0].Animation.Name;
		if (name == "close_idle" || name == "close-idle")
		{
			AudioMgr.I.PlaySound("chest_open");
			anim.AnimationState.SetAnimation(0, "open", loop: false);
		}
		else if (name == "open")
		{
			onStartMove(tempRews);
			anim.AnimationState.SetAnimation(0, "open-idle", loop: true);
			adBtn.gameObject.SetActive(place != REWARD_PLACE.None && type != RewardType.Novice);
			receiveBtn.gameObject.SetActive(place == REWARD_PLACE.None || type == RewardType.Novice);
			if ((bool)textTitle)
			{
				textTitle.gameObject.SetActive(type == RewardType.Novice);
			}
		}
		else if (name == "open-idle")
		{
			anim.AnimationState.Complete -= ac;
		}
		Logs.Common.Info("Play a footstep sound!" + anim.AnimationState.Tracks.Items[0].Animation.Name);
	}

	public void showView(RewardType type, int[] iNums, int[,] rews, REWARD_PLACE place = REWARD_PLACE.None, bool isShowExeAdIcon = false, string s_sign = "", string skinName = "")
	{
		Init();
		tempRews = rews;
		sSign = s_sign;
		this.place = place;
		this.type = type;
		DispatcherMgr.Emit("GamePause", true, "Reward");
		if (type == RewardType.ActivityOne)
		{
			type = RewardType.RoyalPassActivity;
		}
		if (type == RewardType.None)
		{
			adBtn.gameObject.SetActive(value: false);
			receiveBtn.gameObject.SetActive(value: true);
			onStartMove(tempRews);
			chestParentTran.gameObject.SetActive(value: false);
		}
		else
		{
			adBtn.gameObject.SetActive(value: false);
			receiveBtn.gameObject.SetActive(value: false);
			string text = "{0}_Chest";
			if (type != RewardType.Chest && type != RewardType.Novice)
			{
				text = ((type != RewardType.RoyalPassActivity) ? type.ToString() : ("{0}_" + type.ToString()));
			}
			GameObject gameObject = Singleton<FestivalMgr>.I.LoadAsset<GameObject>("Chests/" + text);
			if ((bool)gameObject)
			{
				anim = UnityEngine.Object.Instantiate(gameObject, chestParentTran).GetComponent<SkeletonGraphic>();
				anim.AnimationState.SetAnimation(0, "close-idle", loop: false);
				ac = delegate
				{
					HandleEvent();
				};
				anim.AnimationState.Complete += ac;
				if (!string.IsNullOrEmpty(skinName))
				{
					anim.Skeleton.SetSkin(skinName);
					anim.Skeleton.SetSlotsToSetupPose();
				}
			}
			else
			{
				Debug.LogError("没找找到宝箱 type:" + type);
			}
		}
		for (int i = 0; i < rews.GetLength(0); i++)
		{
			int num = rews[i, 0];
			if (num != 20 || isShowExeAdIcon || !Global.pStores.AdFree)
			{
				this.iNums.Add(iNums[i]);
				int num2 = rews[i, 1];
				if (num2 < 1)
				{
					rews[i, 1] = -num2;
				}
				this.rews.Add(new rewInfo
				{
					propId = num,
					isVipReward = (num2 < 1),
					value = rews[i, 1]
				});
			}
		}
		for (int j = 0; j < rews.GetLength(0); j++)
		{
			allItems[j].Init(isVipReward: false, rews[j, 0], rews[j, 1], isLight: false);
		}
		Logs.Common.Info("传入个数 = " + rews.GetLength(0));
	}

	private void onStartMove(int[,] rews)
	{
		switch (rews.GetLength(0))
		{
		case 1:
			moveFun(0, 0, 0);
			break;
		case 2:
			moveFun(0, 0, -itemWidth);
			moveFun(1, 0, itemWidth);
			break;
		case 3:
			moveFun(0, 0, itemWidth * -2);
			moveFun(1, 0, 0);
			moveFun(2, 0, itemWidth * 2);
			break;
		case 4:
			moveFun(0, 0, itemWidth * -3);
			moveFun(1, 0, -itemWidth);
			moveFun(2, 0, itemWidth);
			moveFun(3, 0, itemWidth * 3);
			break;
		case 5:
			moveFun(0, 1, itemWidth * -2);
			moveFun(1, 1, 0);
			moveFun(2, 1, itemWidth * 2);
			moveFun(3, 2, -itemWidth);
			moveFun(4, 2, itemWidth);
			break;
		case 6:
			moveFun(0, 1, itemWidth * -2);
			moveFun(1, 1, 0);
			moveFun(2, 1, itemWidth * 2);
			moveFun(3, 2, itemWidth * -2);
			moveFun(4, 2, 0);
			moveFun(5, 2, itemWidth * 2);
			break;
		case 7:
			moveFun(0, 1, itemWidth * -3);
			moveFun(1, 1, -itemWidth);
			moveFun(2, 1, itemWidth);
			moveFun(3, 1, itemWidth * 3);
			moveFun(4, 2, itemWidth * -2);
			moveFun(5, 2, 0);
			moveFun(6, 2, itemWidth * 2);
			break;
		case 8:
			moveFun(0, 1, itemWidth * -3);
			moveFun(1, 1, -itemWidth);
			moveFun(2, 1, itemWidth);
			moveFun(3, 1, itemWidth * 3);
			moveFun(4, 2, itemWidth * -3);
			moveFun(5, 2, -itemWidth);
			moveFun(6, 2, itemWidth);
			moveFun(7, 2, itemWidth * 3);
			break;
		}
	}

	private void moveFun(int id, int posID, int width)
	{
		Transform child = allItem.GetChild(id);
		Vector3 localPosition = allPos.GetChild(posID).localPosition;
		Vector3 localPosition2 = allPos.GetChild(posID).localPosition;
		localPosition2.x = localPosition.x + (float)width;
		localPosition2.z = 0f;
		child.DOLocalMove(localPosition2, DOLocalMoveTime);
		allItem.GetChild(id).DOScale(1f, DOLocalMoveTime);
	}

	private void Update()
	{
	}

	private void _ordinary(MButton btn)
	{
		btn.interactable = false;
		adBtn.interactable = false;
		isAd = false;
		for (int i = 0; i < rews.Count; i++)
		{
			int propId = rews[i].propId;
			if (propId > 23 || ((1 << propId) & 0xB00000) == 0)
			{
				int n_raw = iNums[i];
				if (propId == 2)
				{
					DB.Bag.ModifyShow(PropID.Energy);
				}
				else
				{
					DB.Bag.ModifyShow((PropID)propId, ref n_raw, rews[i].value);
				}
			}
		}
		_close();
	}

	private void _ad(MButton btn)
	{
		Global.ShowRewardedVideo(delegate(string info)
		{
			if (info.Equals("succ"))
			{
				_receive(isAd: true);
			}
		}, place);
	}

	private void _receive(bool isAd)
	{
		this.isAd = isAd;
		for (int i = 0; i < rews.Count; i++)
		{
			int propId = rews[i].propId;
			if (propId > 23 || ((1 << propId) & 0xB00000) == 0)
			{
				int n_raw = iNums[i];
				int n_count = rews[i].value;
				if (isAd)
				{
					int value = rews[i].value;
					n_count = value + n_count;
					DB.Bag.Modify((PropID)propId, rews[i].value, b_update: false, sSign);
				}
				if (propId == 2)
				{
					DB.Bag.ModifyShow(PropID.Energy);
				}
				else
				{
					DB.Bag.ModifyShow((PropID)propId, ref n_raw, n_count);
				}
			}
		}
		_close();
	}

	private void _close()
	{
		DispatcherMgr.Emit("GamePause", false, "Reward");
		_Close(delegate
		{
			cback?.Invoke(isAd);
		});
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
