using System.Collections.Generic;
using UnityEngine;

public class SelectMedalDialog : DialogBase
{
	[SerializeField]
	private Transform tfTips;

	[SerializeField]
	private Transform tfGuide;

	[SerializeField]
	private MButton BtnSave;

	[SerializeField]
	private MButton BtnDesc;

	[SerializeField]
	private MButton BtnClose;

	[SerializeField]
	private SelectMedalScrollView pScroll;

	[SerializeField]
	private MButton[] tabTypeBtns;

	[SerializeField]
	private GameObject normallyButtonRedObj;

	[SerializeField]
	private GameObject specialButtonRedObj;

	private int iSelectGroupIndex = -1;

	private int iGroupCount;

	private int iTypeIndex = 1;

	private MedalType medalType = MedalType.Special;

	protected override void Awake()
	{
		base.Awake();
		DispatcherMgr.On("Update_User_MedalRed", _ShowButtonsRed);
	}

	private void Start()
	{
		if (BtnSave != null)
		{
			BtnSave.onClick.AddListener(OnClickBtnSave);
		}
		if (BtnDesc != null)
		{
			BtnDesc.onClick.AddListener(OnClickBtnDesc);
		}
		if (BtnClose != null)
		{
			BtnClose.onClick.AddListener(OnClickBtnClose);
		}
		if (tabTypeBtns != null)
		{
			for (int i = 0; i < tabTypeBtns.Length; i++)
			{
				tabTypeBtns[i].onClick.AddListener(OnClickTabBtn);
			}
		}
		_ShowButtonsRed();
		SetSelectBigButton(0);
		_Open();
		LogEvent.I.Track("Medal_Open").CommonAttr(true).Send(false);
	}

	private void ShowGuide()
	{
		if (DB.User.IsFirstEnterMedal)
		{
			DB.User.IsFirstEnterMedal = false;
			MedalGuideInside medalGuideInside = SceneMgr.AddTopNode<MedalGuideInside>("Prefabs/Medal/MedalGuideInside");
			if (medalGuideInside != null)
			{
				medalGuideInside.Show(tfGuide);
			}
		}
	}

	private void OnClickBtnSave(MButton button)
	{
		if (!CheckCurSelectIsNotUnlock())
		{
			LogEvent.I.Track("Medal_Change").Add("Medal_ID", MedalMgr.I.CurSelectId).CommonAttr(true).Send(false);
			DB.User.Medal = MedalMgr.I.CurSelectId;
			DB.User.IsChangedMedal = true;
			DispatcherMgr.Emit("Update_User_Medal");
			_Close();
		}
		else
		{
			Global.ShowMsgTip("Language297");
		}
	}

	private void OnClickBtnDesc(MButton button)
	{
		SceneMgr.AddTopNode<DialogShow>("Prefabs/Medal/MedalDescDialog");
	}

	private bool CheckCurSelectIsNotUnlock()
	{
		cfg_medal cfgById = MedalMgr.I.GetCfgById(MedalMgr.I.CurSelectId);
		if (cfgById.Type == 2)
		{
			return !DB.Bag.Medal.Contains(cfgById.ID);
		}
		if (cfgById.Type == 1)
		{
			return DB.User.LvMedal < cfgById.Lv;
		}
		return false;
	}

	private void OnClickBtnClose(MButton button)
	{
		_Close(delegate
		{
			if (DB.User.IsFirstEnterMedal)
			{
				DB.User.IsFirstEnterMedal = false;
			}
			if (!CheckCurSelectIsNotUnlock() && MedalMgr.I.CurSelectId != DB.User.Medal)
			{
				Global.CreateMsgBoxDialog("Language295", delegate(bool bIsOk)
				{
					if (!bIsOk)
					{
						MedalMgr.I.CurSelectId = DB.User.Medal;
					}
					else
					{
						LogEvent.I.Track("Medal_Change").Add("Medal_ID", MedalMgr.I.CurSelectId).CommonAttr(true).Send(false);
						DB.User.Medal = MedalMgr.I.CurSelectId;
						DB.User.IsChangedMedal = true;
						DispatcherMgr.Emit("Update_User_Medal");
					}
				}, isTwoBtn: true);
			}
			else
			{
				MedalMgr.I.CurSelectId = DB.User.Medal;
			}
		});
	}

	private void OnClickTabBtn(MButton button)
	{
		if (iTypeIndex != button.nTag)
		{
			SetSelectBigButton(button.nTag, bRefreshData: true);
		}
	}

	private void SetSelectBigButton(int index, bool bRefreshData = false)
	{
		tabTypeBtns[iTypeIndex].interactable = true;
		iTypeIndex = index;
		tabTypeBtns[index].interactable = false;
		medalType = (MedalType)(index + 1);
		Debug.Log(string.Format("medalType:{0}", (int)medalType));
		if (bRefreshData)
		{
			pScroll.SetMedalType(medalType);
		}
	}

	private void _ShowButtonsRed()
	{
		bool active = MedalMgr.I.JudgeHasNewGetMedals(new List<MedalType> { MedalType.Normall });
		normallyButtonRedObj.SetActive(active);
		bool active2 = MedalMgr.I.JudgeHasNewGetMedals(new List<MedalType> { MedalType.Special });
		specialButtonRedObj.SetActive(active2);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DispatcherMgr.Off("Update_User_MedalRed", _ShowButtonsRed);
	}
}
