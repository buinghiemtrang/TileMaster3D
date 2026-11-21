using System.Collections;
using Com.TheFallenGames.OSA.Core;

namespace Scroll
{
	public abstract class ScrollBase<T1, T2> : OSA<T1, T2> where T1 : BaseParams where T2 : BaseItemViewsHolder
	{
		private bool _isInit;

		protected override void Awake()
		{
			base.Awake();
			_isInit = false;
		}

		protected override void Start()
		{
			base.Start();
			_isInit = true;
		}

		protected void ExecuteRefresh()
		{
			StopCoroutine("IERefresh");
			StartCoroutine("IERefresh"); // UNVERIFIED: pseudo-C inlines the string-based coroutine call after StopCoroutine; Start vs Stop of the second call is inferred
		}

		protected abstract void Reset();

		private IEnumerator IERefresh()
		{
			while (!_isInit)
			{
				yield return null;
			}
			Reset();
		}
	}
}
