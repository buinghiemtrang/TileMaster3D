using System.Collections.Generic;
using GameScene_Element;
using UnityEngine;

namespace GameScene_Element_Data
{
	public class ElementsData
	{
		public int TotalNumber { get; private set; }

		public int RemainingNumber { get; private set; }

		public int NeedClearNumber { get; private set; }

		public Dictionary<int, List<Element>> Elements { get; private set; }

		public List<Dictionary<int, List<Element>>> WaveElements { get; private set; }

		public Dictionary<int, List<Element>> DisplayedElementsId { get; private set; }

		public Dictionary<Transform, Element> DisplayedElementsTf { get; private set; }

		public int DisplayedTotalNumber { get; private set; }

		public int DisplayedCollectNumber { get; private set; }

		public ElementsData()
		{
			Elements = new Dictionary<int, List<Element>>();
			WaveElements = new List<Dictionary<int, List<Element>>>();
			DisplayedElementsId = new Dictionary<int, List<Element>>();
			DisplayedElementsTf = new Dictionary<Transform, Element>();
		}

		public void Init()
		{
			_Clear();
			NeedClearNumber = 0;
			TotalNumber = 0;
			RemainingNumber = 0;
			DisplayedTotalNumber = 0;
			DisplayedCollectNumber = 0;
		}

		public void ResetDisplayNumber()
		{
			DisplayedTotalNumber = 0;
			DisplayedCollectNumber = 0;
		}

		public void SetTotalNumber(int value)
		{
			if (TotalNumber <= value)
			{
				TotalNumber = value;
			}
		}

		public void AddWaveElements(List<Element> elements)
		{
			_JoinTotalElements(elements);
			Dictionary<int, List<Element>> dictionary = new Dictionary<int, List<Element>>();
			for (int i = 0; i < elements.Count; i++)
			{
				Element element = elements[i];
				int id = element.Config.id;
				if (!dictionary.ContainsKey(id))
				{
					List<Element> list = new List<Element>();
					list.Add(element);
					dictionary.Add(id, list);
				}
				else
				{
					dictionary[id].Add(element);
				}
			}
			WaveElements.Add(dictionary);
		}

		public List<Element> GetNewWaveElementsAndRemove()
		{
			List<Element> list = new List<Element>();
			if (WaveElements.Count == 0)
			{
				return list;
			}
			foreach (KeyValuePair<int, List<Element>> item in WaveElements[0])
			{
				list.AddRange(item.Value);
			}
			WaveElements.RemoveAt(0);
			return list;
		}

		public void AddDisplayedElements(List<Element> elements, bool isNew)
		{
			if (isNew)
			{
				_JoinTotalElements(elements);
			}
			for (int i = 0; i < elements.Count; i++)
			{
				Element element = elements[i];
				int id = element.Config.id;
				if (!DisplayedElementsId.ContainsKey(id))
				{
					List<Element> list = new List<Element>();
					list.Add(element);
					DisplayedElementsId.Add(id, list);
				}
				else
				{
					DisplayedElementsId[id].Add(element);
				}
				DisplayedElementsTf.Add(element.Self, element);
				DisplayedTotalNumber++;
			}
		}

		public Element GetPlacedElementById(int id)
		{
			if (!DisplayedElementsId.ContainsKey(id))
			{
				return null;
			}
			List<Element> list = DisplayedElementsId[id];
			for (int i = 0; i < list.Count; i++)
			{
				Element element = list[i];
				if (!element.IsGuided && !element.IsLocked && !element.IsSelected)
				{
					return element;
				}
			}
			return null;
		}

		public Element GetElement(Transform tf)
		{
			if (!DisplayedElementsTf.TryGetValue(tf, out var value))
			{
				value = null;
			}
			return value;
		}

		public void Remove(List<Element> elements)
		{
			for (int i = 0; i < elements.Count; i++)
			{
				Element element = elements[i];
				int id = element.Config.id;
				if (DisplayedElementsId.TryGetValue(id, out var value))
				{
					value.Remove(element);
					if (value.Count == 0)
					{
						DisplayedElementsId.Remove(id);
					}
				}
				DisplayedElementsTf.Remove(element.Self);
			}
			_RemoveFromTotalElements(elements);
		}

		public bool JudgeWholePlaced()
		{
			foreach (KeyValuePair<int, List<Element>> element in Elements)
			{
				List<Element> value = element.Value;
				for (int i = 0; i < value.Count; i++)
				{
					if (!value[i].IsLocked)
					{
						return false;
					}
				}
			}
			return true;
		}

		private void _Clear()
		{
			WaveElements.Clear();
			DisplayedElementsId.Clear();
			DisplayedElementsTf.Clear();
			foreach (KeyValuePair<int, List<Element>> element in Elements)
			{
				List<Element> value = element.Value;
				foreach (Element item in value)
				{
					item.Destroy();
				}
				value.Clear();
			}
			Elements.Clear();
		}

		private void _JoinTotalElements(IReadOnlyList<Element> elements)
		{
			for (int i = 0; i < elements.Count; i++)
			{
				Element element = elements[i];
				int id = element.Config.id;
				if (!Elements.ContainsKey(id))
				{
					List<Element> list = new List<Element>();
					list.Add(element);
					Elements.Add(id, list);
				}
				else
				{
					Elements[id].Add(element);
				}
				TotalNumber++;
				RemainingNumber++;
			}
			_CalculateNeedClearNumber();
		}

		private void _CalculateNeedClearNumber()
		{
			int num = 0;
			foreach (KeyValuePair<int, List<Element>> element in Elements)
			{
				num += element.Value.Count / 3;
			}
			NeedClearNumber = num;
		}

		private void _RemoveFromTotalElements(IReadOnlyList<Element> elements)
		{
			for (int i = 0; i < elements.Count; i++)
			{
				Element element = elements[i];
				int id = element.Config.id;
				if (!Elements.TryGetValue(id, out var value))
				{
					return;
				}
				if (!value.Remove(element))
				{
					return;
				}
				element.Destroy();
				if (value.Count == 0)
				{
					Elements.Remove(id);
				}
				RemainingNumber--;
				DisplayedCollectNumber++;
			}
			_CalculateNeedClearNumber();
		}
	}
}
