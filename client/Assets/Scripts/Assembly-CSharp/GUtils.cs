using UnityEngine;

public static class GUtils
{
	public static GameObject GetDontDestroyObject(string goName)
	{
		GameObject gameObject = GameObject.Find(goName);
		if (gameObject == null)
		{
			gameObject = new GameObject(goName);
		}
		Object.DontDestroyOnLoad(gameObject);
		return gameObject;
	}

	public static T GetDontDestroyObjectWithMono<T>() where T : MonoBehaviour
	{
		GameObject dontDestroyObject = GetDontDestroyObject(typeof(T).ToString());
		T component = dontDestroyObject.GetComponent<T>();
		if (component == null)
		{
			component = dontDestroyObject.AddComponent<T>();
		}
		return component;
	}

	public static int[] strSplit2Int(string str, char c = '|')
	{
		int result = 0;
		string[] array = str.Split(c);
		int[] array2 = new int[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (!int.TryParse(array[i], out result))
			{
				Debug.LogError("[strSplit2Int] invalid str:" + str);
				return array2;
			}
			array2[i] = result;
		}
		return array2;
	}
}
