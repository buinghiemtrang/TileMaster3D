using System;
using UnityEngine;
using UnityEngine.UI;

public class RegionRankPreparePanel : MonoBehaviour
{
	[SerializeField]
	private MButton helpBtn;

	[SerializeField]
	private MButton btnStart;

	[SerializeField]
	private DialogShow dialog;

	[SerializeField]
	private Text desc;

	private void Awake()
	{
		helpBtn.onClick.AddListener(delegate
		{
			SceneMgr.AddTopNode<DialogShow>("Prefabs/RegionRankHelpPanel");
		});
		GSingleton<RegionRankActivityManager>.ins.SetPreparePopTrue();
		btnStart.onClick.AddListener(ClickStart);
		desc.SetLKey("TM3DLanguage24", cf_regionRank_const.UnlockLevel);
	}

	private void OnDestroy()
	{
	}

	private void ClickStart(MButton button)
	{
		dialog.ClickClose();
	}

	public static RegionRankPreparePanel Create(Action endcall)
	{
		RegionRankPreparePanel regionRankPreparePanel = SceneMgr.AddTopNode<RegionRankPreparePanel>("Prefabs/RegionRankPreparePanel");
		regionRankPreparePanel.dialog.func_callback_close = (Action)Delegate.Combine(regionRankPreparePanel.dialog.func_callback_close, endcall);
		return regionRankPreparePanel;
	}
}
