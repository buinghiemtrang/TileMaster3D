using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SceneMainBottom : MonoBehaviour
{
	public static SceneMainBottom instance;

	public SceneMainTop mainTop;

	public Transform tfContent;

	public Image imgArrow;

	public Image imgBG;

	public MButton btnShop;

	public MButton homeBtn;

	public MButton teamBtn;

	public MButton btnGetEnergy;

	[SerializeField]
	private GameObject achiveRedObj;

	[SerializeField]
	private GameObject teamRedObj;

	private Transform tfLayer;

	private MButton curBtn;

	private bool isCanClick = true;

	private void Awake()
	{
		instance = this;
		DispatcherMgr.On("ActiveHomeBtn", delegate
		{
			homeBtn.ClickSelf();
		});
		DispatcherMgr.On("CHECK_NEW_TEAMBOARD", new Action<bool>(_teamRed));
	}

	private void Start()
	{
		TweenMgr.DelayCall(0.1f, delegate
		{
			ClickBtn(homeBtn);
		});
	}

	public void ClickBtn(MButton button)
	{
		if (button == curBtn)
		{
			return;
		}
		DispatcherMgr.Emit("Change_Module_Event");
		_execute(button);
		string type;
		switch (button.nTag)
		{
		case 1:
			type = "Shop";
			break;
		case 2:
			type = "Life";
			break;
		case 3:
			type = "Home";
			break;
		case 4:
			type = "Team";
			break;
		case 5:
			type = "Rank";
			break;
		default:
			return;
		}
		LogEvent.I.HomeButtonClick(type);
	}

	private void _execute(MButton button)
	{
		Transform transform = LoadMgr.LoadRes<Transform>(button.sMsg);
		transform.SetParent(tfContent, worldPositionStays: false);
		imgArrow.transform.DOLocalMoveX(button.transform.localPosition.x, 0.5f);
		SetButton(button, b_active: true);
		if (tfLayer == null)
		{
			tfLayer = transform;
		}
		DispatcherMgr.Emit("LoadMainContent");
		if (button.nTag == 4)
		{
			_teamRed(bShow: false);
		}
		RedDotHttpMgr.I.CheckChangeState(button.nTag);
		Action endCallback = delegate
		{
			DispatcherMgr.Emit("Change_Module_End_Event", button.nTag);
		};
		if ((bool)curBtn)
		{
			SetButton(curBtn, b_active: false);
			AniMoveFade(tfLayer, transform, button.nTag - curBtn.nTag < 0, endCallback);
		}
		curBtn = button;
		tfLayer = transform;
	}

	private void AniMoveFade(Transform l1, Transform l2, bool b_right, Action endCallback)
	{
		float width = tfContent.Rect().width;
		l1.DOKill(complete: true);
		l2.DOKill(complete: true);
		if (b_right)
		{
			l1.DOLocalMoveX(width, 0.3f).OnComplete(l1.DestroySelf);
			l2.SetLocPosX(0f - width);
			l2.DOLocalMoveX(0f, 0.3f).OnComplete(delegate
			{
				endCallback?.Invoke();
			});
		}
		else
		{
			l1.DOLocalMoveX(0f - width, 0.3f).OnComplete(l1.DestroySelf);
			l2.SetLocPosX(width);
			l2.DOLocalMoveX(0f, 0.3f).OnComplete(delegate
			{
				endCallback?.Invoke();
			});
		}
	}

	private void SetButton(MButton button, bool b_active)
	{
		button.interactable = !b_active;
		Transform transform = button.Find("Pos");
		Text text = button.FindC<Text>("Name");
		Transform transform2 = button.Find("Icon");
		transform2.DOKill();
		text.DOKill();
		float endValue;
		float endValue2;
		if (b_active)
		{
			transform2.gameObject.SetActive(value: true);
			endValue = transform.localPosition.y;
			endValue2 = 1f;
		}
		else
		{
			transform2.gameObject.SetActive(value: true);
			endValue = 0f;
			endValue2 = 0f;
		}
		transform2.DOLocalMoveY(endValue, 0.3f);
		text.DOFade(endValue2, 0.3f);
	}

	public void ClickBtnTeam(MButton btn)
	{
		_teamRed(bShow: false);
		teamBtn.ClickSelf();
	}

	public void ClickBtnShop(MButton button)
	{
		btnShop.ClickSelf();
	}

	public void ClickBtnSetting(MButton btn)
	{
		SceneMgr.AddTopNode<Transform>("Prefabs/SettingsDialog");
	}

	public void ClickBtnGetEnergy(MButton btn)
	{
		btnGetEnergy.ClickSelf();
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off("ActiveHomeBtn", delegate
		{
			homeBtn.ClickSelf();
		});
		DispatcherMgr.Off("CHECK_NEW_TEAMBOARD", new Action<bool>(_teamRed));
		instance = null;
	}

	private void _teamRed(bool bShow)
	{
		if (teamRedObj != null)
		{
			teamRedObj.SetActive(bShow);
		}
	}
}
