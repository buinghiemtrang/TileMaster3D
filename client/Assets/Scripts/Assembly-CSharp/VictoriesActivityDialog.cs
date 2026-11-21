using System;
using System.Collections.Generic;
using System.Text;
using Festival;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class VictoriesActivityDialog : DialogBase
{
	[SerializeField]
	private Text timeTex;

	[SerializeField]
	private Transform rewardParentNodeTran;

	[SerializeField]
	private Transform highestTran;

	[SerializeField]
	private Text highestValueTex;

	[SerializeField]
	private GameObject rewardItemObj;

	[SerializeField]
	private Text btnTex;

	[SerializeField]
	private GameObject closeBtnObj;

	[SerializeField]
	private Image barImg;

	[SerializeField]
	private GameObject EnergyIconObj;

	[SerializeField]
	private Text energyTex;

	[SerializeField]
	private List<VictoriesActivityRewardItem> items;

	[SerializeField]
	private MButton closeBtn;

	[SerializeField]
	private MButton helpBtn;

	[SerializeField]
	private MButton playBtn;

	public Action<int> closedCallback;

	private int canReceiveNum;

	private bool isActivityEnd;

	protected override void Awake()
	{
		base.Awake();
		closeBtn.onClick.AddListener(_ClickClose);
		helpBtn.onClick.AddListener(_ClickHelp);
		playBtn.onClick.AddListener(_ClickPlayOrReceive);
		DispatcherMgr.On<int>("VictoriesActivity_TimeChange", _RefreshTime);
	}

	public void Show(string openType, int surTime)
	{
		DB.VictoriesActivity.IsAutoEjectedActivityDialog = true;
		isActivityEnd = VictoriesActivityMgr.I.RefreshData() == VictoriesActivityState.NoStart;
		_RefreshUI();
		_RefreshTime(surTime);
		highestTran.gameObject.SetActive(DB.VictoriesActivity.HighestVictoryNum > 0);
		if (DB.VictoriesActivity.HighestVictoryNum >= 1)
		{
			highestValueTex.text = string.Format("Max:{0}", DB.VictoriesActivity.HighestVictoryNum);
			_SetPos(DB.VictoriesActivity.HighestVictoryNum, highestTran);
		}
		barImg.fillAmount = (float)DB.VictoriesActivity.CurVictoryNum / (float)DB.VictoriesActivity.MaxVictoryNum;
		List<VictoriesActivityRewardInfo> rewardInfos = VictoriesActivityMgr.I.GetRewardInfos();
		if (items.Count == rewardInfos.Count)
		{
			for (int i = 0; i < items.Count; i++)
			{
				if (rewardInfos[i].rewardState == 1)
				{
					canReceiveNum++;
				}
				_SetPos(rewardInfos[i].cfg.condition, items[i].transform);
				items[i].onReceived = delegate
				{
					canReceiveNum--;
					_RefreshUI();
				};
				items[i].Init(rewardInfos[i]);
			}
			_Open();
			LogEvent.I.Track("Activity_Victories_Open").CommonAttr(true).Send();
			LogEvent.I.HomeFrameOpen("WinningStreak", openType);
		}
		else
		{
			Debug.LogErrorFormat("Victories activity info num not equal to {0}", items.Count);
		}
	}

	private void _RefreshUI()
	{
		closeBtnObj.SetActive(canReceiveNum < 1);
		btnTex.SetLKey((canReceiveNum < 1) ? "Language276" : "Language277");
		EnergyIconObj.SetActive(!DB.VictoriesActivity.IsReceivedEnergy);
		energyTex.text = string.Format("{0}m", cf_const.victoriesActivityRewardEnergyTime / 60f);
	}

	private void _SetPos(int num, Transform target)
	{
		float x = rewardParentNodeTran.RT().sizeDelta.x;
		target.SetLocPosX((float)num / (float)DB.VictoriesActivity.MaxVictoryNum * x - 0.5f * x);
	}

	private void _RefreshTime(int surTime)
	{
		timeTex.text = Helper.FormatTimer(surTime, "hh:mm:ss");
	}

	private void _ClickHelp(MButton btn)
	{
		Singleton<FestivalMgr>.I.AddTopNode<DialogShow>("Prefabs/Activity/Victories/{0}/VictoriesActivityHelpDialog");
	}

	private void _ClickPlayOrReceive(MButton btn)
	{
		btn.interactable = false;
		if (!DB.VictoriesActivity.IsReceivedEnergy)
		{
			DB.VictoriesActivity.IsReceivedEnergy = true;
			DB.Bag.AddEnergyUnlimitedTime(cf_const.victoriesActivityRewardEnergyTime);
		}
		if (canReceiveNum < 1)
		{
			_EnterGame();
			_Close(delegate
			{
				closedCallback?.Invoke(3);
			});
			return;
		}
		int[,] reward = VictoriesActivityMgr.I.Receive(-1);
		canReceiveNum = 0;
		string text = null;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].info.rewardState == 1)
			{
				text = items[i].skinName;
			}
		}
		RewardUnifyDialog rewardUnifyDialog = Global.CreateReward(reward, (RewardType)3, 0, "Victories", false, false);
		if (rewardUnifyDialog != null)
		{
			rewardUnifyDialog.SetChestSkin(text);
			rewardUnifyDialog.SetComplete(delegate
			{
				_RefreshUI();
				for (int k = 0; k < items.Count; k++)
				{
					if (items[k].info.rewardState == 1)
					{
						items[k].info.rewardState = 2;
						items[k].RefreshUI();
					}
				}
				if (isActivityEnd)
				{
					_Close(delegate
					{
						closedCallback?.Invoke(1);
					});
				}
				else
				{
					btn.interactable = true;
				}
			});
			StringBuilder stringBuilder = new StringBuilder();
			if (reward != null)
			{
				int length = reward.GetLength(0);
				for (int j = 0; j < length; j++)
				{
					stringBuilder.Append(reward[j, 0]);
					stringBuilder.Append(";");
					stringBuilder.Append(reward[j, 1]);
					if (j < length - 1)
					{
						stringBuilder.Append(",");
					}
				}
				LogEvent.I.Track("Activity_Victories_Reward")
					.Add("VictoriesNum", DB.VictoriesActivity.HighestVictoryNum)
					.Add("TaskItem", stringBuilder.ToString())
					.CommonAttr(true)
					.Send();
			}
		}
	}

	private void _ClickClose(MButton btn)
	{
		btn.interactable = false;
		_Close(delegate
		{
			closedCallback?.Invoke(2);
		});
	}

	private void _EnterGame()
	{
		Action action = delegate
		{
			BlackCutDialog.EnterGame();
		};
		if (DB.Bag.JudgeHasEnergy())
		{
			action();
		}
		else
		{
			EnergyDialog.Create(action, delegate
			{
				SceneMainBottom.instance.ClickBtnShop(null);
			});
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DispatcherMgr.Off<int>("VictoriesActivity_TimeChange", _RefreshTime);
	}
}
