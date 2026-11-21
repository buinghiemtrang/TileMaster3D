using System;
using UnityEngine;

public class LevelUnlockProgress : MonoBehaviour
{
	public LevelUnlockProgressCom com;

	private Action _endCall;

	public static void Show(Action endCall)
	{
		LevelUnlockProgress levelUnlockProgress = SceneMgr.AddTopNode<LevelUnlockProgress>("Prefabs/LevelUnlockProgress");
		if (levelUnlockProgress != null)
		{
			levelUnlockProgress._endCall = endCall;
		}
	}

	private void Start()
	{
		com.setEndCall(showUnlock);
	}

	private void showUnlock()
	{
		UnityEngine.Object.Destroy(base.gameObject);
		UnlockNewItemsControl unlockNewItemsControl = SceneMgr.AddTopNode<UnlockNewItemsControl>("Prefabs/Unlocknewitems");
		if (unlockNewItemsControl != null)
		{
			unlockNewItemsControl.EndCall = _endCall;
		}
	}
}
