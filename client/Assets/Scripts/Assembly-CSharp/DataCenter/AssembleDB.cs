using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using BestHTTP;

namespace DataCenter
{
	public class AssembleDB<T> : IAssembleDB where T : AbsDataModel, new()
	{
		private string _sUserId;

		private readonly string _sDBName;

		private const BindingFlags KBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

		private HTTPRequest _curRequest;

		private readonly AbsDispatchServlet _pAbsDispatch;

		private readonly Dictionary<string, object> _dataCache;

		private readonly Dictionary<string, DBValueType> _dictType;

		private bool _bSending;

		private DataInfo _curDispatch;

		private bool _bOpenDequeue;

		private List<DataInfo> _queueDispatch;

		private float _repeatSendTime;

		private string DispatchQueueStorage => "DispatchQueueStorage." + _sUserId + "." + _sDBName;

		public T Data { get; set; }

		public Type GetClsType()
		{
			return typeof(T);
		}

		public object GetData()
		{
			return Data;
		}

		public void SetData(object value)
		{
			_CopyDataBy((T)value);
		}

		public object Create()
		{
			return Activator.CreateInstance<T>();
		}

		public string GetName()
		{
			return _sDBName;
		}

		public AssembleDB(AbsDispatchServlet absDispatch, string name)
		{
			_sUserId = "";
			_dataCache = new Dictionary<string, object>();
			_dictType = new Dictionary<string, DBValueType>();
			_queueDispatch = new List<DataInfo>();
			_repeatSendTime = 5f;
			_pAbsDispatch = absDispatch;
			if (string.IsNullOrEmpty(name))
			{
				name = typeof(T).Name;
			}
			_sDBName = name;
			InitValueType();
		}

		public void InitQueueDispatch(string userid)
		{
			_sUserId = userid;
			_queueDispatch.Clear();
			if (PlayerPrefs.HasKey(DispatchQueueStorage))
			{
				_queueDispatch = JsonConvert.DeserializeObject<List<DataInfo>>(PlayerPrefs.GetString(DispatchQueueStorage, "[]"));
			}
		}

		private void InitValueType()
		{
			DBValueType dBValueType = new DBValueType();
			FieldInfo[] fields = typeof(T).GetFields(KBindingFlags);
			foreach (FieldInfo fieldInfo in fields)
			{
				DBValueAttribute customAttribute = fieldInfo.GetCustomAttribute<DBValueAttribute>();
				_dictType.Add(fieldInfo.Name, (customAttribute != null) ? customAttribute.type : dBValueType);
			}
		}

		private AssembleDB<T> Add(string key, object value)
		{
			string key2 = _sDBName + "." + key;
			DBValueType dBValueType = _dictType[key];
			if (dBValueType.kStore != DBStoreType.None)
			{
				WriteLocalDataOne(key2, value);
			}
			if (dBValueType.kPut == DBPutType.Remote)
			{
				AddDataCache(key, value);
			}
			return this;
		}

		private AssembleDB<T> Add(string field, string key, object value, string alias, object obj)
		{
			DBValueType dBValueType = _dictType[field];
			if (dBValueType.kStore != DBStoreType.None)
			{
				WriteLocalDataOne(alias, obj);
			}
			if (dBValueType.kPut == DBPutType.Remote)
			{
				AddDataCache(key, value);
			}
			return this;
		}

		private void AddDataCache(string key, object value)
		{
			if (_pAbsDispatch.bLogined)
			{
				_dataCache[key] = value;
			}
		}

		public void SetValue<T1>(T1 cls, string key, object value)
		{
			Type type = cls.GetType();
			FieldInfo field = type.GetField(key);
			if (!(field == null))
			{
				object value2 = value;
				if (value is JArray || value is JObject)
				{
					value2 = JsonConvert.DeserializeObject(value.ToString(), field.FieldType);
				}
				else if (field.FieldType.Name == "Int32")
				{
					value2 = Convert.ToInt32(value);
				}
				else if (field.FieldType.Name == "Single")
				{
					value2 = Convert.ToSingle(value);
				}
				field.SetValue(cls, value2);
			}
		}

		public void SetValue(string key, object value)
		{
			SetValue(Data, key, value);
		}

		public void Save(string key, object value)
		{
			if (key.IndexOf('.') == -1)
			{
				Add(key, value);
			}
			else
			{
				string text = key.Split('.')[0];
				FieldInfo field = typeof(T).GetField(text, KBindingFlags);
				if (field == null)
				{
					return;
				}
				string alias = _sDBName + "." + text;
				object value2 = field.GetValue(Data);
				Add(text, key, value, alias, value2);
			}
			Send();
		}

		public void SaveForced(string key, object value)
		{
			Write(key, value);
			AddDataCache(key, value);
			if (_dataCache.Count > 0)
			{
				DataMerge(_dataCache);
				_dataCache.Clear();
				PlayerPrefs.SetString(DispatchQueueStorage, JsonConvert.SerializeObject(_queueDispatch));
			}
			if (!_bSending)
			{
				_bSending = true;
				DelayCallBack.Start(3f, _send);
			}
		}

		public void Write(string key)
		{
			FieldInfo field = typeof(T).GetField(key, KBindingFlags);
			if (!(field == null))
			{
				string key2 = _sDBName + "." + key;
				object value = field.GetValue(Data);
				WriteLocalDataOne(key2, value);
			}
		}

		public void Write(string key, object value)
		{
			string key2 = _sDBName + "." + key;
			WriteLocalDataOne(key2, value);
		}

		private void Send()
		{
			if (_dataCache.Count > 0)
			{
				DataMerge(_dataCache);
				_dataCache.Clear();
				PlayerPrefs.SetString(DispatchQueueStorage, JsonConvert.SerializeObject(_queueDispatch));
			}
			if (!_bSending)
			{
				_bSending = true;
				DelayCallBack.Start(3f, _send);
			}
		}

		public void Send(string key, object value)
		{
			AddDataCache(key, value);
			if (_dataCache.Count > 0)
			{
				DataMerge(_dataCache);
				_dataCache.Clear();
				PlayerPrefs.SetString(DispatchQueueStorage, JsonConvert.SerializeObject(_queueDispatch));
			}
			if (!_bSending)
			{
				_bSending = true;
				DelayCallBack.Start(3f, _send);
			}
		}

		private void _send()
		{
			_bSending = false;
			DequeueEnabled();
		}

		public void ReadValue(string key)
		{
			FieldInfo field = typeof(T).GetField(key, KBindingFlags);
			ReadLocalDataOne(field);
		}

		public void UploadDataAllToRemote()
		{
			FieldInfo[] fields = typeof(T).GetFields(KBindingFlags);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (_dictType[fieldInfo.Name].kPut == DBPutType.Remote)
				{
					AddDataCache(fieldInfo.Name, fieldInfo.GetValue(Data));
				}
			}
			Send();
		}

		public void InitLocalData()
		{
			Data.InitData();
		}

		public void ReadLocalData()
		{
			FieldInfo[] fields = typeof(T).GetFields(KBindingFlags);
			foreach (FieldInfo info in fields)
			{
				ReadLocalDataOne(info);
			}
			Data.ReadLocalData();
		}

		public void DelLocalData()
		{
			FieldInfo[] fields = typeof(T).GetFields(KBindingFlags);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (_dictType[fieldInfo.Name].kStore != DBStoreType.Permanent)
				{
					PlayerPrefs.DeleteKey(_sDBName + "." + fieldInfo.Name);
				}
			}
			Data.DelLocalData();
		}

		public void WriteLocalData()
		{
			FieldInfo[] fields = typeof(T).GetFields(KBindingFlags);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (_dictType[fieldInfo.Name].kStore != DBStoreType.None)
				{
					WriteLocalDataOne(_sDBName + "." + fieldInfo.Name, fieldInfo.GetValue(Data));
				}
			}
			Data.WriteLocalData();
		}

		private static void WriteLocalDataOne(string key, object value)
		{
			if (string.IsNullOrEmpty(key))
			{
				return;
			}
			if (value is int)
			{
				PlayerPrefs.SetInt(key, (int)value);
				return;
			}
			if (value is float)
			{
				PlayerPrefs.SetFloat(key, (float)value);
				return;
			}
			if (value is bool)
			{
				PlayerPrefs.SetInt(key, ((bool)value) ? 1 : 0);
				return;
			}
			if (!(value is string))
			{
				value = JsonConvert.SerializeObject(value);
			}
			PlayerPrefs.SetString(key, (string)value);
		}

		private void ReadLocalDataOne(FieldInfo info)
		{
			string key = _sDBName + "." + info.Name;
			if (PlayerPrefs.HasKey(key))
			{
				Type fieldType = info.FieldType;
				if (fieldType == typeof(int))
				{
					info.SetValue(Data, PlayerPrefs.GetInt(key, 0));
				}
				else if (fieldType == typeof(float))
				{
					info.SetValue(Data, PlayerPrefs.GetFloat(key, 0f));
				}
				else if (fieldType == typeof(bool))
				{
					info.SetValue(Data, PlayerPrefs.GetInt(key, 0) == 1);
				}
				else if (fieldType == typeof(string))
				{
					info.SetValue(Data, PlayerPrefs.GetString(key, ""));
				}
				else
				{
					info.SetValue(Data, JsonConvert.DeserializeObject(PlayerPrefs.GetString(key, ""), fieldType));
				}
			}
		}

		public void ReadRemoteData(string sBody)
		{
			_CopyDataBy(JsonConvert.DeserializeObject<T>(GetRemoteDataToString(sBody)));
		}

		private void _CopyDataBy(T data)
		{
			FieldInfo[] fields = typeof(T).GetFields(KBindingFlags);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (_dictType[fieldInfo.Name].kPut == DBPutType.Remote)
				{
					fieldInfo.SetValue(Data, fieldInfo.GetValue(data));
				}
			}
		}

		public string GetRemoteDataToString(string sBody)
		{
			Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(sBody);
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			foreach (KeyValuePair<string, object> item in _queueDispatch.SelectMany((DataInfo d) => d.data))
			{
				dictionary2[item.Key] = item.Value;
			}
			foreach (KeyValuePair<string, object> item2 in dictionary2)
			{
				_DeserializeQueueDispatch(dictionary, item2.Key, item2.Value);
			}
			return JsonConvert.SerializeObject(dictionary);
		}

		private void _DeserializeQueueDispatch(IDictionary<string, object> data, string key, object value)
		{
			int num = key.IndexOf('.');
			if (num == -1)
			{
				data[key] = value;
				return;
			}
			string key2 = key.Substring(0, num);
			string key3 = key.Substring(num + 1, key.Length - num - 1);
			if (!data.ContainsKey(key2))
			{
				data[key2] = new Dictionary<string, object>();
			}
			Dictionary<string, object> data2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(data[key2].ToString());
			_DeserializeQueueDispatch(data2, key3, value);
			data[key2] = data2;
		}

		private void DataMerge(Dictionary<string, object> dict)
		{
			DataInfo dataInfo = null;
			if (_queueDispatch.Count >= 1)
			{
				dataInfo = _queueDispatch[_queueDispatch.Count - 1];
			}
			if (dataInfo == null || dataInfo == _curDispatch)
			{
				DataInfo dataInfo2 = new DataInfo();
				dataInfo2.data = new Dictionary<string, object>(dict);
				_queueDispatch.Add(dataInfo2);
				return;
			}
			foreach (string key in dict.Keys)
			{
				dataInfo.data[key] = dict[key];
			}
		}

		public void DequeueEnabled()
		{
			if (!_bOpenDequeue)
			{
				_Dequeue();
			}
		}

		public void DequeueDisable()
		{
			if (_curRequest != null)
			{
				_curRequest.Abort();
			}
			DelayCallBack.Stop(_send);
		}

		private void _Dequeue()
		{
			DelayCallBack.Stop(_send);
			if (_queueDispatch.Count == 0 || !_pAbsDispatch.bLogined)
			{
				_bOpenDequeue = false;
				return;
			}
			_bOpenDequeue = true;
			_curDispatch = _queueDispatch[0];
			_curRequest = _pAbsDispatch.Post(_sDBName, _curDispatch, delegate(bool success)
			{
				_curRequest = null;
				if (!success)
				{
					_repeatSendTime += _repeatSendTime;
					DelayCallBack.Start(_repeatSendTime, _send);
				}
				else
				{
					_repeatSendTime = 5f;
					_queueDispatch.RemoveAt(0);
					_curDispatch = null;
					PlayerPrefs.SetString(DispatchQueueStorage, JsonConvert.SerializeObject(_queueDispatch));
					_Dequeue();
				}
			});
		}
	}
}
