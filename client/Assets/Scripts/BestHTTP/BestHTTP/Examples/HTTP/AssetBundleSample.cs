using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BestHTTP.Examples.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples.HTTP
{
	public sealed class AssetBundleSample : SampleBase
	{
		[CompilerGenerated]
		private sealed class _003CDownloadAssetBundle_003Ed__10 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public AssetBundleSample _003C_003E4__this;

			private AssetBundleCreateRequest _003Casync_003E5__2;

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
			public _003CDownloadAssetBundle_003Ed__10(int _003C_003E1__state)
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
		private sealed class _003CProcessAssetBundle_003Ed__11 : IEnumerator<object>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private object _003C_003E2__current;

			public AssetBundle bundle;

			public AssetBundleSample _003C_003E4__this;

			private AssetBundleRequest _003CasyncAsset_003E5__2;

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
			public _003CProcessAssetBundle_003Ed__11(int _003C_003E1__state)
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

		[Tooltip("The url of the resource to download")]
		[SerializeField]
		private string _path;

		[SerializeField]
		private string _assetnameInBundle;

		[SerializeField]
		private Text _statusText;

		[SerializeField]
		private RawImage _rawImage;

		[SerializeField]
		private Button _downloadButton;

		private HTTPRequest request;

		private AssetBundle cachedBundle;

		protected override void Start()
		{
		}

		private void OnDestroy()
		{
		}

		public void OnStartDownloadButton()
		{
		}

		[IteratorStateMachine(typeof(_003CDownloadAssetBundle_003Ed__10))]
		private IEnumerator DownloadAssetBundle()
		{
			return null;
		}

		[IteratorStateMachine(typeof(_003CProcessAssetBundle_003Ed__11))]
		private IEnumerator ProcessAssetBundle(AssetBundle bundle)
		{
			return null;
		}

		private void UnloadBundle()
		{
		}
	}
}
