using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class LoadMgr
{
	private static readonly Dictionary<string, Object[]> m_pAtlasDic = new Dictionary<string, Object[]>();

	private static string _sSpriteAtlasPath;

	public static Sprite LoadAtlasSprite(string _spriteAtlasPath, string _spriteName)
	{
		_sSpriteAtlasPath = _spriteAtlasPath;
		Sprite sprite = FindSpriteFormBuffer(_spriteAtlasPath, _spriteName);
		if (sprite != null)
		{
			return sprite;
		}
		Object[] array = Resources.LoadAll(_spriteAtlasPath);
		m_pAtlasDic.Add(_spriteAtlasPath, array);
		return SpriteFormAtlas(array, _spriteName);
	}

	public static void LoadAtlas(string _spriteAtlasPath)
	{
		if (!m_pAtlasDic.ContainsKey(_spriteAtlasPath))
		{
			m_pAtlasDic.Add(_spriteAtlasPath, Resources.LoadAll(_spriteAtlasPath));
		}
	}

	public static void DeleteAtlas(string _spriteAtlasPath)
	{
		if (m_pAtlasDic.ContainsKey(_spriteAtlasPath))
		{
			m_pAtlasDic.Remove(_spriteAtlasPath);
		}
	}

	private static Sprite FindSpriteFormBuffer(string _spriteAtlasPath, string _spriteName)
	{
		if (m_pAtlasDic.ContainsKey(_spriteAtlasPath))
		{
			return SpriteFormAtlas(m_pAtlasDic[_spriteAtlasPath], _spriteName);
		}
		return null;
	}

	private static Sprite SpriteFormAtlas(IEnumerable<Object> _atlas, string _spriteName)
	{
		foreach (Object item in _atlas)
		{
			Sprite sprite = item as Sprite;
			if (sprite != null && sprite.name == _spriteName)
			{
				return sprite;
			}
		}
		return null;
	}

	public static T LoadRes<T>(string sFile) where T : Object
	{
		T val = Resources.Load<T>(sFile);
		T result = null;
		if (val)
		{
			result = Object.Instantiate(val);
			result.name = val.name;
		}
		return result;
	}

	public static Sprite LoadSprite(string sFile)
	{
		Sprite sprite = Resources.Load<Sprite>(sFile);
		if (!sprite)
		{
			return null;
		}
		return sprite;
	}

	public static T Load<T>(string sFile) where T : Object
	{
		T val = Resources.Load<T>(sFile);
		if (!val)
		{
			return null;
		}
		return val;
	}

	private static string __loadText(string sOriginFile, string sRealityFile, string type)
	{
		if (Util.File.IsExistsByLoc(sRealityFile + type))
		{
			return Util.File.ReadLocalText(sRealityFile + type);
		}
		TextAsset textAsset = Resources.Load<TextAsset>(sRealityFile);
		if (!textAsset && sRealityFile != sOriginFile)
		{
			textAsset = Resources.Load<TextAsset>(sOriginFile);
		}
		if (!textAsset)
		{
			return null;
		}
		return textAsset.text;
	}

	public static string LoadText(string sFile, string type = "")
	{
		string sRealityFile = UserLabelManager.Instance.GetRealPath(sFile);
		return __loadText(sFile, sRealityFile, type);
	}

	public static T LoadJson<T>(string sFile)
	{
		string text = LoadText(sFile, ".json");
		if (text == null)
		{
			return default(T);
		}
		return JsonConvert.DeserializeObject<T>(text);
	}
}
