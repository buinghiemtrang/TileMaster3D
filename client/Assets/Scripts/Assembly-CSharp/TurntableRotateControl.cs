using System;
using UnityEngine;

public class TurntableRotateControl : MonoBehaviour
{
	private enum State
	{
		Standby = 0,
		Accelerate = 1,
		ReduceSpeed = 2
	}

	public int areaNumber = 6;

	public bool isCenter = true;

	[SerializeField]
	private Transform rotateNode;

	[SerializeField]
	private float accelerateTime = 1f;

	[SerializeField]
	private float maxSpeed = 3000f;

	[SerializeField]
	private float uniformSpeedTime = 1f;

	[SerializeField]
	private int reduceSpeedTurnsNumber = 20;

	[SerializeField]
	private float reduceSpeedK = 3f;

	[SerializeField]
	private float fixAngle;

	private Action<int> _enterAreaCallback;

	private Action<bool, int> _completedCallback;

	private State _curState;

	private float _curAngle;

	private float _endAngle;

	private float _duration;

	private int _areaId;

	private int _curAreaId;

	private bool _isAd;

	private void Awake()
	{
		_curState = State.Standby;
	}

	private void Update()
	{
		if (_curState == State.Standby)
		{
			return;
		}
		_duration += Time.deltaTime;
		if (_curState == State.ReduceSpeed)
		{
			float t = Mathf.Clamp01(Time.deltaTime * reduceSpeedK);
			_curAngle += (_endAngle - _curAngle) * t;
			rotateNode.localRotation = Quaternion.Euler(0f, 0f, _curAngle);
			_CalculationAreaId(_curAngle);
			if (Mathf.Abs(_curAngle - _endAngle) <= 1f)
			{
				_curState = State.Standby;
				_completedCallback?.Invoke(_isAd, _areaId);
			}
		}
		else
		{
			float t2 = Mathf.Clamp01(_duration / accelerateTime);
			float num = Time.deltaTime * maxSpeed * t2;
			_curAngle += num;
			_CalculationAreaId(_curAngle);
			rotateNode.Rotate(Vector3.back, num, Space.Self);
			if (_duration > uniformSpeedTime + accelerateTime)
			{
				_curAngle = -(360f - rotateNode.eulerAngles.z) % 360f;
				_curState = State.ReduceSpeed;
			}
		}
	}

	public void Rotate(int areaId)
	{
		_areaId = areaId;
		float num = 360f / (float)areaNumber;
		float num2 = num * 0.5f;
		float num3 = (isCenter ? num : num2);
		float num4 = num * (float)areaId - num3;
		_endAngle = num4;
		float num5 = UnityEngine.Random.Range(4f - num2, num2 - 4f);
		_endAngle = num5 + num4 - (float)(reduceSpeedTurnsNumber * 360) - fixAngle;
		Debug.LogError(string.Format(" _areaId :{0} , _endAngle:{1}", _areaId, _endAngle));
		_duration = 0f;
		_curState = State.Accelerate;
		_curAreaId = 0;
	}

	public void AddEnterAreaCallback(Action<int> callback)
	{
		_enterAreaCallback = callback;
	}

	public void AddCompletedCallback(bool isAd, Action<bool, int> callback)
	{
		_completedCallback = callback;
		_isAd = isAd;
	}

	private void _CalculationAreaId(float angle)
	{
		float num = 360f / (float)areaNumber;
		float num2 = 360f / (float)(areaNumber * 2);
		int num3 = (int)((Mathf.Abs(angle) + num2) / num);
		int num4 = num3 % areaNumber + 1;
		if (num4 != _curAreaId)
		{
			_enterAreaCallback?.Invoke(num4);
			_curAreaId = num4;
		}
	}
}
