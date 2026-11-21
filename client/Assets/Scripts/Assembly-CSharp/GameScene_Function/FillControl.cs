using System.Collections.Generic;
using GameScene_Element;
using GameScene_Element_Display;
using GameScene_Level;
using UnityEngine;

namespace GameScene_Function
{
	public class FillControl : FunctionBase
	{
		public override void Init()
		{
		}

		public void Use(int[] data)
		{
			if (data != null)
			{
				List<Element> elements = ElementsMgr.I.createControl.SynCreate(data);
				ElementsMgr.I.Data.AddDisplayedElements(elements, true);
				_Display(elements);
			}
		}

		public void Use(int[,] data)
		{
			if (data == null)
			{
				return;
			}
			List<int> list = new List<int>();
			int length = data.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				List<int> modelsByModelGroupConfig = LevelMgr.Data.GetModelsByModelGroupConfig(data[i, 0]);
				int num = data[i, 1];
				if (num <= 0)
				{
					continue;
				}
				while (modelsByModelGroupConfig.Count > 0)
				{
					list.Add(modelsByModelGroupConfig.PopR());
					if (num < 2)
					{
						break;
					}
					num--;
				}
			}
			List<Element> elements = ElementsMgr.I.createControl.SynCreate(list);
			ElementsMgr.I.Data.AddDisplayedElements(elements, true);
			_Display(elements);
		}

		private void _Display(List<Element> elements)
		{
			FallFromSkyDisplay display = new FallFromSkyDisplay();
			display.Display(elements, delegate(Element element)
			{
				element.SetPosition(display.GetPositionLimitMinY(4f), true);
				element.SetAngle(display.GetRandomAngle());
				element.Self.localScale = new Vector3(1.2f, 1.2f, 1.2f);
			}, null);
		}
	}
}
