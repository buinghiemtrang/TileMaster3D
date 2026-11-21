using System;
using Authentication;
using BestHTTP;
using Ranking;
using UnityEngine;
using UnityEngine.UI;

public class RankingRootControl : MonoBehaviour
{
	[SerializeField]
	private RankingControlBase[] pRControls;

	[SerializeField]
	private MButton[] tabBigButtons;

	[SerializeField]
	private MButton[] tabSmallButtons;

	[SerializeField]
	protected RankingScrollView pScroll;

	[SerializeField]
	protected Image imageLine;

	[SerializeField]
	private NetworkTips networkTips;

	[SerializeField]
	private Transform tfTop;

	private int bigIndex = 1;

	private int smallIndex = 1;

	private int typeIndex;

	private bool bIsFirstEnter = true;

	private void Awake()
	{
		DispatcherMgr.On("Change_Module_Event", ClearAllEvent);
		DispatcherMgr.On<GuestLoginState>("Guest_Login", OnGuestLogin);
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off("Change_Module_Event", ClearAllEvent);
		DispatcherMgr.Off<GuestLoginState>("Guest_Login", OnGuestLogin);
		RankingUtils.GetMonthIssueCallback = null;
	}

	private void Start()
	{
		if (tabBigButtons != null)
		{
			for (int i = 0; i < tabBigButtons.Length; i++)
			{
				tabBigButtons[i].onClick.AddListener(OnClickBigBtn);
			}
		}
		if (tabSmallButtons != null)
		{
			for (int j = 0; j < tabSmallButtons.Length; j++)
			{
				tabSmallButtons[j].onClick.AddListener(OnClickSmallBtn);
			}
		}
		RankingUtils.GetMonthIssueCallback = delegate(HTTPRequestStates state)
		{
			if (state != HTTPRequestStates.Aborted)
			{
				if (DB.User.MonthIssue.Equals(string.Empty))
				{
					if (RankMaskCtl.I != null)
					{
						RankMaskCtl.I.Hide();
					}
					if (networkTips != null)
					{
						networkTips.Show();
					}
				}
				else
				{
					ControlLoadData();
				}
			}
		};
		SetSelectBigButton(0);
		SetSelectSmallButton(0);
		pRControls[typeIndex].gameObject.SetActive(value: true);
		if (tfTop != null)
		{
			tfTop.gameObject.SetActive(value: false);
		}
		if (!DataManager.I.bLogined)
		{
			OwnServerLogin.Instance.Login(PlatformType.Guest, DataCenterLoginMgr.I.LoginFinish);
		}
		else
		{
			LoadRanking();
		}
	}

	private void LoadRanking()
	{
		if (networkTips != null)
		{
			networkTips.Hide();
		}
		if (!DB.Account.IsEditorName())
		{
			RankNameDialog.Show(delegate
			{
				DB.Account.Name = DB.Account.GetName();
				_Load();
			}, delegate
			{
				_Load();
			});
		}
		else
		{
			_Load();
		}
	}

	private void _Load()
	{
		if (RankMaskCtl.I != null)
		{
			RankMaskCtl.I.Show(0f, isNeedMask: false);
		}
		if (pScroll != null)
		{
			pScroll.gameObject.SetActive(value: false);
		}
		if (imageLine != null)
		{
			imageLine.gameObject.SetActive(value: false);
		}
		if (bigIndex == 1 && DB.User.MonthIssue.Equals(string.Empty))
		{
			RankingUtils.ReqMonthIssue();
		}
		else
		{
			ControlLoadData();
		}
	}

	private void _GoHome()
	{
		DispatcherMgr.Emit("ActiveHomeBtn");
	}

	private void LoadCallback(bool loadSucceeded, HTTPRequestStates state)
	{
		if (state == HTTPRequestStates.Aborted)
		{
			if (!loadSucceeded)
			{
				return;
			}
		}
		else
		{
			if (RankMaskCtl.I != null)
			{
				RankMaskCtl.I.Hide();
			}
			if (!loadSucceeded)
			{
				if (networkTips != null)
				{
					networkTips.Show();
				}
				return;
			}
		}
		if (tfTop != null && !tfTop.gameObject.activeSelf)
		{
			tfTop.gameObject.SetActive(value: true);
		}
		LogEvent.I.Track("RankShow").Add("RankNum", GetOwnRanking()).CommonAttr(true)
			.Send();
	}

	private void ControlLoadData()
	{
		pRControls[typeIndex].LoadData(LoadCallback);
	}

	private int GetOwnRanking()
	{
		return pRControls[typeIndex].OwnRanking;
	}

	private void OnClickBigBtn(MButton button)
	{
		if (bigIndex != button.nTag)
		{
			SetSelectBigButton(button.nTag, bIsChangePanel: true);
		}
	}

	private void SetSelectBigButton(int index, bool bIsChangePanel = false)
	{
		tabBigButtons[bigIndex].interactable = true;
		tabBigButtons[bigIndex].transform.SetAsFirstSibling();
		bigIndex = index;
		tabBigButtons[index].interactable = false;
		tabBigButtons[index].transform.SetAsLastSibling();
		if (bIsChangePanel)
		{
			ChangePanel();
		}
	}

	private void OnClickSmallBtn(MButton button)
	{
		if (smallIndex != button.nTag)
		{
			SetSelectSmallButton(button.nTag, bIsChangePanel: true);
		}
	}

	private void SetSelectSmallButton(int index, bool bIsChangePanel = false)
	{
		tabSmallButtons[smallIndex].interactable = true;
		smallIndex = index;
		tabSmallButtons[index].interactable = false;
		if (bIsChangePanel)
		{
			ChangePanel();
		}
	}

	private void ChangePanel()
	{
		pRControls[typeIndex].gameObject.SetActive(value: false);
		typeIndex = smallIndex + bigIndex * 2;
		pRControls[typeIndex].gameObject.SetActive(value: true);
		LoadRanking();
	}

	private void ClearAllEvent()
	{
		OwnServerLogin instance = OwnServerLogin.Instance;
		if (instance != null)
		{
			instance.AbortGuestSignIn();
		}
		DispatcherMgr.Off("Change_Module_Event", ClearAllEvent);
		DispatcherMgr.Off<GuestLoginState>("Guest_Login", OnGuestLogin);
		RankingUtils.GetMonthIssueCallback = null;
		for (int i = 0; i < pRControls.Length; i++)
		{
			UnityEngine.Object.Destroy(pRControls[i].gameObject);
		}
	}

	private void OnGuestLogin(GuestLoginState state)
	{
		switch (state)
		{
		case GuestLoginState.Fail:
			if (RankMaskCtl.I != null)
			{
				RankMaskCtl.I.Hide();
			}
			if (networkTips != null)
			{
				networkTips.Show();
			}
			break;
		case GuestLoginState.Start:
			if (RankMaskCtl.I != null)
			{
				RankMaskCtl.I.Show(0f);
			}
			break;
		default:
			if (RankMaskCtl.I != null)
			{
				RankMaskCtl.I.SetMaskRaycast();
			}
			LoadRanking();
			break;
		}
	}
}
