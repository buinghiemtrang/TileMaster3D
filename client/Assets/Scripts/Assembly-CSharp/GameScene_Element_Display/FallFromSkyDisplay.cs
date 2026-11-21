using System;
using System.Collections.Generic;
using GameScene;
using GameScene_Element;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameScene_Element_Display
{
	public class FallFromSkyDisplay : ElementDisplayMode
	{
		public void Display(List<Element> elements, Action<Element> attribute, Action completed)
		{
			if (elements.Count == 0)
			{
				completed.InvokeSafe();
				return;
			}
			int number = elements.Count;
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

		public Vector3 GetPosition()
		{
			float x = (float)Random.Range((int)(Config.CreateLeftBoundary * 10f), (int)(Config.CreateRightBoundary * 10f)) / 10f;
			float y = Random.Range(Config.CreateDownBoundary, Config.CreateUpBoundary);
			float z = (float)Random.Range((int)(Config.CreateForwardBoundary * 10f), (int)(Config.CreateBackBoundary * 10f)) / 10f;
			return new Vector3(x, y, z);
		}

		public Vector3 GetPositionLimitMinY(float minY)
		{
			float x = (float)Random.Range((int)(Config.CreateLeftBoundary * 10f), (int)(Config.CreateRightBoundary * 10f)) / 10f;
			float y = Random.Range(minY, Config.CreateUpBoundary);
			float z = (float)Random.Range((int)(Config.CreateForwardBoundary * 10f), (int)(Config.CreateBackBoundary * 10f)) / 10f;
			return new Vector3(x, y, z);
		}

		public Vector3 GetRandomAngle()
		{
			float x = Random.Range(0, 360);
			float y = Random.Range(0, 360);
			float z = Random.Range(0, 360);
			return new Vector3(x, y, z);
		}
	}
}
