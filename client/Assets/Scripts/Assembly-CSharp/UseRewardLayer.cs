using System;
using DG.Tweening;
using GameScene_Level;
using UnityEngine;
using UnityEngine.UI;

public class UseRewardLayer : MonoBehaviour
{
	[SerializeField]
	private Text _currentLevel;

	[SerializeField]
	private Text _nextLevel;

	[SerializeField]
	private Image _image;

	[SerializeField]
	private GameObject _object;

	[SerializeField]
	private ParticleSystem _particleSystem;

	[SerializeField]
	private Text _timeLeft;

	private bool _open;

	private void DoAnimation()
	{
		Debug.Log(string.Format("UserLevel:   {0}/{1}", DB.UserLevelReward.UserScore, Singleton<UserLevelRewardManager>.I.NextLevelInfo.NeedScore));
		_image.DOKill();
		_image.DOFillAmount((float)DB.UserLevelReward.UserScore / (float)Singleton<UserLevelRewardManager>.I.NextLevelInfo.NeedScore, 0.2f);
	}

	public void InitView()
	{
		_open = cfgUserLevel_const.FunctionStage == 1 && LevelMgr.Data.Type == LevelType.Ordinary;
		_object.SetActive(_open);
		if (_open)
		{
			DispatcherMgr.On("AddUseLevelScore", new Action(DoAnimation));
			DispatcherMgr.On("UseLevelUp", new Action<Action>(LevelUp));
			if (_open)
			{
				UpdateView();
			}
		}
	}

	private void OnDestroy()
	{
		_timeLeft.DOKill();
		_image.DOKill();
		if (_open)
		{
			DispatcherMgr.Off("AddUseLevelScore", new Action(DoAnimation));
			DispatcherMgr.Off("UseLevelUp", new Action<Action>(LevelUp));
		}
	}

	private void LevelUp(Action action)
	{
		_particleSystem.Play();
		GTimer.ins.addDelay(delegate
		{
			UpdateView();
			SceneMgr.AddTopNode<UseLevelRewardDialog>("UseLevelRewardDialog").SetInfo(Singleton<UserLevelRewardManager>.I.RewardInfo).AddCloseCallback(action)
				.Show();
		}, 0.8f);
	}

	private void UpdateView()
	{
		_timeLeft.DOKill();
		_image.DOKill();
		_currentLevel.text = DB.UserLevelReward.UserLevel.ToString();
		_nextLevel.text = (DB.UserLevelReward.UserLevel + 1).ToString();
		_image.fillAmount = (float)DB.UserLevelReward.UserScore / (float)Singleton<UserLevelRewardManager>.I.NextLevelInfo.NeedScore;
	}
}
