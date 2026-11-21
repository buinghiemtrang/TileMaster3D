using System;
using System.Collections.Generic;
using DG.Tweening;
using ExtraFoundation.Components;
using GamePark.BrushstrokeEliminate.BaseData;
using GameScene;
using UnityEngine;
using UnityEngine.UI;

namespace GamePark.BrushstrokeEliminate
{
	public class ElementControl : MonoBehaviour
	{
		private int _raycastLayerMask;

		[SerializeField]
		private ElementBase cellElementBase;

		[SerializeField]
		private Transform tfElementLayer;

		[SerializeField]
		private Transform tfElementRange;

		[SerializeField]
		private Camera pCamera;

		[SerializeField]
		private LineRenderer3D lineRenderer;

		[SerializeField]
		private ElementTargetControl targetControl;

		[SerializeField]
		private Text ttLinkFailure;

		private readonly List<ElementBase> _selects;

		public ElementControl()
		{
			_selects = new List<ElementBase>();
		}

		private void OnDestroy()
		{
			this.DOKill(false);
			if (tfElementLayer != null)
			{
				ElementBase[] componentsInChildren = tfElementLayer.GetComponentsInChildren<ElementBase>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].transform.DOKill(false);
				}
			}
			DOTween.Kill("Brushstroke_LinkFailure_DeductTime", false);
		}

		private void Awake()
		{
			_raycastLayerMask = 1 << LayerMask.NameToLayer("Element");
			ElementMgr.I.Init(cellElementBase, tfElementLayer, tfElementRange);
		}

		public void InitDataOne()
		{
			targetControl.InitTarget();
			InitData();
		}

		private void InitData()
		{
			lineRenderer.Clear();
			_selects.Clear();
			ElementMgr.I.CreateElements();
		}

		public Vector3 CameraToUIPos(Vector3 pos3d)
		{
			Vector3 vector = pCamera.WorldToViewportPoint(pos3d);
			return Camera.main.ViewportToScreenPoint(vector);
		}

		public void OnOptMove(Vector3 touch, Vector3 difference)
		{
			if (DataMgr.I.kGameStatus == GameStatus.Playing)
			{
				SetSelectElement(touch);
			}
		}

		public void OnOptDown(Vector3 touch)
		{
			if (DataMgr.I.kGameStatus == GameStatus.Playing)
			{
				DataMgr.I.IsCanPause = false;
				SetSelectElement(touch);
			}
		}

		public void OnOptUp(Vector3 touch)
		{
			if (DataMgr.I.kGameStatus == GameStatus.Playing)
			{
				DataMgr.I.IsCanPause = true;
				SetSelectElement(touch);
				JudgeLinkTarget();
			}
		}

		private bool RaycastObj(Vector2 position, out GameObject hitObject)
		{
			Ray ray = pCamera.ScreenPointToRay(position);
			if (Physics.Raycast(ray, out RaycastHit hitInfo, 50f, _raycastLayerMask))
			{
				hitObject = hitInfo.collider.gameObject;
				return true;
			}
			hitObject = null;
			return false;
		}

		private void SetSelectElement(Vector2 position)
		{
			if (!RaycastObj(position, out GameObject hitObject))
			{
				return;
			}
			ElementBase element = ElementMgr.I.GetElement(hitObject.transform.localPosition);
			TargetData target = DataMgr.I.Level.GetTarget();
			if (target == null || element == null || target.ModelId != element.nModelId)
			{
				return;
			}
			ElementBase last = (_selects.Count == 0) ? null : _selects.Last();
			if (last == element)
			{
				return;
			}
			if (last != null)
			{
				int num = Math.Abs(element.loc.x - last.loc.x);
				int num2 = Math.Abs(element.loc.y - last.loc.y);
				if (num + num2 > 1)
				{
					return;
				}
				if (element.IsSelect)
				{
					last.SetSelect(false);
					_selects.RemoveAt(_selects.Count - 1);
					lineRenderer.DelLast();
					return;
				}
			}
			element.SetSelect(true);
			lineRenderer.Add(element.transform.localPosition);
			_selects.Add(element);
		}

		private void JudgeLinkTarget()
		{
			if (_selects.Count < 2)
			{
				foreach (ElementBase item in _selects)
				{
					item.SetSelect(false);
				}
				_selects.Clear();
				lineRenderer.Clear();
				return;
			}
			TargetData target = DataMgr.I.Level.GetTarget();
			if (_selects.Count >= target.Count)
			{
				DataMgr.I.ModifyScore(_selects.Count);
				Sequence sequence = DOTween.Sequence();
				sequence.SetTarget(this);
				for (int i = _selects.Count - 1; i >= 0; i--)
				{
					int index = i;
					sequence.AppendCallback(() =>
					{
						_selects[index].gameObject.SetActive(false);
						lineRenderer.DelLast();
						targetControl.ModifyTargetCount(-1);
						Vector3 uiPos = CameraToUIPos(_selects[index].transform.position);
						DataMgr.I.ModifyScoreShow(uiPos);
					});
					sequence.AppendInterval(0.1f);
				}
				sequence.OnComplete(CheckResult);
				return;
			}
			tfElementLayer.DOShakePosition(0.8f, Vector3.one * 0.1f, 20, 90f, false, true);
			AudioMgr.I.PlaySound("place_fail");
			Text text = UnityEngine.Object.Instantiate(ttLinkFailure, ttLinkFailure.transform.parent);
			text.gameObject.SetActive(true);
			text.text = string.Format("-{0}s", DataMgr.I.cfg.DeductTime);
			text.transform.SetScale(0f);
			Vector3 targetPos = text.transform.parent.InverseTransformPoint(DataMgr.I.pMain.pTime.transform.position);
			Sequence seq = DOTween.Sequence();
			seq.SetTarget(text.transform).SetId("Brushstroke_LinkFailure_DeductTime");
			seq.Append(text.transform.DOScale(Vector3.one * 1.1f, 0.2f));
			seq.Append(text.transform.DOScale(Vector3.one, 0.1f));
			seq.AppendInterval(0.2f);
			seq.Append(text.transform.DOLocalMove(targetPos, 0.5f, false));
			seq.OnComplete(() =>
			{
				DataMgr.I.pMain.pTime.DelTime(DataMgr.I.cfg.DeductTime, true);
				UnityEngine.Object.Destroy(text.gameObject);
			});
			foreach (ElementBase item in _selects)
			{
				item.SetSelect(false);
			}
			_selects.Clear();
			lineRenderer.Clear();
		}

		private void CheckResult()
		{
			ElementMgr.I.ClearElement();
			TweenMgr.DelayCall(0.3f, () =>
			{
				bool isWin = DataMgr.I.Level.NextLayer();
				targetControl.NextTarget();
				if (!isWin)
				{
					InitData();
				}
				else
				{
					DataMgr.I.pMain.GameVictory("PassGame");
				}
			});
		}
	}
}
