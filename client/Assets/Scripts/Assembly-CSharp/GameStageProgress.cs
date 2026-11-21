using System.Collections.Generic;
using DG.Tweening;
using GameScene_Element;
using GameScene_Element_Data;
using GameScene_Level;
using UnityEngine;
using UnityEngine.UI;

public class GameStageProgress : MonoBehaviour
{
	[SerializeField]
	private Sprite[] stageProgressSprites;

	[SerializeField]
	private float[] stageProgressWidth;

	private int _stageResourceIndex;

	[SerializeField]
	private float stageProgressDefaultWidth;

	[SerializeField]
	private Transform stageProgressPrefab;

	[SerializeField]
	private Transform stageProgressRoot;

	[SerializeField]
	private Text _currenStageProgressText;

	private int _currenStageProgressIndex = -1;

	private Image _currenStageProgressImage;

	private readonly List<Transform> _allStageProgress = new List<Transform>();

	private readonly string _progressImageString = "ImageProgress";

	private readonly string _progressCompleteString = "ImageComplete";

	private readonly string _LevelTextString = "Lv";

	private Tweener _completeProgressTweener;

	private float _currenCompleteProgress;

	private bool _isRegenerate;

	public void ResetStageProgress()
	{
		LevelMgr.GetLevelStatus(out var _, out var isRegenerate);
		_isRegenerate = isRegenerate;
		int num = (isRegenerate ? 2 : 1);
		_currenStageProgressIndex = -1;
		_stageResourceIndex = num - 1;
		for (int i = 0; i < _allStageProgress.Count; i++)
		{
			Object.Destroy(_allStageProgress[i].gameObject);
		}
		_allStageProgress.Clear();
		do
		{
			Transform transform = Object.Instantiate(stageProgressPrefab, stageProgressRoot).transform;
			ResetProgressUI(transform);
			transform.gameObject.SetActive(value: true);
			_allStageProgress.Add(transform);
			num--;
		}
		while (num != 0);
		ResetCurrenProgressUI(0);
		UpdateProductProgress(doAnimation: false);
	}

	private void ResetCurrenProgressUI(int index)
	{
		RectTransform completeStageProgress;
		if (_currenStageProgressIndex != index && _currenStageProgressIndex > -1)
		{
			completeStageProgress = _allStageProgress[_currenStageProgressIndex].RT();
			if ((bool)completeStageProgress)
			{
				Debug.Log("ResetCurrenProgressUI  completeStageProgress");
				completeStageProgress.DOSizeDelta(new Vector2(stageProgressDefaultWidth, stageProgressDefaultWidth), 0.5f).OnComplete(delegate
				{
					completeStageProgress.Find(_progressImageString).gameObject.SetActive(value: false);
					completeStageProgress.Find(_progressCompleteString).gameObject.SetActive(value: true);
					completeStageProgress.Find(_LevelTextString).gameObject.SetActive(value: false);
				});
			}
		}
		RectTransform rectTransform = _allStageProgress[index].RT();
		if ((bool)rectTransform)
		{
			Debug.Log("ResetCurrenProgressUI  nowStageProgressRT");
			rectTransform.DOSizeDelta(new Vector2(stageProgressWidth[_stageResourceIndex], stageProgressDefaultWidth), 0.5f);
		}
		_currenStageProgressImage = _allStageProgress[index].Find(_progressImageString).GetComponent<Image>();
		_currenStageProgressImage.gameObject.SetActive(value: true);
		_currenStageProgressIndex = index;
		_allStageProgress[index].Find(_LevelTextString).gameObject.SetActive(value: true);
	}

	private void ResetProgressUI(Transform progressTf)
	{
		progressTf.SetWidth(stageProgressDefaultWidth);
		Image component = progressTf.Find(_progressImageString).GetComponent<Image>();
		if (component != null)
		{
			component.sprite = stageProgressSprites[_stageResourceIndex];
			component.fillAmount = 0f;
		}
		Text component2 = progressTf.Find(_LevelTextString).GetComponent<Text>();
		if (component2 != null)
		{
			component2.SetLBy(LevelMgr.Data.Lv);
			component2.gameObject.SetActive(value: false);
		}
	}

	public void OnLevelStagePass(int beforeStageIndex, int currentStageIndex)
	{
		if (_isRegenerate)
		{
			_currenStageProgressText.text = "";
			_ = _allStageProgress[beforeStageIndex];
			_currenStageProgressImage.DOFillAmount(1f, 0.5f).OnComplete(delegate
			{
			});
			ResetCurrenProgressUI(currentStageIndex);
			UpdateProductProgress(doAnimation: false);
		}
	}

	public void UpdateProductProgress(bool doAnimation)
	{
		ElementsData data = ElementsMgr.I.Data;
		if (data.DisplayedTotalNumber >= 1)
		{
			float num = (float)data.DisplayedCollectNumber / (float)data.DisplayedTotalNumber;
			_currenCompleteProgress = num;
			if (doAnimation)
			{
				if (_completeProgressTweener != null)
				{
					_completeProgressTweener.Kill();
				}
				_completeProgressTweener = _currenStageProgressImage.DOFillAmount(num, 0.5f);
			}
			else
			{
				_currenStageProgressImage.fillAmount = num;
			}
		}
	}
}
