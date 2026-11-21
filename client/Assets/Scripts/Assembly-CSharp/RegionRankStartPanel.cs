using System;
using UnityEngine;

public class RegionRankStartPanel : MonoBehaviour
{
	[SerializeField]
	private MButton helpBtn;

	[SerializeField]
	private MButton btnStart;

	[SerializeField]
	private DialogShow dialog;

	private Action _endCall;

	private void Start()
	{
		helpBtn.onClick.AddListener(delegate
		{
			SceneMgr.AddTopNode<DialogShow>("Prefabs/RegionRankHelpPanel");
		});
		btnStart.onClick.AddListener(ClickStart);
		GSingleton<RegionRankActivityManager>.ins.SetStartPopTrue();
	}

	private void OnDestroy()
	{
		_endCall?.Invoke();
	}

	private void ClickStart(MButton button)
	{
		GSingleton<RegionRankActivityManager>.ins.SetActivityOpen();
		LogEvent.I.Peak_Match_Event_Open();
		RegionRankPanel.Create(_endCall, true);
		_endCall = null;
		dialog.ClickClose();
	}

	public static RegionRankStartPanel Create(Action endcall)
	{
		RegionRankStartPanel regionRankStartPanel = SceneMgr.AddTopNode<RegionRankStartPanel>("Prefabs/RegionRankStartPanel");
		regionRankStartPanel._endCall = (Action)Delegate.Combine(regionRankStartPanel._endCall, endcall);
		return regionRankStartPanel;
	}
}
