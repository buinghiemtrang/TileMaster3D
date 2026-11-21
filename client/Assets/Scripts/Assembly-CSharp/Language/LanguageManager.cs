using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Language
{
	public class LanguageManager
	{
		public delegate void EventFunc();

		public const string LanguageIdentify = "LanguageIdentify";

		public FontManager font;

		public static LanguageManager Instance = new LanguageManager();

		private string _curLanguage;

		private Dictionary<string, ConfigurationData> cfgs = new Dictionary<string, ConfigurationData>();

		private Dictionary<string, LanguageData> languagesData;

		private readonly string path = "Language/Jsons/";

		public string Language
		{
			get
			{
				return _curLanguage;
			}
			set
			{
				if (!string.IsNullOrEmpty(_curLanguage) && _curLanguage.Equals(value))
				{
					return;
				}
				if (!cfgs.ContainsKey(value))
				{
					Debug.LogError("不支持该语言 " + value);
					_curLanguage = SystemLanguage.English.ToString();
				}
				else
				{
					_curLanguage = value;
				}
				languagesData = LoadMgr.LoadJson<Dictionary<string, LanguageData>>(path + _curLanguage);
				this._handler?.Invoke();
			}
		}

		private event EventFunc _handler;

		public LanguageManager()
		{
			font = new FontManager();
			_curLanguage = null;
			languagesData = new Dictionary<string, LanguageData>();
			Dictionary<string, ConfigurationData> dictionary = LoadMgr.LoadJson<Dictionary<string, ConfigurationData>>(path + "LanguageConfig");
			foreach (KeyValuePair<string, ConfigurationData> item in dictionary)
			{
				cfgs[item.Value.name] = item.Value;
			}
			string text = StorageMgr.GetString("LanguageIdentify", "");
			if (text.Equals(""))
			{
				text = Application.systemLanguage.ToString();
				if (!cfgs.ContainsKey(text))
				{
					if (!dictionary.TryGetValue("Default", out var value))
					{
						text = SystemLanguage.English.ToString();
						Debug.Log("语言种类配置文件中，没有Default标识!");
					}
					else
					{
						text = value.name;
					}
				}
			}
			Language = text;
		}

		public void AddEventHandler(EventFunc handler)
		{
			_handler += handler;
		}

		public void RemoveEventHandler(EventFunc handler)
		{
			_handler -= handler;
		}

		public bool IsEnglish()
		{
			return _curLanguage == "en";
		}

		public void Clear()
		{
			cfgs.Clear();
			languagesData.Clear();
			Instance = new LanguageManager();
		}

		public GameObject GetPrefab(string key)
		{
			LanguageData languageDataByKey = GetLanguageDataByKey(key);
			GameObject gameObject = Resources.Load("Language/Prefab/" + languageDataByKey.value) as GameObject;
			if (!gameObject)
			{
				Debug.LogError("没有找到预制体 " + languageDataByKey.value);
				return null;
			}
			return gameObject;
		}

		public List<string> GetLanguagesName()
		{
			return cfgs.Keys.ToList();
		}

		public List<string> GetLanguagesDescribe()
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, ConfigurationData> cfg in cfgs)
			{
				list.Add(cfg.Value.describe);
			}
			return list;
		}

		public Dictionary<string, ConfigurationData> Cfgs()
		{
			return cfgs;
		}

		public string GetLanguageName(int theIndex)
		{
			foreach (KeyValuePair<string, ConfigurationData> cfg in cfgs)
			{
				if (cfg.Value.index == theIndex)
				{
					return cfg.Key;
				}
			}
			return "en";
		}

		public string[] GetLanguageDataKeys()
		{
			return languagesData.Keys.ToArray();
		}

		public LanguageData GetLanguageDataByKey(string key)
		{
			if (!languagesData.ContainsKey(key))
			{
				return null;
			}
			LanguageData languageData = languagesData[key];
			languageData.value = languageData.value.Replace("\\u3000", "\u3000").Replace("\\n", "\n");
			return languageData;
		}
	}
}
