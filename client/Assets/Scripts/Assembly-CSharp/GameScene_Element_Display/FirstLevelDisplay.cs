using System;
using System.Collections.Generic;
using GameScene_Element;
using UnityEngine;

namespace GameScene_Element_Display
{
	public class FirstLevelDisplay : ElementDisplayMode
	{
		private int _index;

		private readonly Vector3[] _position = new Vector3[9]
		{
			new Vector3(-1.5f, 1f, 3f),
			new Vector3(0f, 1f, 3f),
			new Vector3(1.5f, 1f, 3f),
			new Vector3(0f, 1f, -1f),
			new Vector3(0f, 1f, 1f),
			new Vector3(1.5f, 1f, 1f),
			new Vector3(-1.5f, 1f, -1f),
			new Vector3(-1.5f, 1f, 1f),
			new Vector3(1.5f, 1f, -1f)
		};

		private readonly Vector3[] _angle = new Vector3[9]
		{
			new Vector3(-120f, 0f, 180f),
			new Vector3(-120f, 0f, 180f),
			new Vector3(-120f, 0f, 180f),
			new Vector3(80f, 0f, 0f),
			new Vector3(80f, 0f, 0f),
			new Vector3(80f, 0f, 0f),
			new Vector3(150f, 0f, 0f),
			new Vector3(150f, 0f, 0f),
			new Vector3(150f, 0f, 0f)
		};

		public void Display(List<Element> elements, Action<Element> attribute, Action completed)
		{
			int number = elements.Count;
			_index = 0;
			if (elements.Count == 0)
			{
				completed.InvokeSafe();
				return;
			}
			for (int i = 0; i < elements.Count; i++)
			{
				Element element = elements[i];
				element.Self.SetActive(true);
				attribute.InvokeSafe(element);
				element.FallDown(1f, delegate
				{
					number--;
					if (number <= 0)
					{
						completed.InvokeSafe();
					}
				});
			}
		}

		public void SetPositionAndAngle(Element element)
		{
			if (_index < _position.Length)
			{
				element.Self.localPosition = _position[_index];
				element.Self.localEulerAngles = _angle[_index];
				_index++;
			}
			else
			{
				element.Self.localPosition = Vector3.up;
				element.Self.localRotation = Quaternion.Euler(Vector3.zero);
			}
		}
	}
}
