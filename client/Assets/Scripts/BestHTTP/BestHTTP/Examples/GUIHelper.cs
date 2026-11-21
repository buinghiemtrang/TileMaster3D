using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BestHTTP.Examples.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples
{
	public static class GUIHelper
	{
		[CompilerGenerated]
		private sealed class _003CScrollToBottom_003Ed__4 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public ScrollRect scrollRect;

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
			public _003CScrollToBottom_003Ed__4(int _003C_003E1__state)
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

		private static string[] prefixes;

		public static string GetBytesStr(double bytes, byte precision)
		{
			return null;
		}

		public static void RemoveChildren(RectTransform transform, int maxChildCount)
		{
		}

		public static TextListItem AddText(TextListItem prefab, RectTransform contentRoot, string text, int maxEntries, ScrollRect scrollRect)
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CScrollToBottom_003Ed__4))]
		public static IEnumerator ScrollToBottom(ScrollRect scrollRect)
		{
			return null;
		}
	}
}
