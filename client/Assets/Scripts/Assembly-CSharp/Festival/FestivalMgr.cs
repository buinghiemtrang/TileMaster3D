using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Festival
{
	public class FestivalMgr : Singleton<FestivalMgr>
	{
		public const string LOADING_LOCALSAVE = "LOADING_LOCALSAVE";

		public const string LOADING_LOCALNAMESAVE = "LOADING_LOCALNAMESAVE";

		public const string UPDATE_FESTIVAL = "UPDATE_FESTIVAL";

		private string _currentFestivalType = string.Empty;

		private cfgFestival _currentFestival;

		public bool IsShow
		{
			get
			{
				return GetShow();
			}
			set
			{
				DB.User.IsShowFestivalSkin = value;
				UpdateLocalShow(value);
				DispatcherMgr.Emit("UPDATE_FESTIVAL");
			}
		}

		public bool HasFestival
		{
			get
			{
				string realFestivalType = GetRealFestivalType();
				if (!string.IsNullOrEmpty(realFestivalType))
				{
					return !realFestivalType.Equals(FestivalType.Ordinary.ToString());
				}
				return false;
			}
		}

		public void Initialize()
		{
			_currentFestivalType = FestivalType.Ordinary.ToString();
			DateTime now = DateTime.Now;
			if (DataCfg.cfgFestivals == null || DataCfg.cfgFestivals.Count == 0)
			{
				DataCfg.cfgFestivals = LoadMgr.LoadJson<List<cfgFestival>>("Json/cfgFestival");
			}
			for (int i = 0; i < DataCfg.cfgFestivals.Count; i++)
			{
				cfgFestival cfgFestival = DataCfg.cfgFestivals[i];
				if (new ActivitySpan(cfgFestival.startTime, cfgFestival.endTime).Contain(now))
				{
					_currentFestival = cfgFestival;
					_currentFestivalType = cfgFestival.type;
					break;
				}
			}
			UpdateLocalName(_currentFestivalType);
			UpdateLocalShow(GetShow());
		}

		public string GetFestivalNameKey()
		{
			if (_currentFestival != null)
			{
				return _currentFestival.themeName;
			}
			return "";
		}

		public string GetRealFestivalType()
		{
			if (string.IsNullOrEmpty(_currentFestivalType) && DataCfg.cfgFestivalPointInformations.Count == 0)
			{
				_currentFestivalType = GetLocalFestivalName(FestivalType.Ordinary.ToString());
			}
			return _currentFestivalType;
		}

		public string GetCurrentFestivalType()
		{
			if (!GetShow())
			{
				return FestivalType.Ordinary.ToString();
			}
			return GetRealFestivalType();
		}

		public cfgFestivalPointInformation GetFestivalInfo(string pointKey)
		{
			string type = GetCurrentFestivalType();
			if (DataCfg.cfgFestivalPointInformations == null || DataCfg.cfgFestivalPointInformations.Count == 0)
			{
				DataCfg.cfgFestivalPointInformations = LoadMgr.LoadJson<List<cfgFestivalPointInformation>>("Json/cfgFestivalPointInformation");
			}
			List<cfgFestivalPointInformation> list = DataCfg.cfgFestivalPointInformations.Where((cfgFestivalPointInformation w) => w.type.Equals(type) && w.pointKey.Equals(pointKey)).ToList();
			if (list.Count == 1)
			{
				return list[0];
			}
			return null;
		}

		private bool GetShow()
		{
			if (DB.User != null)
			{
				if (DB.User.IsShowFestivalSkin)
				{
					return cf_festival_const.isOpen == 1;
				}
				return false;
			}
			return GetLocalFestivalShow();
		}

		public void UpdateLocalName(string name)
		{
			PlayerPrefs.SetString("LOADING_LOCALNAMESAVE", name);
		}

		public void UpdateLocalShow(bool isShow)
		{
			PlayerPrefs.SetInt("LOADING_LOCALSAVE", isShow ? 1 : 0);
		}

		public string GetLocalFestivalName(string defaultName)
		{
			return PlayerPrefs.GetString("LOADING_LOCALNAMESAVE", defaultName);
		}

		public bool GetLocalFestivalShow()
		{
			return PlayerPrefs.GetInt("LOADING_LOCALSAVE", 1) == 1;
		}

		public T LoadAndInstantiate<T>(string filePath) where T : UnityEngine.Object
		{
			T val = LoadMgr.LoadRes<T>(string.Format(filePath, GetCurrentFestivalType()));
			if (val == null)
			{
				val = LoadMgr.LoadRes<T>(string.Format(filePath, FestivalType.Ordinary.ToString()));
			}
			return val;
		}

		public T LoadAsset<T>(string filePath) where T : UnityEngine.Object
		{
			T val = LoadMgr.Load<T>(string.Format(filePath, GetCurrentFestivalType()));
			if (val == null)
			{
				val = LoadMgr.Load<T>(string.Format(filePath, FestivalType.Ordinary.ToString()));
			}
			return val;
		}

		public T AddTopNode<T>(string filePath) where T : Component
		{
			T val = SceneMgr.AddTopNode<T>(string.Format(filePath, GetCurrentFestivalType()));
			if (val == null)
			{
				val = SceneMgr.AddTopNode<T>(string.Format(filePath, FestivalType.Ordinary.ToString()));
			}
			return val;
		}

		public T AddNode<T>(string filePath, Transform parent) where T : Component
		{
			T val = SceneMgr.AddNode<T>(string.Format(filePath, GetCurrentFestivalType()), parent);
			if (val == null)
			{
				val = SceneMgr.AddNode<T>(string.Format(filePath, FestivalType.Ordinary.ToString()), parent);
			}
			return val;
		}
	}
}
