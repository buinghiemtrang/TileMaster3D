using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameTeam
{
	public class KickTipsDialog : DialogBase
	{
		[SerializeField]
		private Text textTitle;

		[SerializeField]
		private Text textMsg;

		private Action callback;

		public void InitData(string userName, Action cback)
		{
			textMsg.SetLBy(userName);
			callback = cback;
		}

		public void OnClickYes(MButton btn)
		{
			_Close(() =>
			{
				Debug.Log("Kick");
				if (callback != null)
				{
					callback();
				}
			});
		}

		public void OnClickNo(MButton button)
		{
			_Close(null);
		}
	}
}
