using System.Collections.Generic;
using UnityEngine;

namespace frame8.Logic.Misc.Other.Extensions
{
	public static class TransformExtensions
	{
		public static void GetComponentAtPath<T>(this Transform transform, string path, out T foundComponent)
		{
			Transform transform2;
			if (path == null)
			{
				foreach (Transform item in transform)
				{
					T component = item.GetComponent<T>();
					if (component != null)
					{
						foundComponent = component;
						return;
					}
				}
				transform2 = null;
			}
			else
			{
				transform2 = transform.Find(path);
			}
			if (transform2 == null)
			{
				foundComponent = default(T);
			}
			else
			{
				foundComponent = transform2.GetComponent<T>();
			}
		}

		public static T GetComponentAtPath<T>(this Transform transform, string path)
		{
			transform.GetComponentAtPath(path, out T foundComponent);
			return foundComponent;
		}

		public static Transform[] GetChildren(this Transform tr)
		{
			int childCount = tr.childCount;
			Transform[] array = new Transform[childCount];
			for (int i = 0; i < childCount; i++)
			{
				array[i] = tr.GetChild(i);
			}
			return array;
		}

		public static List<T> GetComponentsInDirectChildren<T>(this Transform tr)
		{
			List<T> list = new List<T>();
			Transform[] children = tr.GetChildren();
			for (int i = 0; i < children.Length; i++)
			{
				T component = children[i].GetComponent<T>();
				if (component != null)
				{
					list.Add(component);
				}
			}
			return list;
		}

		public static void GetEnoughChildrenToFitInArray(this Transform tr, Transform[] array)
		{
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				array[i] = tr.GetChild(i);
			}
		}

		public static List<Transform> GetDescendants(this Transform tr)
		{
			Transform[] children = tr.GetChildren();
			List<Transform> list = new List<Transform>();
			list.AddRange(children);
			int num = children.Length;
			for (int i = 0; i < num; i++)
			{
				list.AddRange(children[i].GetDescendants());
			}
			return list;
		}

		public static void GetDescendantsAndRelativePaths(this Transform tr, ref Dictionary<Transform, string> mapDescendantToPath)
		{
			tr.GetDescendantsAndRelativePaths("", ref mapDescendantToPath);
		}

		private static void GetDescendantsAndRelativePaths(this Transform tr, string currentPath, ref Dictionary<Transform, string> mapDescendantToPath)
		{
			Transform[] children = tr.GetChildren();
			int num = children.Length;
			for (int i = 0; i < num; i++)
			{
				Transform transform = children[i];
				string text = currentPath + "/" + transform.name;
				mapDescendantToPath[transform] = text;
				transform.GetDescendantsAndRelativePaths(text, ref mapDescendantToPath);
			}
		}

		public static int GetNumberOfAncestors(this Transform tr)
		{
			int num = 0;
			while ((bool)(tr = tr.parent))
			{
				num++;
			}
			return num;
		}
	}
}
