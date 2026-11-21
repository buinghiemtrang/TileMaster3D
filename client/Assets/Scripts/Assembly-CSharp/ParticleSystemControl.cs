using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ParticleSystemControl : MonoBehaviour
{
	public enum ParticleSystemOperateType
	{
		None = 0,
		Hide = 1,
		Destroy = 2
	}

	[CompilerGenerated]
	private sealed class _003COperateType_003Ed__6 : IEnumerator<object>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ParticleSystemControl _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003COperateType_003Ed__6(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
		}

		private bool MoveNext()
		{
			ParticleSystemControl particleSystemControl = _003C_003E4__this;
			switch (_003C_003E1__state)
			{
			case 0:
				_003C_003E1__state = -1;
				if (particleSystemControl._fDelay == 0f)
				{
					_003C_003E2__current = new WaitForSeconds(particleSystemControl.gameObject.GetParticleDuration());
					_003C_003E1__state = 1;
				}
				else
				{
					_003C_003E2__current = new WaitForSeconds(particleSystemControl._fDelay);
					_003C_003E1__state = 2;
				}
				return true;
			case 1:
			case 2:
				_003C_003E1__state = -1;
				if (particleSystemControl._funcComplete != null)
				{
					particleSystemControl._funcComplete.InvokeSafe();
				}
				if (particleSystemControl._kOperateType == ParticleSystemOperateType.Destroy)
				{
					UnityEngine.Object.Destroy(particleSystemControl.gameObject);
				}
				else if (particleSystemControl._kOperateType == ParticleSystemOperateType.Hide)
				{
					particleSystemControl.gameObject.SetActive(false);
				}
				return false;
			default:
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[SerializeField]
	private ParticleSystemOperateType _kOperateType;

	[SerializeField]
	private float _fDelay;

	private Action _funcComplete;

	private void Start()
	{
		if (_kOperateType != ParticleSystemOperateType.None)
		{
			StartCoroutine(OperateType());
		}
	}

	public void Play(Action callback = null)
	{
		_funcComplete = callback;
		base.gameObject.SetActive(true);
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Play();
		}
	}

	[IteratorStateMachine(typeof(_003COperateType_003Ed__6))]
	private IEnumerator OperateType()
	{
		return new _003COperateType_003Ed__6(0)
		{
			_003C_003E4__this = this
		};
	}
}
