using System;
using System.Collections.Generic;
using System.Reflection;

public class DataBufferPool<T>
{
	private T root;

	private Dictionary<string, object> dicts;

	private Dictionary<string, string> alias;

	private Dictionary<string, Action> funcs;

	public Action callback_init;

	public Action callback_send;

	public Action<string, object, string, object> callback_add;

	private string _name;

	public void SetRoot(T _root)
	{
		root = _root;
	}

	public DataBufferPool(string name)
	{
		dicts = new Dictionary<string, object>();
		alias = new Dictionary<string, string>();
		funcs = new Dictionary<string, Action>();
		_name = name;
	}

	public void ModifyInt(string key, int value)
	{
		ModifyInt(key, value, _name + "." + key);
	}

	public void ModifyInt(string key, int value, string alias)
	{
		dicts[key] = (int)(dicts.ContainsKey(key) ? dicts[key] : _get_value(key)) + value;
		this.alias[key] = alias;
	}

	public void Add(string key, object value, string alias)
	{
		dicts[key] = value;
		this.alias[key] = alias;
	}

	public void Add(string key, object value)
	{
		Add(key, value, _name + "." + key);
	}

	public void Add(string key, object value, Action callback)
	{
		Add(key, value, "");
		funcs[key] = callback;
	}

	public void Save()
	{
		if (dicts.Count == 0)
		{
			return;
		}
		callback_init?.Invoke();
		foreach (KeyValuePair<string, object> item in dicts)
		{
			object obj = root;
			string key = item.Key;
			string[] parts = item.Key.Split('.');
			bool isListItem = parts[parts.Length - 1].StartsWith("|");
			int count = parts.Length;
			if (isListItem)
			{
				key = key.Substring(0, key.IndexOf(".|"));
				count--;
			}
			FieldInfo field = null;
			for (int i = 0; i < count; i++)
			{
				if (i == count - 1)
				{
					field = obj.GetType().GetField(parts[i]);
				}
				else
				{
					obj = obj.GetType().GetField(parts[i]).GetValue(obj);
				}
			}
			if (isListItem)
			{
				string[] listInfo = parts[parts.Length - 1].Substring(1, parts[parts.Length - 1].Length - 1).Split('-');
				if (listInfo[0].Equals("List<int>"))
				{
					List<int> list = (List<int>)field.GetValue(obj);
					int index = int.Parse(listInfo[1]);
					list[index] = (int)item.Value;
					callback_add(key, list, alias[item.Key], item.Value);
				}
			}
			else if (field != null)
			{
				field.SetValue(obj, item.Value);
				callback_add(key, item.Value, alias[item.Key], item.Value);
			}
			if (funcs.ContainsKey(item.Key))
			{
				funcs[item.Key]();
			}
		}
		Clear();
		callback_send?.Invoke();
	}

	public void Clear()
	{
		dicts.Clear();
		alias.Clear();
		funcs.Clear();
	}

	private object _get_value(string key)
	{
		object obj = root;
		string[] parts = key.Split('.');
		bool isListItem = parts[parts.Length - 1].StartsWith("|");
		int count = parts.Length - (isListItem ? 1 : 0);
		for (int i = 0; i < count; i++)
		{
			obj = obj.GetType().GetField(parts[i]).GetValue(obj);
		}
		if (isListItem)
		{
			string[] listInfo = parts[parts.Length - 1].Substring(1, parts[parts.Length - 1].Length - 1).Split('-');
			if (listInfo[0].Equals("List<int>"))
			{
				int index = int.Parse(listInfo[1]);
				return ((List<int>)obj)[index];
			}
		}
		return obj;
	}

	public void Write(string key, object value)
	{
		Add(key, value);
		Save();
	}
}
