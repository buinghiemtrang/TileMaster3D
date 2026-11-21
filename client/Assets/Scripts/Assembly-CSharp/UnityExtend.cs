using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class UnityExtend
{
	public static RectTransform RT(this Component self)
	{
		return self.transform as RectTransform;
	}

	public static Rect Rect(this Component self)
	{
		return (self.transform as RectTransform).rect;
	}

	public static void SetWidth(this Transform self, float width)
	{
		// UNVERIFIED: decompiler lost the Vector2 build; width replaces sizeDelta.x, y kept (RVA 0x106fb78)
		RectTransform rectTransform = self as RectTransform;
		Vector2 sizeDelta = rectTransform.sizeDelta;
		sizeDelta.x = width;
		rectTransform.sizeDelta = sizeDelta;
	}

	public static void SetHeight(this Transform self, float height)
	{
		// UNVERIFIED: decompiler lost the Vector2 build; height replaces sizeDelta.y, x kept (RVA 0x106fbf4)
		RectTransform rectTransform = self as RectTransform;
		Vector2 sizeDelta = rectTransform.sizeDelta;
		sizeDelta.y = height;
		rectTransform.sizeDelta = sizeDelta;
	}

	public static void SetSize(this Transform self, float width, float height)
	{
		(self as RectTransform).sizeDelta = new Vector2(width, height);
	}

	public static void SetPivot(this Transform self, float x, float y)
	{
		(self as RectTransform).pivot = new Vector2(x, y);
	}

	public static float GetDistance(this Transform one, Transform two)
	{
		return Vector3.Distance(one.position, two.position);
	}

	public static void SetLocPos(this Transform self, float x, float y)
	{
		Vector3 localPosition = self.localPosition;
		self.localPosition = new Vector3(x, y, localPosition.z);
	}

	public static void SetLocPos(this Transform self, Vector2 vec2)
	{
		Vector3 localPosition = self.localPosition;
		self.localPosition = new Vector3(vec2.x, vec2.y, localPosition.z);
	}

	public static void SetLocPosX(this Transform self, float x)
	{
		Vector3 localPosition = self.localPosition;
		self.localPosition = new Vector3(x, localPosition.y, localPosition.z);
	}

	public static void Set3DLocPosX(this Transform self, float x)
	{
		Vector3 localPosition = self.localPosition;
		self.localPosition = new Vector3(x / 100f, localPosition.y, localPosition.z);
	}

	public static void SetLocPosY(this Transform self, float y)
	{
		Vector3 localPosition = self.localPosition;
		self.localPosition = new Vector3(localPosition.x, y, localPosition.z);
	}

	public static void SetLocPosZ(this Transform self, float z)
	{
		Vector3 localPosition = self.localPosition;
		self.localPosition = new Vector3(localPosition.x, localPosition.y, z);
	}

	public static void SetPos(this Transform self, float x, float y)
	{
		Vector3 position = self.position;
		self.position = new Vector3(x, y, position.z);
	}

	public static void SetAnchorPos(this Transform self, float x, float y)
	{
		(self as RectTransform).anchoredPosition = new Vector2(x, y);
	}

	public static void SetAnchorPos(this Transform self, Vector2 vec2)
	{
		(self as RectTransform).anchoredPosition = vec2;
	}

	public static void SetAnchorPosX(this Transform self, float x)
	{
		// UNVERIFIED: decompiler lost the Vector2 build; x replaces anchoredPosition.x, y kept (RVA 0x10700e4)
		RectTransform rectTransform = self as RectTransform;
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		anchoredPosition.x = x;
		rectTransform.anchoredPosition = anchoredPosition;
	}

	public static void SetAnchorPosY(this Transform self, float y)
	{
		// UNVERIFIED: decompiler lost the Vector2 build; y replaces anchoredPosition.y, x kept (RVA 0x1070160)
		RectTransform rectTransform = self as RectTransform;
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		anchoredPosition.y = y;
		rectTransform.anchoredPosition = anchoredPosition;
	}

	public static void SetRotation(this Transform self, float z)
	{
		Quaternion rotation = self.rotation;
		self.rotation = Quaternion.Euler(rotation.x, rotation.y, z);
	}

	public static void SetLocRotation(this Transform self, float z)
	{
		Quaternion localRotation = self.localRotation;
		self.localRotation = Quaternion.Euler(localRotation.x, localRotation.y, z);
	}

	public static void SetScale(this Transform self, float v)
	{
		Vector3 localScale = self.localScale;
		self.localScale = new Vector3(v, v, localScale.z);
	}

	public static void SetScale3(this Transform self, float v)
	{
		self.localScale = new Vector3(v, v, v);
	}

	public static Graphic SetAlpha(this Graphic self, float alpha)
	{
		Color color = self.color;
		color.a = alpha;
		self.color = color;
		return self;
	}

	public static void SetColors(this Transform self, Color color)
	{
		Graphic[] componentsInChildren = self.GetComponentsInChildren<Graphic>();
		foreach (Graphic graphic in componentsInChildren)
		{
			graphic.color = color;
		}
	}

	public static void SetShadowColors(this Text self, Color color)
	{
		Shadow[] componentsInChildren = self.GetComponentsInChildren<Shadow>();
		foreach (Shadow shadow in componentsInChildren)
		{
			shadow.effectColor = color;
		}
	}

	public static Vector3 ConvertToWorld(this Component self)
	{
		// UNVERIFIED: z of the ScreenToWorldPoint argument is lost by the decompiler; camera z vs self z distance assumed (RVA 0x1070530)
		Vector2 vector = RectTransformUtility.WorldToScreenPoint(Camera.main, self.transform.position);
		return Camera.main.ScreenToWorldPoint(new Vector3(vector.x, vector.y, self.transform.position.z - Camera.main.transform.position.z));
	}

	public static Vector3 ConvertToWorld(this Vector3 pos)
	{
		// UNVERIFIED: z of the ScreenToWorldPoint argument is lost by the decompiler; pos.z - camera z assumed (RVA 0x1070644)
		Vector2 vector = RectTransformUtility.WorldToScreenPoint(Camera.main, pos);
		return Camera.main.ScreenToWorldPoint(new Vector3(vector.x, vector.y, pos.z - Camera.main.transform.position.z));
	}

	public static T FindC<T>(this Component self, string name)
	{
		return self.transform.Find(name).GetComponent<T>();
	}

	public static T FindC<T>(this Transform self, string name)
	{
		return self.Find(name).GetComponent<T>();
	}

	public static Transform Find(this Component self, string name)
	{
		return self.transform.Find(name);
	}

	public static void DestroySelf(this Component self)
	{
		Object.Destroy(self.gameObject);
	}

	public static void DestroysInChildren(this Transform self)
	{
		foreach (Transform item in self)
		{
			item.DOKill();
			item.DestroySelf();
		}
	}

	public static bool IsActive(this Component self)
	{
		return self.gameObject.activeSelf;
	}

	public static void SetActive(this Component self, bool value)
	{
		self.gameObject.SetActive(value);
	}

	public static void Show(this Component self)
	{
		self.gameObject.SetActive(value: true);
	}

	public static void Hide(this Component self)
	{
		self.gameObject.SetActive(value: false);
	}

	public static string GetFullName(this Transform self)
	{
		List<string> list = new List<string>();
		while (self != null)
		{
			list.Add(self.name);
			self = self.parent;
		}
		list.Reverse();
		string text = "";
		foreach (string item in list)
		{
			text = text + item + "."; // [rebuild] separator "." suy từ literal gốc "Canvas (Environment)." (tên SL bị Ghidra mangle)
		}
		text = text.Substring(0, text.Length - 1);
		return text.Replace("Canvas (Environment).", "");
	}

	public static float ToAngle(this Transform self, Vector2 pos)
	{
		Vector3 vector = self.InverseTransformPoint(new Vector3(pos.x, pos.y, 0f));
		return Mathf.Atan(vector.y / vector.x) * 57.29578f + ((vector.x >= 0f) ? 90f : 270f);
	}
}
