using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using frame8.Logic.Misc.Other.Extensions;

namespace Com.TheFallenGames.OSA.Core
{
	public class OSADebugger : MonoBehaviour, IDragHandler, IEventSystemHandler
	{
		public bool onlyAcceptedGameObjects;

		public string[] acceptedGameObjectsNames;

		private IOSA _AdapterImpl;

		public Text debugText1;

		public Text debugText2;

		public Text debugText3;

		public Text debugText4;

		public bool allowReinitializationWithOtherAdapter;

		private Toggle _EndStationary;

		private InputField _AmountInputField;

		private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private void Update()
		{
			if (_AdapterImpl == null)
			{
				return;
			}
			IList list = GetFieldValue("_VisibleItems") as IList;
			IList list2 = GetFieldValue("_RecyclableItems") as IList;
			IList list3 = GetFieldValue("_BufferredRecyclableItems") as IList;
			string text = null;
			if (list != null)
			{
				foreach (object item in list)
				{
					text = string.Concat(text, ((AbstractViewsHolder)item).ItemIndex + " ");
				}
			}
			string text2 = null;
			string text3 = null;
			if (list2 != null)
			{
				foreach (object item2 in list2)
				{
					AbstractViewsHolder abstractViewsHolder = item2 as AbstractViewsHolder;
					text2 = string.Concat(text2, abstractViewsHolder.ItemIndex + " ");
					text3 = string.Concat(text3, abstractViewsHolder.root.GetSiblingIndex() + " ");
				}
			}
			string text4 = null;
			if (list3 != null)
			{
				foreach (object item3 in list3)
				{
					AbstractViewsHolder abstractViewsHolder2 = item3 as AbstractViewsHolder;
					text4 = string.Concat(text4, abstractViewsHolder2.ItemIndex + " ");
					text3 = string.Concat(text3, abstractViewsHolder2.root.GetSiblingIndex() + "(" + abstractViewsHolder2.ItemIndex + ") ");
				}
			}
			Text obj = debugText1;
			string[] obj2 = new string[15];
			obj2[0] = "ctVrtIns: ";
			object internalStatePropertyValue = GetInternalStatePropertyValue("ctVirtualInsetFromVPS_Cached");
			obj2[1] = ((internalStatePropertyValue != null) ? internalStatePropertyValue.ToString() : null);
			obj2[2] = "\nctVrtSz: ";
			object internalStateFieldValue = GetInternalStateFieldValue("ctVirtualSize");
			obj2[3] = ((internalStateFieldValue != null) ? internalStateFieldValue.ToString() : null);
			obj2[4] = "\nvisCount: ";
			object propertyValue = GetPropertyValue("VisibleItemsCount");
			obj2[5] = ((propertyValue != null) ? propertyValue.ToString() : null);
			obj2[6] = "\nvis: ";
			obj2[7] = text;
			obj2[8] = "\nrec: ";
			obj2[9] = text2;
			obj2[10] = "\nbufRec: ";
			obj2[11] = text4;
			obj2[12] = "\nrecyclableSiblingIndices: ";
			obj2[13] = text3;
			obj2[14] = "\n";
			obj.text = string.Concat(obj2);
		}

		internal void InitWithAdapter(IOSA adapterImpl)
		{
			if ((_AdapterImpl != null && !allowReinitializationWithOtherAdapter) || (onlyAcceptedGameObjects && Array.IndexOf(acceptedGameObjectsNames, adapterImpl.gameObject.name) == -1))
			{
				return;
			}
			_AdapterImpl = adapterImpl;
			Button button;
			base.transform.GetComponentAtPath("ComputePanel/ComputeNowButton", out button);
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(delegate
			{
				Call("ComputeVisibilityForCurrentPosition", true, false);
			});
			base.transform.GetComponentAtPath("ComputePanel/ComputeNowButton_PlusDelta", out button);
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(delegate
			{
				Call("ComputeVisibilityForCurrentPositionRawParams", true, false, 0.1f);
			});
			base.transform.GetComponentAtPath("ComputePanel/ComputeNowButton_MinusDelta", out button);
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(delegate
			{
				Call("ComputeVisibilityForCurrentPositionRawParams", true, false, -0.1f);
			});
			base.transform.GetComponentAtPath("ComputePanel/CorrectNowButton", out button);
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(delegate
			{
				Call("CorrectPositionsOfVisibleItems", true, true);
			});
			base.transform.GetComponentAtPath("DataManipPanel/EndStationaryToggle", out _EndStationary);
			base.transform.GetComponentAtPath("DataManipPanel/AmountInputField", out _AmountInputField);
			Button button2;
			base.transform.GetComponentAtPath("DataManipPanel/head", out button2);
			button2.onClick.RemoveAllListeners();
			button2.onClick.AddListener(delegate
			{
				AddOrRemove(true);
			});
			Button button3;
			base.transform.GetComponentAtPath("DataManipPanel/tail", out button3);
			button3.onClick.RemoveAllListeners();
			button3.onClick.AddListener(delegate
			{
				AddOrRemove(false);
			});
		}

		private void AddOrRemove(bool atStart)
		{
			int num = int.Parse(_AmountInputField.text);
			int num2 = (atStart ? 0 : (_AdapterImpl.GetItemsCount() + ((num < 0) ? num : 0)));
			if (num < 0)
			{
				Call("RemoveItems", num2, -num, _EndStationary.isOn, false);
			}
			else
			{
				Call("InsertItems", num2, num, _EndStationary.isOn, false);
			}
		}

		private object GetFieldValue(string field)
		{
			return GetBaseType().GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(_AdapterImpl);
		}

		private object GetPropertyValue(string prop)
		{
			return GetBaseType().GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(_AdapterImpl, null);
		}

		private object GetInternalStateFieldValue(string field)
		{
			object fieldValue = GetFieldValue("_InternalState");
			return GetInternalStateBaseType(fieldValue).GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(fieldValue);
		}

		private object GetInternalStatePropertyValue(string prop)
		{
			object fieldValue = GetFieldValue("_InternalState");
			return GetInternalStateBaseType(fieldValue).GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(fieldValue, null);
		}

		private Type GetBaseType()
		{
			Type type = _AdapterImpl.GetType();
			while (!type.Name.Contains("OSA") || !type.IsGenericType)
			{
				type = type.BaseType;
				if (type == typeof(object))
				{
					return null;
				}
			}
			return type;
		}

		private Type GetInternalStateBaseType(object internalState)
		{
			Type type = internalState.GetType();
			while (!type.Name.Contains("InternalStateGeneric"))
			{
				type = type.BaseType;
				if (type == typeof(object))
				{
					return null;
				}
			}
			return type;
		}

		private void Call(string methodName, params object[] parameters)
		{
			if (_AdapterImpl != null)
			{
				Type baseType = GetBaseType();
				Type[] array = Array.ConvertAll(parameters, (object p) => p.GetType());
				baseType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, array, null).Invoke(_AdapterImpl, parameters);
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			base.transform.position += (Vector3)eventData.delta;
		}
	}
}
