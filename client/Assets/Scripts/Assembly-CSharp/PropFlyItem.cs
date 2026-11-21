using System;
using DG.Tweening;
using UnityEngine;

public class PropFlyItem : MonoBehaviour
{
	[SerializeField]
	private RewardItem _rewardItem;

	[SerializeField]
	private Transform[] _transforms;

	[SerializeField]
	private Transform[] _bezierPositions;

	private Transform _targetTransform;

	private Transform _bezierPosition;

	private Sequence _sequence;

	private void Awake()
	{
		DispatcherMgr.On<int, int, Action>("GetRewardByUserLevel", ShowAnimation);
	}

	public void Reset()
	{
		base.transform.position = Vector3.zero;
		_rewardItem.gameObject.transform.SetScale(1f);
		_rewardItem.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		if (_sequence != null)
		{
			_sequence.Kill();
		}
		DispatcherMgr.Off<int, int, Action>("GetRewardByUserLevel", ShowAnimation);
	}

	private void ShowAnimation(int id, int value, Action arg3)
	{
		_rewardItem.SetInfo(id, value);
		_targetTransform = GetTargetTransform(id);
		_rewardItem.gameObject.SetActive(true);
		if (_sequence != null)
		{
			_sequence.Kill();
		}
		_sequence = DOTween.Sequence();
		_sequence.Append(_rewardItem.transform.DOBezier(0.3f, base.transform.position, _bezierPosition.position, _targetTransform.position));
		_sequence.Insert(0f, _rewardItem.transform.DOScale(0.5f, 0.3f));
		_sequence.OnComplete(delegate
		{
			Reset();
			if (arg3 != null)
			{
				arg3();
			}
			int prop = DB.Bag.GetProp((PropID)id);
			int num = prop - value;
			int num2 = DB.Bag.GetProp((PropID)id);
			if (id == 2)
			{
				num2 = Mathf.Min(cf_const.maxEnergyUpperLimit, num2);
			}
			DispatcherMgr.Emit("Update" + (PropID)id, num, num2);
		});
	}

	private Transform GetTargetTransform(int id)
	{
		int num;
		switch (id)
		{
		case 10:
			num = 0;
			break;
		case 11:
			num = 2;
			break;
		case 14:
			num = 1;
			break;
		case 15:
			num = 3;
			break;
		default:
			num = 4;
			break;
		}
		_bezierPosition = _bezierPositions[num];
		return _transforms[num];
	}
}
