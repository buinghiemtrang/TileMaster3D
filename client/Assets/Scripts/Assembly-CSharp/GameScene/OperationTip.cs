using System.Collections.Generic;
using GameScene_Element;
using GameScene_Level;
using UnityEngine;

namespace GameScene
{
	public class OperationTip : MonoBehaviour
	{
		public static OperationTip I;

		public bool isHighlightTip;

		private int _level;

		private float _highlightTipTime;

		private float _hintTipTime;

		private List<Element> _highlightElements;

		private bool _isHintTip;

		private bool _isBackTip;

		private bool _isFreezeTip;

		private int _freezeTipNumber;

		private void Awake()
		{
			I = this;
			_highlightElements = new List<Element>();
		}

		public void Init()
		{
			_level = LevelMgr.Data.Lv;
			CancelHintTip();
			CancelHighlightTip();
			_CancelBackTip();
			_freezeTipNumber = 1;
			CancelFreezeTip();
		}

		private void Update()
		{
			if (GameMgr.I.GameStatus == GameStatus.Playing)
			{
				_HintTip();
				if (LevelMgr.Data.Type == LevelType.Ordinary)
				{
					_HighlightTip();
				}
			}
		}

		public void BackTip(int number, bool isDirectCancel = false)
		{
			if (_level < cf_const.unlockBackLv)
			{
				return;
			}
			UpdatePropExpend pPropBack = GameMgr.I.propControl.pPropBack;
			if (number == 6 && !isDirectCancel)
			{
				if (_isBackTip)
				{
					return;
				}
				_isBackTip = true;
				pPropBack.Tip(isOpen: true);
				if (DB.User.BShowBackGuide)
				{
					return;
				}
				DB.User.BShowBackGuide = true;
				pPropBack.isFree = true;
			}
			else
			{
				if (!_isBackTip)
				{
					return;
				}
				_isBackTip = false;
				pPropBack.Tip(isOpen: false);
				if (!pPropBack.isFree)
				{
					return;
				}
				pPropBack.isFree = false;
			}
			pPropBack.Refresh();
		}

		private void _CancelBackTip()
		{
			if (_isBackTip)
			{
				_isBackTip = false;
				UpdatePropExpend pPropBack = GameMgr.I.propControl.pPropBack;
				pPropBack.Tip(isOpen: false);
				if (pPropBack.isFree)
				{
					pPropBack.isFree = false;
					pPropBack.Refresh();
				}
			}
		}

		public bool JudgeFreezeTip()
		{
			if (_level < cf_const.unlockFreezeLv || _isFreezeTip)
			{
				return false;
			}
			return _freezeTipNumber != 0;
		}

		public void FreezeTip(float time)
		{
			if (!(time > 60f))
			{
				_isFreezeTip = true;
				_freezeTipNumber--;
				GameMgr.I.propControl.pPropFreeze.Tip(isOpen: true);
			}
		}

		public void CancelFreezeTip()
		{
			if (_isFreezeTip)
			{
				_isFreezeTip = false;
				GameMgr.I.propControl.pPropFreeze.Tip(isOpen: false);
			}
		}

		private void _HighlightTip()
		{
			if (isHighlightTip || cf_const.highlightTipMinLv > _level || _level > cf_const.highlightTipMaxLv)
			{
				return;
			}
			_highlightTipTime += Time.deltaTime;
			if (!(_highlightTipTime >= cf_const.highlightTipInterval))
			{
				return;
			}
			_highlightTipTime = 0f;
			_highlightElements.AddRange(GameMgr.I.RandomHintControl.GetHintElements());
			isHighlightTip = _highlightElements.Count > 0;
			if (isHighlightTip)
			{
				for (int i = 0; i < _highlightElements.Count; i++)
				{
					_highlightElements[i].Highlight(isLight: true);
				}
			}
		}

		public void CancelHighlightTip()
		{
			_highlightTipTime = 0f;
			if (isHighlightTip)
			{
				isHighlightTip = false;
				for (int i = 0; i < _highlightElements.Count; i++)
				{
					Element element = _highlightElements[i];
					if (!element.IsLocked && !element.IsSelected)
					{
						_highlightElements[i].Highlight(isLight: false);
					}
				}
				_highlightElements.Clear();
			}
		}

		private void _HintTip()
		{
			if (!_isHintTip && _level >= cf_const.unlockHintLv)
			{
				_hintTipTime += Time.deltaTime;
				if (_hintTipTime >= cf_const.hintTipInterval)
				{
					_hintTipTime = 0f;
					_isHintTip = GameMgr.I.RandomHintControl.JudgeCanUse();
					if (_isHintTip)
					{
						GameMgr.I.propControl.pPropHint.Tip(isOpen: true);
					}
				}
			}
		}

		public void CancelHintTip()
		{
			_hintTipTime = 0f;
			if (_isHintTip)
			{
				_isHintTip = false;
				GameMgr.I.propControl.pPropHint.Tip(isOpen: false);
			}
		}
	}
}
