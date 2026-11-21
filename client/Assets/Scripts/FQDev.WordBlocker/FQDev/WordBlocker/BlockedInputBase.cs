using System;
using UnityEngine;
using UnityEngine.Events;

namespace FQDev.WordBlocker
{
	public abstract class BlockedInputBase : UIBase
	{
		[Serializable]
		public class CheckInputValidCallback : UnityEvent<bool>
		{
		}

		[SerializeField]
		protected CheckInputValidCallback _onValidation = new CheckInputValidCallback();

		protected virtual void CheckValid(string content)
		{
			bool flag = GetFilter().CheckWord(content);
			if (_onValidation != null)
			{
				_onValidation.Invoke(!flag);
			}
		}
	}
}
