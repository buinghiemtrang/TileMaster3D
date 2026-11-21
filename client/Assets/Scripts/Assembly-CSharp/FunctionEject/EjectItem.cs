using UnityEngine;

namespace FunctionEject
{
	public abstract class EjectItem : MonoBehaviour
	{
		public FunctionType functionType;

		public abstract void Init();

		public abstract bool Eject();

		public virtual void ForceEject()
		{
		}

		protected void End(EjectType type)
		{
			if (EjectMgr.I != null)
			{
				EjectMgr.I.Next(type);
			}
		}
	}
}
