using System;
using DG.Tweening;
using UnityEngine;

public class FlyItem : MonoBehaviour
{
	[HideInInspector]
	public bool isFree;

	private Transform self;

	private Tween moveTw;

	private Tween scaleTw;

	private Vector3 endScale = new Vector3(0.8f, 0.8f, 0.8f);

	public void Fly2D(Transform pNode, Vector3 sPos, Vector2 ePos, Action cback)
	{
		isFree = false;
		if (!self)
		{
			self = base.transform;
		}
		_hide(isHide: false);
		self.localPosition = sPos;
		self.SetParent(pNode, worldPositionStays: false);
		self.SetAsLastSibling();
		Vector3 p = new Vector3(sPos.x, sPos.y - 100f, 0f);
		Vector3 p2 = new Vector3((sPos.x + ePos.x) * 0.5f, sPos.y - 100f, 0f);
		Vector3 p3 = new Vector3(ePos.x, ePos.y, 0f);
		moveTw = self.DOLocalBezier(0.6f, sPos, p, p2, p3).OnComplete(delegate
		{
			isFree = true;
			_hide(isHide: true);
			cback?.Invoke();
		});
	}

	public void Fly(Transform pNode, Vector3 sPos, Vector3 ePos, Action cback)
	{
		isFree = false;
		if (!self)
		{
			self = base.transform;
		}
		self.position = sPos;
		self.SetParent(pNode, worldPositionStays: false);
		_hide(isHide: false);
		Vector3 p = new Vector3(sPos.x, sPos.y, sPos.z - 1f);
		Vector3 p2 = new Vector3(ePos.x, sPos.y, sPos.z - 1f);
		moveTw = self.DOBezier(0.6f, sPos, p, p2, ePos).OnComplete(delegate
		{
			isFree = true;
			_hide(isHide: true);
			cback?.Invoke();
		});
		scaleTw = self.DOScale(Vector3.one * 0.4f, 0.6f);
	}

	private void _hide(bool isHide)
	{
		base.gameObject.SetActive(!isHide);
	}

	private void OnDestroy()
	{
		if (moveTw != null)
		{
			moveTw.Kill();
			moveTw = null;
		}
		if (scaleTw != null)
		{
			scaleTw.Kill();
			scaleTw = null;
		}
	}
}
