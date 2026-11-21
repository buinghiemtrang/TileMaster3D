using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class GMGridController : MonoBehaviour, UIGridViewController
{
	[CompilerGenerated]
	private sealed class _003COnItemShown_003Ed__6 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

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
		public _003COnItemShown_003Ed__6(int _003C_003E1__state)
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

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
		}
	}

	[SerializeField]
	private GameObject _prefab;

	[SerializeField]
	private UIGridView _gridView;

	[SerializeField]
	private ToggleGroup _toggleGroup;

	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	public GameObject GetItemPrefab()
	{
		return null;
	}

	[IteratorStateMachine(typeof(_003COnItemShown_003Ed__6))]
	public IEnumerator OnItemShown(GameObject item, int index)
	{
		return null;
	}

	public int GetItemCount()
	{
		return 0;
	}
}
