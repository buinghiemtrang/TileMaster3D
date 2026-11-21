using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BestHTTP.Examples.Helpers;
using BestHTTP.SignalRCore;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples
{
	public sealed class UploadHubSample : SampleBase
	{
		[CompilerGenerated]
		private sealed class _003CPersonEcho_003Ed__19 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public UploadHubSample _003C_003E4__this;

			private UpStreamItemController<Person> _003Ccontroller_003E5__2;

			private int _003Ci_003E5__3;

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
			public _003CPersonEcho_003Ed__19(int _003C_003E1__state)
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

		[CompilerGenerated]
		private sealed class _003CScoreTracker_003Ed__16 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public UploadHubSample _003C_003E4__this;

			private UpStreamItemController<string> _003Ccontroller_003E5__2;

			private int _003Ci_003E5__3;

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
			public _003CScoreTracker_003Ed__16(int _003C_003E1__state)
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

		[CompilerGenerated]
		private sealed class _003CScoreTrackerWithParameterChannels_003Ed__17 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public UploadHubSample _003C_003E4__this;

			private UpStreamItemController<string> _003Ccontroller_003E5__2;

			private UploadChannel<string, int> _003Cplayer1param_003E5__3;

			private int _003Ci_003E5__4;

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
			public _003CScoreTrackerWithParameterChannels_003Ed__17(int _003C_003E1__state)
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

			private void _003C_003Em__Finally2()
			{
			}

			private void _003C_003Em__Finally3()
			{
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
			}
		}

		[CompilerGenerated]
		private sealed class _003CStreamEcho_003Ed__18 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public UploadHubSample _003C_003E4__this;

			private UpStreamItemController<string> _003Ccontroller_003E5__2;

			private int _003Ci_003E5__3;

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
			public _003CStreamEcho_003Ed__18(int _003C_003E1__state)
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

		[CompilerGenerated]
		private sealed class _003CUploadWord_003Ed__15 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public UploadHubSample _003C_003E4__this;

			private UpStreamItemController<string> _003Ccontroller_003E5__2;

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
			public _003CUploadWord_003Ed__15(int _003C_003E1__state)
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
		private string _path;

		[SerializeField]
		private ScrollRect _scrollRect;

		[SerializeField]
		private RectTransform _contentRoot;

		[SerializeField]
		private TextListItem _listItemPrefab;

		[SerializeField]
		private int _maxListItemEntries;

		[SerializeField]
		private Button _connectButton;

		[SerializeField]
		private Button _closeButton;

		[SerializeField]
		private float _yieldWaitTime;

		private HubConnection hub;

		protected override void Start()
		{
		}

		private void OnDestroy()
		{
		}

		public void OnConnectButton()
		{
		}

		public void OnCloseButton()
		{
		}

		private void Hub_Redirected(HubConnection hub, Uri oldUri, Uri newUri)
		{
		}

		private void Hub_OnConnected(HubConnection hub)
		{
		}

		[IteratorStateMachine(typeof(_003CUploadWord_003Ed__15))]
		private IEnumerator UploadWord()
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CScoreTracker_003Ed__16))]
		private IEnumerator ScoreTracker()
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CScoreTrackerWithParameterChannels_003Ed__17))]
		private IEnumerator ScoreTrackerWithParameterChannels()
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CStreamEcho_003Ed__18))]
		private IEnumerator StreamEcho()
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CPersonEcho_003Ed__19))]
		private IEnumerator PersonEcho()
		{
			return null;
		}

		private void Hub_OnClosed(HubConnection hub)
		{
		}

		private void Hub_OnError(HubConnection hub, string error)
		{
		}

		private void SetButtons(bool connect, bool close)
		{
		}

		private TextListItem AddText(string text)
		{
			return null;
		}
	}
}
