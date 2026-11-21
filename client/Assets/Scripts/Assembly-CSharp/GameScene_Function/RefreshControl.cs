using System.Collections.Generic;
using DG.Tweening;
using GameScene;
using GameScene_Element;
using UnityEngine;

namespace GameScene_Function
{
	public class RefreshControl : FunctionBase
	{
		private bool _isCooling;

		private Tweener _delayTw;

		public override void Init()
		{
			_isCooling = false;
			if (_delayTw != null)
			{
				_delayTw.Kill();
			}
		}

		public bool JudgeCanUse()
		{
			if (_isCooling)
			{
				return false;
			}
			foreach (KeyValuePair<Transform, Element> item in ElementsMgr.I.Data.DisplayedElementsTf)
			{
				Element value = item.Value;
				if (!value.IsLocked && !value.IsSelected && !value.IsGuided)
				{
					return true;
				}
			}
			return false;
		}

		public void Use(bool isJudge = false)
		{
			if (isJudge && !JudgeCanUse())
			{
				return;
			}
			if (_delayTw != null)
			{
				_delayTw.Kill();
			}
			_delayTw = TweenMgr.DelayCall(cf_const.refreshCoolTime, delegate
			{
				_isCooling = false;
			});
			OperationTip.I.CancelHighlightTip();
			GameMgr.I.propControl.pPropRefresh.SetCoolDown(cf_const.refreshCoolTime);
			if (_isCooling)
			{
				_isCooling = true;
				return;
			}
			RefreshEffectControl.I.Play(1.5f);
			foreach (KeyValuePair<Transform, Element> item in ElementsMgr.I.Data.DisplayedElementsTf)
			{
				Element value = item.Value;
				if (!value.IsSelected && !value.IsLocked && !value.IsGuided)
				{
					value.AddForce(new Vector3(Random.Range(-0.8f, 0.8f) * 10f, 10f, 0f), ForceMode.Impulse);
				}
			}
			_isCooling = true;
		}

		public void Destroy()
		{
			if (_delayTw != null)
			{
				_delayTw.Kill();
			}
		}
	}
}
