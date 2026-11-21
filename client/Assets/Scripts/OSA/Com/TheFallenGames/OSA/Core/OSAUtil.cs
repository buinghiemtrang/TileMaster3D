using System;
using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;

namespace Com.TheFallenGames.OSA.Core
{
	public class OSAUtil
	{
		public static bool DotNETCoreCompat_IsAssignableFrom(Type to, Type from)
		{
			return to.IsAssignableFrom(from);
		}

		public static ScrollbarFixer8 ConfigureDinamicallyCreatedScrollbar(Scrollbar scrollbar, IOSA iAdapter, RectTransform viewport, bool addFixerIfDoesntExist = true)
		{
			ScrollbarFixer8 component = scrollbar.GetComponent<ScrollbarFixer8>();
			if (!component)
			{
				if (!addFixerIfDoesntExist)
				{
					return null;
				}
				component = scrollbar.gameObject.AddComponent<ScrollbarFixer8>();
			}
			component.externalScrollRectProxy = iAdapter;
			component.viewport = viewport;
			return component;
		}
	}
}
