using UnityEngine;

public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
	private const string RootObjectName = "SingletonMonoRoot";

	private static T instance;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				GameObject gameObject = new GameObject(typeof(T).Name);
				gameObject.transform.SetParent(Root.transform);
				instance = gameObject.AddComponent<T>();
			}
			return instance;
		}
	}

	private static GameObject Root
	{
		get
		{
			GameObject gameObject = GameObject.Find("SingletonMonoRoot");
			if (gameObject == null)
			{
				gameObject = new GameObject("SingletonMonoRoot");
				Object.DontDestroyOnLoad(gameObject);
			}
			return gameObject;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this as T;
			base.transform.SetParent(Root.transform);
			Init();
		}
		else if (instance != this)
		{
			Debug.LogErrorFormat("Singleton duplicated class:{0}", GetType().Name);
		}
	}

	protected virtual void Init()
	{
	}

	protected virtual void OnDestroy()
	{
		if (instance == this)
		{
			instance = null;
		}
	}

	public void Dispose()
	{
		Object.Destroy(base.gameObject);
	}
}
