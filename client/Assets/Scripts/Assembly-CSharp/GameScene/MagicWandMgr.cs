using System;
using System.Collections.Generic;
using GameScene_Element;
using _FrameWork;
using UnityEngine;

namespace GameScene
{
	public class MagicWandMgr : MonoBehaviour
	{
		public static MagicWandMgr I;

		[SerializeField]
		private MagicWandAnim anim;

		private List<Element> _elements;

		private void Awake()
		{
			I = this;
			_elements = new List<Element>();
		}

		public void Init()
		{
			_elements.Clear();
		}

		public void Execute(Action completed)
		{
			if (completed == null)
			{
				return;
			}
			_elements.AddRange(GameMgr.I.DestroyControl.JudgeCanUse(BoosterMgr.I.GetValue()));
			if (_elements.Count == 0)
			{
				completed.InvokeSafe();
				return; // khớp pseudo-C: gọi xong thì return (bản dịch cũ thiếu, completed bị gọi 2 lần)
			}
			anim.CloseCallback = completed;
			anim.ClearUpCallback = _Magic;
			// thân MagicWandAnim.Play() được inline vào hàm
			anim.Play();
		}

		private void _Magic()
		{
			List<Element> elements = _elements;
			GameObject original = LoadMgr.Load<GameObject>("Effect/Element_Destroy_Use_Effect");
			// pseudo-C có một nhánh ModifyParent(null, true) bị bọc bởi điều kiện luôn false (op_Implicit(null)); bỏ qua (mã chết)
			for (int i = 0; i < elements.Count; i++)
			{
				Element element = elements[i];
				element.ModifyLockStatus(true);
				element.Freeze(RigidbodyConstraints.FreezeAll);
				Vector3 position = element.Self.position;
				ParticleSystemInfo component = UnityEngine.Object.Instantiate(original).GetComponent<ParticleSystemInfo>();
				component.SetPosition(position);
				component.Play();
				component.AddDuration(0.3f, isDestroy: true);
			}
			ElementsMgr.I.RemoveElements(elements, Vector3.zero);
		}
	}
}
