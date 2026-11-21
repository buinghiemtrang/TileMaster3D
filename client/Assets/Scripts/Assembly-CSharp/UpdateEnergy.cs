using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UpdateEnergy : MonoBehaviour
{
	public Text ttNum;

	public Text ttTime;

	public GameObject infiniteIconObj;

	public MButton mButton;

	public Transform tfTimeOff;

	private Vector3 vTimeRaw;

	private PropID kProp = PropID.Energy;

	private Tween unlimitedTw;

	private Tween normalTw;

	protected virtual void Awake()
	{
		if ((bool)mButton)
		{
			mButton.onClick.AddListener(ClickBtn);
		}
		if ((bool)ttNum)
		{
			ttNum.text = "";
		}
		if ((bool)ttTime)
		{
			ttTime.text = "";
			ttTime.SetLEvent(delegate
			{
				ttTime.SetLKey("Language52");
			});
			vTimeRaw = ttTime.transform.localPosition;
		}
		DispatcherMgr.On<int, int>("Update" + kProp, UpdateText);
		DispatcherMgr.On("ForceUpdate" + kProp, Start);
		DispatcherMgr.On("Update_EnergyRecoverUpLimit_Change", Start);
	}

	private void Start()
	{
		_clearTween();
		Refresh();
	}

	public void Refresh()
	{
		if (DB.Bag.JudgeForceUnlimitedEnergy())
		{
			_showUnlimited();
			return;
		}
		int energyUnlimitedTime = DB.Bag.GetEnergyUnlimitedTime();
		if (energyUnlimitedTime != 0)
		{
			ShowUnlimitedTime(energyUnlimitedTime);
		}
		else
		{
			ShowNormalTime(DB.Bag.GetProp(kProp));
		}
	}

	private void ShowNormalTime(int value)
	{
		int maxEnergyRecover = DB.Bag.GetMaxEnergyRecover();
		SetBuyBtn(value < maxEnergyRecover);
		if ((bool)ttNum)
		{
			ttNum.text = value.ToString();
		}
		_showInfiniteIcon(isShow: false);
		_clearTween();
		if (maxEnergyRecover <= value)
		{
			if ((bool)ttTime)
			{
				ttTime.ExecuteLEvent();
			}
			if (!string.IsNullOrEmpty(DB.Bag.EnergyGainTime))
			{
				DB.Bag.EnergyGainTime = string.Empty;
			}
			return;
		}
		DateTime now = DateTime.Now;
		DateTime result;
		if (string.IsNullOrEmpty(DB.Bag.EnergyGainTime) || !DateTime.TryParse(DB.Bag.EnergyGainTime, out result))
		{
			result = now;
			DB.Bag.EnergyGainTime = result.ToString();
		}
		int num = (int)(now - result).TotalSeconds;
		int addEnergyTime = cf_const.addEnergyTime;
		int num2 = num / addEnergyTime;
		int num3 = 0;
		if (value + num2 > maxEnergyRecover)
		{
			num3 = maxEnergyRecover - (value + num2);
		}
		num2 += num3;
		if (num2 < 1)
		{
			if (normalTw != null)
			{
				normalTw.Kill();
				normalTw = null;
			}
			normalTw = TweenMgr.NumberTo(addEnergyTime - num, addEnergyTime - num, 0, delegate(int v)
			{
				if ((bool)ttTime)
				{
					ttTime.text = Util.Helper.FormatTimer(v, "mm:ss");
				}
			}).SetEase(Ease.Linear).SetId("UpdateEnergy_Normal").OnComplete(delegate
			{
				DB.Bag.EnergyGainTime = string.Empty;
				if (DB.Bag.GetProp(kProp) + 1 <= DB.Bag.GetMaxEnergyRecover())
				{
					DB.Bag.ModifyEnergy(1, b_update: true, "Time");
				}
			});
		}
		else
		{
			DB.Bag.EnergyGainTime = result.AddSeconds(num2 * addEnergyTime).ToString();
			DB.Bag.ModifyEnergy(num2, b_update: true, "Time");
		}
	}

	private void ShowUnlimitedTime(int time)
	{
		SetBuyBtn(b_active: false);
		_showInfiniteIcon(isShow: true);
		_clearTween();
		unlimitedTw = TweenMgr.NumberTo(time, time, 0, delegate(int v)
		{
			if ((bool)ttTime)
			{
				ttTime.text = Util.Helper.FormatTimer(v, "hh:mm:ss", b_shifty: true);
			}
		}).SetEase(Ease.Linear).SetId("UpdateEnergy_Unlimited").OnComplete(delegate
		{
			DB.Bag.EnergyUnlimitedTime = string.Empty;
			ShowNormalTime(DB.Bag.GetProp(kProp));
		});
	}

	private void _showUnlimited()
	{
		if (DB.Bag.GetProp(kProp) < cf_const.maxEnergyRecover)
		{
			DB.Bag.SetProp(kProp.ToString(), cf_const.maxEnergyRecover);
		}
		_showInfiniteIcon(isShow: true);
		SetBuyBtn(b_active: false);
		if ((bool)ttTime)
		{
			ttTime.ExecuteLEvent();
		}
	}

	private void SetBuyBtn(bool b_active)
	{
		if (!(mButton == null))
		{
			mButton.gameObject.SetActive(b_active);
			if (!(tfTimeOff == null) && (bool)ttTime)
			{
				ttTime.transform.SetLocPos(b_active ? tfTimeOff.localPosition : vTimeRaw);
			}
		}
	}

	public virtual void UpdateText(int last = 0, int end = 0)
	{
		if ((bool)ttNum)
		{
			ttNum.text = end.ToString();
		}
		_showInfiniteIcon(isShow: false);
		ShowNormalTime(end);
	}

	private void _showInfiniteIcon(bool isShow)
	{
		if ((bool)infiniteIconObj)
		{
			infiniteIconObj.SetActive(isShow);
		}
	}

	public void ClickBtn(MButton button)
	{
		EnergyDialog.Create(null, delegate
		{
			SceneMainBottom.instance.btnShop.ClickSelf();
		});
		LogEvent.I.HomeButtonClick("LifeButton");
	}

	private void _clearTween()
	{
		if (unlimitedTw != null)
		{
			unlimitedTw.Kill();
		}
		if (normalTw != null)
		{
			normalTw.Kill();
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			_clearTween();
			Refresh();
		}
	}

	private void OnDestroy()
	{
		_clearTween();
		DispatcherMgr.Off("ForceUpdate" + kProp, Start);
		DispatcherMgr.Off<int, int>("Update" + kProp, UpdateText);
		DispatcherMgr.Off("Update_EnergyRecoverUpLimit_Change", Start);
	}
}
