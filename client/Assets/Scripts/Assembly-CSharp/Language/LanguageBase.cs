using UnityEngine;

namespace Language
{
	public class LanguageBase : MonoBehaviour
	{
		[HideInInspector]
		[SerializeField]
		public string language;

		[HideInInspector]
		[SerializeField]
		public string key;

		[HideInInspector]
		[SerializeField]
		public string value;

		public bool bAwake = true;

		private LanguageManager.EventFunc func_event;

		private void Awake()
		{
			if (bAwake)
			{
				OnSet();
				SetEventFunc(OnSetAction);
			}
		}

		private void OnDestroy()
		{
			RemoveFunc();
		}

		private void RemoveFunc()
		{
			if (func_event != null)
			{
				LanguageManager.Instance.RemoveEventHandler(func_event);
			}
		}

		public void SetEventFunc(LanguageManager.EventFunc func)
		{
			RemoveFunc();
			func_event = func;
			LanguageManager.Instance.AddEventHandler(func_event);
		}

		public void ExecuteEventFunc()
		{
			func_event?.Invoke();
		}

		protected virtual void OnSet()
		{
		}

		public void OnSetAction()
		{
			OnSet();
		}

		public void SetContents(string theLanguageContentKey)
		{
			key = theLanguageContentKey;
			OnSet();
		}
	}
}
