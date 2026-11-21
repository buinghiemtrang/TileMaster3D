using System;
using DG.Tweening;
using GameScene;
using GameScene_Level;
using Spine.Unity;
using UnityEngine;

public class BlackCutDialog : MonoBehaviour
{
	private static BlackCutDialog _;

	[SerializeField]
	private Transform _root;

	[SerializeField]
	private SkeletonGraphic _anim;

	private void Awake()
	{
		_ = this;
		_anim.transform.localScale = new Vector3(SceneMgr.scale.max, SceneMgr.scale.max, _anim.transform.localScale.z);
	}

	private void Open(Action callback = null)
	{
		_anim.AnimationState.SetAnimation(0, "show", loop: false);
		_anim.AnimationState.AddAnimation(0, "idle", loop: true, 0f);
		_root.DOKill();
		_root.DelayCall(0.7f, delegate
		{
			callback?.Invoke();
		});
	}

	private void Close(Action callback = null)
	{
		_anim.AnimationState.SetAnimation(0, "out", loop: false);
		_root.DOKill();
		_root.DelayCall(0.7f, delegate
		{
			callback?.Invoke();
			UnityEngine.Object.Destroy(base.gameObject);
		});
	}

	private void OnDestroy()
	{
		_root.DOKill();
		_ = null;
	}

	private void JumpScene(Action callback = null, LevelType levelType = LevelType.Ordinary, int groupId = -1, int level = -1)
	{
		Open(delegate
		{
			SceneMgr.LoadScene3D("GameScene", null, callback).GetComponent<GameMgr>().NewLevel(levelType, groupId, level);
		});
	}

	public static void EnterGame(Action callback = null, LevelType levelType = LevelType.Ordinary, int groupId = -1, int level = -1)
	{
		SceneMgr.AddSuperNode<BlackCutDialog>("Prefabs/BlackCutDialog").JumpScene(callback, levelType, groupId, level);
	}

	public static void Show(Action callback = null)
	{
		SceneMgr.AddSuperNode<BlackCutDialog>("Prefabs/BlackCutDialog").Open(callback);
	}

	public static void Hide(Action callback = null)
	{
		if (_ != null)
		{
			_.Close(callback);
		}
		else
		{
			callback?.Invoke();
		}
	}
}
