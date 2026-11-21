using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class SceneMgr
{
	public struct StructWinScale
	{
		public float x;

		public float y;

		public float min;

		public float max;
	}

	public static Transform tfMainRoot;

	public static Transform tfTopRoot;

	public static Transform tfSuperRoot;

	public static Transform tfMainRoot3D;

	public static Transform tfTopRoot3D;

	public static string sCurSceneName;

	public static string sEnterIn;

	public static List<GameObject> showingDialogs = new List<GameObject>();

	private static GameObject objCurScene;

	private static GameObject objCurScene3D;

	private static WaitingDialog pWaiting;

	private static Action<GameObject> onHandleCompleted;

	public static float width;

	public static float widthH;

	public static float height;

	public static float heightH;

	public static StructWinScale scale;

	private static object data;

	private static Action callback_complete;

	public static void SetData(object _data)
	{
		data = _data;
	}

	public static object GetData()
	{
		object result = data;
		data = null;
		return result;
	}

	private static void InitSceneSize()
	{
		Camera main = Camera.main;
		Vector3 vector = main.ViewportToWorldPoint(new Vector3(1f, 1f, Mathf.Abs(Camera.main.transform.position.z)));
		// UNVERIFIED: component order of the 100x scaling below follows the Ghidra pseudo-C literally (vector.x, then vector.z / camera z for the second axis); assembly was not inspected
		float num = vector.x * 100f;
		float num2 = vector.z * 100f;
		Vector3 position = Camera.main.transform.position;
		float num3 = position.z + position.z;
		num -= position.x + position.x - num;
		num2 -= num3 - num2;
		width = num;
		widthH = num * 0.5f;
		height = num2;
		heightH = num2 * 0.5f;
		// UNVERIFIED: exact log string literal not recoverable from pseudo-C
		Debug.Log("======width,height:" + width.ToString() + "," + height.ToString());
	}

	public static void SetBasicValues(Transform tfMainCanvas)
	{
		InitSceneSize();
		CanvasScaler component = tfMainCanvas.GetComponent<CanvasScaler>();
		scale.x = width / component.referenceResolution.x;
		scale.y = height / component.referenceResolution.y;
		scale.min = Mathf.Min(scale.x, scale.y);
		scale.max = Mathf.Max(scale.x, scale.y);
	}

	public static GameObject DirectLoadScene(string s_file)
	{
		if (objCurScene3D != null)
		{
			UnityEngine.Object.Destroy(objCurScene3D);
		}
		return _LoadScene2D(s_file, null, null, false);
	}

	public static GameObject LoadScene(string s_file, string s_alias = null, Action callback = null)
	{
		if (objCurScene3D != null)
		{
			UnityEngine.Object.Destroy(objCurScene3D);
		}
		return _LoadScene2D(s_file, s_alias, callback);
	}

	private static GameObject _LoadScene2D(string s_file, string s_alias = null, Action callback = null, bool isNeedAnimation = true)
	{
		showingDialogs.Clear();
		sCurSceneName = s_file;
		sEnterIn = (string.IsNullOrEmpty(s_alias) ? sCurSceneName : s_alias);
		DispatcherMgr.Clear();
		callback_complete = callback;
		GameObject gameObject = LoadMgr.LoadRes<GameObject>("_ScenePrefabs/" + s_file);
		gameObject.transform.SetParent(tfMainRoot, false);
		gameObject.transform.SetSiblingIndex(0);
		if (isNeedAnimation)
		{
			SwitchEff_Fade(gameObject);
		}
		else
		{
			if (!gameObject.GetComponent<CanvasGroup>())
			{
				gameObject.AddComponent<CanvasGroup>();
			}
			if (objCurScene)
			{
				UnityEngine.Object.Destroy(objCurScene);
				DispatcherMgr.Emit("Unload_Unused_Assets", false);
			}
			objCurScene = gameObject;
			if (callback_complete != null)
			{
				callback_complete();
			}
			callback_complete = null;
		}
		return gameObject;
	}

	public static GameObject LoadScene3D(string s_file, string s_alias = null, Action callback = null)
	{
		GameObject result = _LoadScene2D(s_file, s_alias, callback);
		if (objCurScene3D != null)
		{
			UnityEngine.Object.Destroy(objCurScene3D);
		}
		objCurScene3D = LoadMgr.LoadRes<GameObject>("_ScenePrefabs/" + s_file + "3D");
		objCurScene3D.transform.SetParent(tfMainRoot3D, false);
		return result;
	}

	private static void SwitchEff_Fade(GameObject objNewScene)
	{
		CanvasGroup groupNewScene = null;
		CanvasGroup groupCurScene = null;
		if (objCurScene != null)
		{
			groupCurScene = objCurScene.transform.GetComponent<CanvasGroup>();
			groupCurScene.interactable = false;
			groupCurScene.blocksRaycasts = false;
		}
		groupNewScene = objNewScene.GetComponent<CanvasGroup>();
		if (groupNewScene == null)
		{
			groupNewScene = objNewScene.AddComponent<CanvasGroup>();
		}
		groupNewScene.interactable = false;
		Sequence sequence = DOTween.Sequence();
		if (groupCurScene != null)
		{
			sequence.Append(groupCurScene.DOFade(0f, 0.2f));
		}
		sequence.OnComplete(delegate
		{
			if (objCurScene != null)
			{
				UnityEngine.Object.Destroy(objCurScene);
				DispatcherMgr.Emit("Unload_Unused_Assets", false);
			}
			objCurScene = objNewScene;
			groupNewScene.interactable = true;
			if (groupCurScene != null)
			{
				groupCurScene.blocksRaycasts = true;
			}
			if (callback_complete != null)
			{
				callback_complete();
			}
			callback_complete = null;
		});
	}

	public static T AddNode<T>(string s_file, Transform parent, bool worldPositionStays = false) where T : Component
	{
		T val = LoadMgr.LoadRes<T>(s_file);
		if (val == null)
		{
			return null;
		}
		if (!parent)
		{
			parent = tfMainRoot;
		}
		val.transform.SetParent(parent, worldPositionStays);
		showingDialogs.Add(val.gameObject);
		return val;
	}

	public static T AddNode<T>(string s_file, bool worldPositionStays = false) where T : Component
	{
		Transform parent = tfMainRoot;
		T val = LoadMgr.LoadRes<T>(s_file);
		if (val == null)
		{
			return null;
		}
		if (!parent)
		{
			parent = tfMainRoot;
		}
		val.transform.SetParent(parent, worldPositionStays);
		showingDialogs.Add(val.gameObject);
		return val;
	}

	public static T AddTopNode<T>(string s_file, bool worldPositionStays = false) where T : Component
	{
		Transform parent = tfTopRoot;
		T val = LoadMgr.LoadRes<T>(s_file);
		if (val == null)
		{
			return null;
		}
		if (!parent)
		{
			parent = tfMainRoot;
		}
		val.transform.SetParent(parent, worldPositionStays);
		showingDialogs.Add(val.gameObject);
		return val;
	}

	public static T AddSuperNode<T>(string s_file, bool worldPositionStays = false) where T : Component
	{
		Transform parent = tfSuperRoot;
		T val = LoadMgr.LoadRes<T>(s_file);
		if (val == null)
		{
			return null;
		}
		if (!parent)
		{
			parent = tfMainRoot;
		}
		val.transform.SetParent(parent, worldPositionStays);
		showingDialogs.Add(val.gameObject);
		return val;
	}

	public static T AddSceneNode<T>(string s_file, bool worldPositionStays = false) where T : Component
	{
		Transform parent = objCurScene.transform;
		T val = LoadMgr.LoadRes<T>(s_file);
		if (val == null)
		{
			return null;
		}
		if (!parent)
		{
			parent = tfMainRoot;
		}
		val.transform.SetParent(parent, worldPositionStays);
		showingDialogs.Add(val.gameObject);
		return val;
	}

	public static WaitingDialog OpenWaiting(float fDestroyTime = -1f, float fShowTime = -1f, Action callback = null, Action onIAPLoadingSuc = null)
	{
		if (pWaiting == null)
		{
			pWaiting = LoadMgr.LoadRes<WaitingDialog>("Prefab/WaitingDialog");
			pWaiting.transform.SetParent(tfTopRoot, false);
		}
		if (onIAPLoadingSuc == null)
		{
			pWaiting.Show(fDestroyTime, fShowTime, delegate
			{
				pWaiting = null;
				if (callback != null)
				{
					callback();
				}
			}, true);
		}
		else
		{
			pWaiting.OnIAPLoadingSuc = onIAPLoadingSuc;
			pWaiting.ShowIAPLoading(fDestroyTime, fShowTime, delegate
			{
				pWaiting = null;
				if (callback != null)
				{
					callback();
				}
			}, true);
		}
		return pWaiting;
	}

	public static void SetWaitingProgress(float va)
	{
		if (!(pWaiting == null))
		{
			pWaiting.Progress(va);
		}
	}

	public static void CloseWaiting()
	{
		if (!(pWaiting == null))
		{
			pWaiting._destroy();
			pWaiting = null;
		}
	}

	public static void RemoveDialog(GameObject obj)
	{
		if (showingDialogs.Contains(obj))
		{
			showingDialogs.Remove(obj);
		}
	}
}
