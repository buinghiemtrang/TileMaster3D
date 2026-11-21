using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class LevelTurntableInMain : MonoBehaviour
{
	[SerializeField]
	private SkeletonGraphic anim;

	[SerializeField]
	private Text progressText;

	[SerializeField]
	private Image progressImage;

	private int _maxLevelNum;

	private int _curLevelNum;

	private LevelProgress _levelTurntable;

	private bool _isComplete;

	private bool _isInit;

	private void Start()
	{
		_maxLevelNum = _levelTurntable.maxCount;
		_curLevelNum = _levelTurntable.currentCount;
		ShowProgress();
		_isInit = true;
		DispatcherMgr.On("Popup_Closed", OnRefresh);
		DispatcherMgr.On("Update_Account", OnRefresh);
		DispatcherMgr.On("OnTurntableOpen", OnLevelTurntableOpenEvent);
	}

	private void OnEnable()
	{
		if (_isInit)
		{
			SetChestData();
			ShowProgress();
		}
	}

	private void OnRefresh()
	{
		SetChestData();
		ShowProgress();
	}

	private void OnLevelTurntableOpenEvent()
	{
		SetChestData();
		ShowProgress();
	}

	private void SetChestData()
	{
		_maxLevelNum = _levelTurntable.maxCount;
		_curLevelNum = _levelTurntable.currentCount;
	}

	private void ShowProgress()
	{
		_isComplete = _maxLevelNum <= _curLevelNum;
		progressText.text = string.Format("{0}/{1}", _curLevelNum, _maxLevelNum);
		progressImage.fillAmount = (float)_curLevelNum / (float)_maxLevelNum;
		if (anim != null)
		{
			anim.AnimationState.SetAnimation(0, _isComplete ? "close-idle" : "idle", loop: true);
		}
	}

	public void ClickBtnGet(MButton button)
	{
		Singleton<TurntableManager>.I.ShowTurntable(null);
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off("Popup_Closed", OnRefresh);
		DispatcherMgr.Off("Update_Account", OnRefresh);
		DispatcherMgr.Off("OnTurntableOpen", OnLevelTurntableOpenEvent);
	}
}
