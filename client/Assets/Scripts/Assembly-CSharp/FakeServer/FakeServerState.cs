// [rebuild] Trạng thái fake server (FAKE_SERVER): một JObject lưu ở persistentDataPath/fake_server.json.
// Handler đọc/ghi qua Get/Set rồi FakeServer gọi SaveIfDirty sau mỗi request.
// Xoá file này = reset server (khác với xoá save của game trong PlayerPrefs).
#if FAKE_SERVER
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class FakeServerState
{
	private static FakeServerState _instance;

	private JObject _root;

	private bool _dirty;

	public static FakeServerState I
	{
		get
		{
			if (_instance == null)
			{
				_instance = new FakeServerState();
				_instance._Load();
			}
			return _instance;
		}
	}

	public static string FilePath => Path.Combine(Application.persistentDataPath, "fake_server.json");

	public JObject Root => _root;

	public JToken Get(string key)
	{
		return _root[key];
	}

	public T Get<T>(string key, T fallback)
	{
		JToken t = _root[key];
		if (t == null || t.Type == JTokenType.Null)
		{
			return fallback;
		}
		try
		{
			return t.ToObject<T>();
		}
		catch (Exception)
		{
			return fallback;
		}
	}

	public void Set(string key, JToken value)
	{
		_root[key] = value;
		_dirty = true;
	}

	public void SetValue<T>(string key, T value)
	{
		Set(key, (value == null) ? JValue.CreateNull() : JToken.FromObject(value));
	}

	// Gọi sau khi sửa trực tiếp JObject lấy từ Get (đừng Set lại token đã có cha: Newtonsoft sẽ clone).
	public void MarkDirty()
	{
		_dirty = true;
	}

	public void SaveIfDirty()
	{
		if (!_dirty)
		{
			return;
		}
		_dirty = false;
		try
		{
			File.WriteAllText(FilePath, _root.ToString(Newtonsoft.Json.Formatting.Indented));
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[FakeServer] save state failed: " + ex.Message);
		}
	}

	private void _Load()
	{
		_root = null;
		try
		{
			if (File.Exists(FilePath))
			{
				_root = JObject.Parse(File.ReadAllText(FilePath));
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[FakeServer] load state failed, reset: " + ex.Message);
		}
		if (_root == null)
		{
			_root = new JObject();
		}
	}
}
#endif
