using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameScene_Element;
using UnityEngine;
using _FrameWork;

namespace GameScene_Function
{
	public class DestroyControl : FunctionBase
	{
		public override void Init()
		{
		}

		public List<Element> JudgeCanUse(int number)
		{
			List<Element> list = new List<Element>();
			Dictionary<int, List<Element>> displayedElementsId = ElementsMgr.I.Data.DisplayedElementsId;
			List<int> list2 = displayedElementsId.Keys.ToList();
			while (number > 0 && list2.Count > 0)
			{
				int key = list2.PopR();
				if (!displayedElementsId.TryGetValue(key, out var value))
				{
					continue;
				}
				List<Element> list3 = new List<Element>();
				for (int i = 0; i < value.Count; i++)
				{
					Element element = value[i];
					if (element.Type != ElementType.Ordinary)
					{
						break;
					}
					if (!element.IsLocked && !element.IsSelected && !element.IsGuided)
					{
						list3.Add(element);
						if (list3.Count == 3)
						{
							list.AddRange(list3);
							number--;
							break;
						}
					}
				}
			}
			return list;
		}

		public void Use(List<Element> elements, Transform parentNode = null)
		{
			GameObject effect = LoadMgr.Load<GameObject>("Effect/Element_Destroy_Use_Effect");
			int number = elements.Count;
			for (int i = 0; i < elements.Count; i++)
			{
				Element element = elements[i];
				element.ModifyLockStatus(true);
				if (parentNode)
				{
					element.ModifyParent(parentNode, true);
				}
				element.Freeze((RigidbodyConstraints)126);
				Vector3 position = element.Self.position;
				position = new Vector3(position.x, 2f, position.z);
				element.ScaleToSize(1.2f, 0.3f).SetEase(Ease.Linear);
				element.MoveToPosition(position, 0.3f).SetEase(Ease.Linear).OnComplete(delegate
				{
					ParticleSystemInfo component = Object.Instantiate(effect).GetComponent<ParticleSystemInfo>();
					component.SetPosition(position);
					component.Play();
					component.AddDuration(0.3f, true);
					number--;
					if (number <= 0)
					{
						ElementsMgr.I.RemoveElements(elements, Vector3.zero);
					}
				});
			}
		}

		public void UseDeleteDirectly(List<Element> elements, Transform parentNode = null)
		{
			GameObject original = LoadMgr.Load<GameObject>("Effect/Element_Destroy_Use_Effect");
			for (int i = 0; i < elements.Count; i++)
			{
				Element element = elements[i];
				element.ModifyLockStatus(true);
				if (parentNode)
				{
					element.ModifyParent(parentNode, true);
				}
				element.Freeze((RigidbodyConstraints)126);
				Vector3 position = element.Self.position;
				ParticleSystemInfo component = Object.Instantiate(original).GetComponent<ParticleSystemInfo>();
				component.SetPosition(position);
				component.Play();
				component.AddDuration(0.3f, true);
			}
			ElementsMgr.I.RemoveElements(elements, Vector3.zero);
		}
	}
}
