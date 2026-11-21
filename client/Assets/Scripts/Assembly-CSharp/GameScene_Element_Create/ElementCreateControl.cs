using System;
using System.Collections;
using System.Collections.Generic;
using GameScene_Element;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameScene_Element_Create
{
	public class ElementCreateControl : MonoBehaviour
	{
		[SerializeField]
		private Transform parentNode;

		private Dictionary<ElementType, List<Element>> _elements;

		private void Awake()
		{
			_elements = new Dictionary<ElementType, List<Element>>();
		}

		public List<Element> SynCreate(IReadOnlyList<cfgModel> configs)
		{
			List<Element> list = new List<Element>();
			for (int i = 0; i < configs.Count; i++)
			{
				cfgModel config = configs[i];
				GameObject res = _GetResources(config);
				for (int j = 0; j < 3; j++)
				{
					list.Add(_Create(config, res));
				}
			}
			return list;
		}

		public Element SynCreate(cfgModel config)
		{
			return _Create(config, _GetResources(config));
		}

		public List<Element> SynCreate(IReadOnlyList<int> ids, bool isGroup = true)
		{
			return SynCreate(_GetConfigs(ids));
		}

		public Element SynCreate(int id)
		{
			return SynCreate(_GetConfig(id));
		}

		public void AsyCreate(IReadOnlyList<cfgModel> configs, int upLimit = 0, float intervalTime = -1f, bool isGroup = true, Action<List<Element>> completed = null)
		{
			StartCoroutine(_IEAsyCreate(configs, upLimit, intervalTime, isGroup, completed));
		}

		public void AsyCreate(IReadOnlyList<int> ids, int upLimit = 0, float intervalTime = -1f, bool isGroup = true, Action<List<Element>> completed = null)
		{
			StartCoroutine(_IEAsyCreate(_GetConfigs(ids), upLimit, intervalTime, isGroup, completed));
		}

		public void AsyCreateGroup(cfgModel config, float intervalTime = -1f, Action<List<Element>> completed = null)
		{
			StartCoroutine(_IEAsyCreateGroup(config, intervalTime, completed));
		}

		public void AsyCreateGroup(int id, float intervalTime = -1f, Action<List<Element>> completed = null)
		{
			StartCoroutine(_IEAsyCreateGroup(_GetConfig(id), intervalTime, completed));
		}

		private IEnumerator _IEAsyCreateGroup(cfgModel config, float intervalTime, Action<List<Element>> completed)
		{
			List<Element> elements = new List<Element>();
			GameObject res = _GetResources(config);
			for (int j = 0; j < 3; j++)
			{
				elements.Add(_Create(config, res));
				if (intervalTime <= 0f)
				{
					yield return null;
				}
				else
				{
					yield return new WaitForSeconds(intervalTime);
				}
			}
			completed.InvokeSafe(elements);
		}

		private IEnumerator _IEAsyCreate(IReadOnlyList<cfgModel> configs, int upLimit, float intervalTime, bool isGroup, Action<List<Element>> completed)
		{
			List<Element> elements = new List<Element>();
			int aGroupNumber = (isGroup ? 3 : 1);
			int count = configs.Count;
			for (int i = 0; i < count; i++)
			{
				cfgModel config = configs[i];
				GameObject res = _GetResources(config);
				int number = 0;
				for (int j = 0; j < aGroupNumber; j++)
				{
					elements.Add(_Create(config, res));
					number++;
					if (number >= upLimit)
					{
						number = 0;
						if (intervalTime <= 0f)
						{
							yield return null;
						}
						else
						{
							yield return new WaitForSeconds(intervalTime);
						}
					}
				}
			}
			completed.InvokeSafe(elements);
		}

		public Coroutine RealAsyCreate(IReadOnlyList<cfgModel> configs, Action<List<Element>, object> completed, bool isGroup, object value)
		{
			return StartCoroutine(_IERealAsyCreate(configs, isGroup, completed, value));
		}

		public Coroutine RealAsyCreate(IReadOnlyList<cfgModel> configs, Action<List<Element>, object> completed, bool isGroup)
		{
			return RealAsyCreate(configs, completed, isGroup, 0);
		}

		public Coroutine RealAsyCreate(IReadOnlyList<int> ids, Action<List<Element>, object> completed, bool isGroup)
		{
			return RealAsyCreate(ids, completed, isGroup, 0);
		}

		public Coroutine RealAsyCreate(IReadOnlyList<int> ids, Action<List<Element>, object> completed, bool isGroup, object value)
		{
			return StartCoroutine(_IERealAsyCreate(_GetConfigs(ids), isGroup, completed, value));
		}

		private IEnumerator _IERealAsyCreate(IReadOnlyList<cfgModel> configs, bool isGroup, Action<List<Element>, object> completed, object value)
		{
			List<Element> elements = new List<Element>();
			int number = (isGroup ? 3 : 1);
			int count = configs.Count;
			for (int i = 0; i < count; i++)
			{
				cfgModel config = configs[i];
				ResourceRequest res = _GetResourcesByAsy(config);
				yield return res;
				for (int j = 0; j < number; j++)
				{
					elements.Add(_Create(config, res.asset as GameObject));
				}
			}
			completed.InvokeSafe(elements, value);
		}

		public void RealAsyCreateMultipleCallbacks(IReadOnlyList<cfgModel> configs, Action<List<Element>> completed, bool isGroup)
		{
			StartCoroutine(_RealAsyCreateMultipleCallbacks(configs, isGroup, completed));
		}

		public void RealAsyCreateMultipleCallbacks(IReadOnlyList<int> ids, Action<List<Element>> completed, bool isGroup)
		{
			StartCoroutine(_RealAsyCreateMultipleCallbacks(_GetConfigs(ids), isGroup, completed));
		}

		private IEnumerator _RealAsyCreateMultipleCallbacks(IReadOnlyList<cfgModel> configs, bool isGroup, Action<List<Element>> completed)
		{
			List<Element> elements = new List<Element>();
			int number = (isGroup ? 3 : 1);
			int count = configs.Count;
			for (int i = 0; i < count; i++)
			{
				cfgModel config = configs[i];
				ResourceRequest res = _GetResourcesByAsy(config);
				yield return res;
				for (int j = 0; j < number; j++)
				{
					elements.Add(_Create(config, res.asset as GameObject));
				}
				completed.InvokeSafe(elements);
				elements.Clear();
			}
		}

		private Element _Create(cfgModel config, GameObject res)
		{
			GameObject gameObject = Object.Instantiate(res, parentNode);
			if (gameObject)
			{
				Element element = _GetElement(config);
				if (element != null)
				{
					element.Init(parentNode, gameObject.transform, config);
					element.Freeze((RigidbodyConstraints)126);
					element.Self.gameObject.SetActive(false);
					return element;
				}
			}
			return null;
		}

		private Element _GetElement(cfgModel config)
		{
			ElementType type = (ElementType)config.type;
			if (_elements.TryGetValue(type, out var value))
			{
				for (int i = 0; i < value.Count; i++)
				{
					if (value[i].IsFree)
					{
						return value[i];
					}
				}
			}
			// UNVERIFIED: ánh xạ ElementType -> lớp con suy theo tên (bảng switch ở 0x25e1228 chưa đọc)
			Element element;
			switch (type)
			{
			case ElementType.Ordinary:
				element = new OrdinaryElement();
				break;
			case ElementType.Hint:
				element = new HintElement();
				break;
			case ElementType.AddTime:
				element = new AddTimeElement();
				break;
			case ElementType.Refresh:
				element = new RefreshElement();
				break;
			case ElementType.Destroy:
				element = new DestroyElement();
				break;
			case ElementType.Freeze:
				element = new FrozenElement();
				break;
			case ElementType.Star:
				element = new StarElement();
				break;
			case ElementType.Gold:
				element = new GoldElement();
				break;
			case ElementType.Fill:
				element = new FillElement();
				break;
			case ElementType.Assimilation:
				element = new AssimilationElement();
				break;
			case ElementType.RegionRankCollect:
				element = new RegionRankElement();
				break;
			default:
				element = null;
				break;
			}
			if (_elements.ContainsKey(type))
			{
				_elements[type].Add(element);
			}
			else
			{
				List<Element> list = new List<Element>();
				list.Add(element);
				_elements.Add(type, list);
			}
			return element;
		}

		private cfgModel _GetConfig(int id)
		{
			if (!DataCfg.cfgModels.TryGetValue(id, out var value))
			{
				value = null;
			}
			return value;
		}

		private List<cfgModel> _GetConfigs(IReadOnlyList<int> ids)
		{
			List<cfgModel> list = new List<cfgModel>();
			for (int i = 0; i < ids.Count; i++)
			{
				list.Add(_GetConfig(ids[i]));
			}
			return list;
		}

		private GameObject _GetResources(cfgModel config)
		{
			GameObject gameObject = Resources.Load<GameObject>("Elements/" + config.prefab);
			if (!gameObject)
			{
				gameObject = null;
			}
			return gameObject;
		}

		private ResourceRequest _GetResourcesByAsy(cfgModel config)
		{
			return Resources.LoadAsync("Elements/" + config.prefab, typeof(Object));
		}
	}
}
