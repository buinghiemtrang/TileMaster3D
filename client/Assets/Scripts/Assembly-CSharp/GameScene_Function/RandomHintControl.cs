using System.Collections.Generic;
using GameScene;
using GameScene_Element;

namespace GameScene_Function
{
	public class RandomHintControl : FunctionBase
	{
		private readonly List<Element> _hintElements = new List<Element>();

		public override void Init()
		{
			_hintElements.Clear();
		}

		public bool JudgeCanUse()
		{
			_hintElements.Clear();
			_hintElements.AddRange(GetHintElements());
			return _hintElements.Count > 0;
		}

		public IEnumerable<Element> GetHintElements()
		{
			List<Element> list = new List<Element>();
			_HintRule(out int id, out int number);
			if (number != 0)
			{
				foreach (KeyValuePair<int, List<Element>> item in ElementsMgr.I.Data.DisplayedElementsId)
				{
					if (id != -1 && item.Key != id)
					{
						continue;
					}
					List<Element> value = item.Value;
					if (number > value.Count)
					{
						continue;
					}
					for (int i = 0; i < value.Count; i++)
					{
						Element element = value[i];
						if (!element.IsLocked && !element.IsSelected && !element.IsGuided)
						{
							list.Add(element);
							if (list.Count == number)
							{
								return list;
							}
						}
					}
					list.Clear();
				}
			}
			return list;
		}

		public void Use(bool isJudge = false)
		{
			if (isJudge && !JudgeCanUse())
			{
				return;
			}
			OperationTip.I.CancelHighlightTip();
			OperationTip.I.CancelHintTip();
			for (int i = 0; i < _hintElements.Count; i++)
			{
				Element element = _hintElements[i];
				element.Selected();
				element.CancelSelected();
				PlatformMgr.I.Place(element, !OperationTip.I.isHighlightTip);
			}
			_hintElements.Clear();
		}

		private void _HintRule(out int id, out int number)
		{
			id = -1;
			number = 0;
			List<PlacePoint> points = PlatformMgr.I.points;
			int count = points.Count;
			Dictionary<int, List<PlacePoint>> dictionary = new Dictionary<int, List<PlacePoint>>();
			for (int i = 0; i < count; i++)
			{
				if (!points[i].IsFree)
				{
					int key = points[i].PlacedElement.Config.id;
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, new List<PlacePoint>());
					}
					dictionary[key].Add(points[i]);
				}
			}
			if (dictionary.Count == 0)
			{
				for (int j = 0; j < count; j++)
				{
					if (points[j].IsFree)
					{
						number++;
						if (number == 3)
						{
							break;
						}
					}
					else
					{
						number = 0;
					}
				}
				if (number < 3)
				{
					number = 0;
				}
				return;
			}
			while (true)
			{
				PlacePoint placePoint = null;
				foreach (KeyValuePair<int, List<PlacePoint>> item in dictionary)
				{
					int count2 = item.Value.Count;
					if (count2 != 3)
					{
						if (count2 > 3)
						{
							count2 %= 3;
						}
						if (id == -1 || 3 - count2 < number)
						{
							id = item.Key;
							placePoint = item.Value[item.Value.Count - 1];
							number = 3 - count2;
						}
					}
				}
				if (!placePoint)
				{
					break;
				}
				int num = 0;
				for (int k = placePoint.Index + 1; k < count; k++)
				{
					if (!points[k].IsFree)
					{
						num = 0;
						continue;
					}
					num++;
					if (num == number)
					{
						return;
					}
				}
				dictionary.Remove(id);
				id = -1;
				number = 0;
			}
		}
	}
}
