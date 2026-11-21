using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BestHTTP.Examples.Helpers;
using BestHTTP.SignalRCore;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples
{
	public class AsyncTestHubSample : SampleBase
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003COnCloseButton_003Ed__11 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncVoidMethodBuilder _003C_003Et__builder;

			public AsyncTestHubSample _003C_003E4__this;

			private TaskAwaiter<HubConnection> _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003COnConnectButton_003Ed__10 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncVoidMethodBuilder _003C_003Et__builder;

			public AsyncTestHubSample _003C_003E4__this;

			private TaskAwaiter<HubConnection> _003C_003Eu__1;

			private TaskAwaiter<object> _003C_003Eu__2;

			private TaskAwaiter<string> _003C_003Eu__3;

			private TaskAwaiter<int> _003C_003Eu__4;

			private TaskAwaiter<int?> _003C_003Eu__5;

			private TaskAwaiter<Person> _003C_003Eu__6;

			private TaskAwaiter<int[]> _003C_003Eu__7;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003COnDestroy_003Ed__9 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncVoidMethodBuilder _003C_003Et__builder;

			public AsyncTestHubSample _003C_003E4__this;

			private TaskAwaiter<HubConnection> _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
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

		private HubConnection hub;

		protected override void Start()
		{
		}

		[AsyncStateMachine(typeof(_003COnDestroy_003Ed__9))]
		private void OnDestroy()
		{
		}

		[AsyncStateMachine(typeof(_003COnConnectButton_003Ed__10))]
		public void OnConnectButton()
		{
		}

		[AsyncStateMachine(typeof(_003COnCloseButton_003Ed__11))]
		public void OnCloseButton()
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
