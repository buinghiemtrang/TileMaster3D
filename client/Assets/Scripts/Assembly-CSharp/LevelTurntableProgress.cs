using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class LevelTurntableProgress : MonoBehaviour
{
	[SerializeField]
	private SkeletonGraphic anim;

	[SerializeField]
	private Text progressText;

	[SerializeField]
	private Image progressImage;

	private int _maxLevelNum;

	private int _curLevelNum;

	private string _rewardInfo;

	private Tweener _tweener;

	private LevelProgress _levelTurntable;

	private bool _isComplete;

	private void Start()
	{
		SetChestData();
		ShowLevelGift();
	}

	private void SetChestData()
	{
		_maxLevelNum = _levelTurntable.maxCount;
		_curLevelNum = _levelTurntable.currentCount;
	}

	private void ShowLevelGift()
	{
		int num = Mathf.Clamp(_curLevelNum - 1, 0, _maxLevelNum);
		_isComplete = _maxLevelNum <= _curLevelNum;
		progressText.text = string.Format("{0}/{1}", num, _maxLevelNum);
		progressImage.fillAmount = (float)num / (float)_maxLevelNum;
		_tweener = TweenMgr.NumberTo(1f, num, _curLevelNum, delegate(float var)
		{
			progressText.text = string.Format("{0}/{1}", (int)var, _maxLevelNum);
			progressImage.fillAmount = var / (float)_maxLevelNum;
		});
		PlayIconAnimation(false);
	}

	private void PlayIconAnimation(bool isComplete)
	{
		if (anim != null)
		{
		}
	}

	private void OnDestroy()
	{
		if (_tweener != null)
		{
			_tweener.Kill(false);
		}
	}

	public void ClickBtnGet(MButton button)
	{
		if (_isComplete)
		{
			Singleton<TurntableManager>.I.ShowTurntable(null);
		}
	}
}
