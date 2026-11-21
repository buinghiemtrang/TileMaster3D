using System.Collections.Generic;
using DG.Tweening;
using ExtraFoundation.Components;
using Language;
using UnityEngine;

namespace Game.Activity.BakingMaster
{
	public class BakingMasterBrandItem : MonoBehaviour
	{
		[SerializeField]
		private TextMarqueeControl ttName;

		public MButton button;

		private List<RequestMatchData> _datas;

		private string sTempName;

		public int LevelId { get; private set; }

		private void OnDestroy()
		{
			if (button != null)
			{
				button.transform.DOKill();
			}
		}

		public void Init(List<RequestMatchData> datas, int id)
		{
			_datas = datas;
			LevelId = id;
			LanguageData languageDataByKey = LanguageManager.Instance.GetLanguageDataByKey("Language29");
			string format = (languageDataByKey != null) ? languageDataByKey.value : "Level {0}";
			if (_datas.Count < 1)
			{
				sTempName = string.Format(format, id + 1);
			}
			else
			{
				sTempName = _datas[0].GetUserName();
			}
			SetName(sTempName);
		}

		private void SetName(string msg)
		{
			ttName.SetText(msg);
		}

		public void RefreshData()
		{
			if (_datas != null && _datas.Count > 0)
			{
				string userName = _datas[0].GetUserName();
				if (sTempName != userName)
				{
					button.transform.DOKill();
					DOTween.Sequence()
						.SetTarget(button.transform)
						.Append(button.transform.DOScaleX(0f, 0.2f))
						.AppendCallback(delegate
						{
							SetName(userName);
						})
						.Append(button.transform.DOScaleX(1f, 0.2f));
				}
				else
				{
					SetName(userName);
				}
				sTempName = userName;
			}
		}

		public bool JudgeEnter()
		{
			return true;
		}
	}
}
