using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelUnlockProgressCom : MonoBehaviour
{
	public Text numTxt;

	public Image pro;

	[Header("是否是胜利界面")]
	public bool isVictoryPanel;

	private bool bHasUnlock;

	private Action endCall;

	private Tweener tw;

	private static bool UnlockAll()
	{
		if (DataCfg.cfgLevelUnlocks.Count != 0)
		{
			return DB.User.Curlv + 1 > DataCfg.cfgLevelUnlocks[DataCfg.cfgLevelUnlocks.Count - 1].id;
		}
		return true;
	}

	public static bool HaveUnlock()
	{
		if (UnlockAll())
		{
			return false;
		}
		int num = DB.User.Curlv + 1;
		for (int i = 0; i < DataCfg.cfgLevelUnlocks.Count; i++)
		{
			if (DataCfg.cfgLevelUnlocks[i].id == num)
			{
				return true;
			}
		}
		return false;
	}

	private void Awake()
	{
		bHasUnlock = HaveUnlock();
		if (UnlockAll() || (isVictoryPanel && bHasUnlock))
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			setProgress();
		}
	}

	public void setEndCall(Action endCall)
	{
		this.endCall = endCall;
	}

	private void setProgress()
	{
		int num = 0;
		int num2 = 0;
		int num3 = DB.User.Curlv + 1;
		int num4 = 0;
		int num5 = 0;
		int num6;
		do
		{
			num6 = num5;
			num5 = DataCfg.cfgLevelUnlocks[num4].id;
			num4++;
		}
		while (num5 < num3);
		num2 = num5 - num6;
		num = num3 - num6;
		numTxt.text = num + "/" + num2;
		float nowPro = (float)num / (float)num2;
		tw = TweenMgr.NumberTo(1f, (float)(num - 1) / (float)num2, nowPro, delegate(float v)
		{
			pro.fillAmount = v;
			if (nowPro == v)
			{
				tw = null;
				tw.Kill();
				if (endCall != null)
				{
					endCall();
				}
				endCall = null;
			}
		});
	}

	private void OnDestroy()
	{
		if (tw != null)
		{
			tw.Kill();
		}
	}
}
