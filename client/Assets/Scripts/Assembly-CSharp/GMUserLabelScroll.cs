using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class GMUserLabelScroll : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCreateUserLabelItems_003Ed__9 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public GMUserLabelScroll _003C_003E4__this;

		private IEnumerator<KeyValuePair<string, string>> _003C_003E7__wrap1;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return null;
			}
		}

		[DebuggerHidden]
		public _003CCreateUserLabelItems_003Ed__9(int _003C_003E1__state)
		{
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	[SerializeField]
	private GMUserLabelItem _itemPrefab;

	[SerializeField]
	private Button _allChangeButton;

	[SerializeField]
	private Transform _contentTf;

	[SerializeField]
	private Text _describeTex;

	[CompilerGenerated]
	private Action m__changeUserLabelAction;

	private event Action _changeUserLabelAction
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	private void Awake()
	{
	}

	private void Start()
	{
	}

	[IteratorStateMachine(typeof(_003CCreateUserLabelItems_003Ed__9))]
	private IEnumerator CreateUserLabelItems()
	{
		return null;
	}

	private void OnAllChangeButtonClick()
	{
	}

	private void SetUserLabel(string key, string value, bool isAllChange)
	{
	}

	private void UpdateUserLabel()
	{
	}
}
