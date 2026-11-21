using System;
using DG.Tweening;
using UnityEngine;

public class LeftRightMoveAdsCtl : MonoBehaviour
{
	[SerializeField]
	private Lattice[] lattices;

	[SerializeField]
	private Transform arrowTra;

	[SerializeField]
	private float leftBoundary;

	[SerializeField]
	private float rightBoundary;

	public Action<int> cback;

	private Sequence sequence;

	private void Start()
	{
		_Multiple();
		_Move();
	}

	private void OnDestroy()
	{
		SetStop();
	}

	public void SetStop()
	{
		if (sequence != null)
		{
			sequence.Kill();
			sequence = null;
		}
	}

	private void _Multiple()
	{
		float x = arrowTra.localPosition.x;
		if (lattices != null)
		{
			for (int i = 0; i < lattices.Length; i++)
			{
				if (lattices[i].leftBoundary <= x && x <= lattices[i].rightBoundary)
				{
					cback?.Invoke(lattices[i].multiple);
					break;
				}
			}
		}
	}

	private void _Move()
	{
		arrowTra.SetLocPosX(lattices[0].leftBoundary);
		sequence = DOTween.Sequence();
		sequence.Append(arrowTra.DOLocalMoveX(lattices[lattices.Length - 1].rightBoundary, 1.5f).SetEase(Ease.InOutQuad));
		sequence.Append(arrowTra.DOLocalMoveX(lattices[0].leftBoundary, 1.5f).SetEase(Ease.InOutQuad));
		sequence.SetLoops(-1);
		sequence.OnUpdate(_Multiple);
	}
}
