using UnityEngine;
using UnityEngine.UI;

public class UnregisterGraphicRaycast : MonoBehaviour
{
	private Graphic[] _allGraphics;

	protected void Awake()
	{
		_allGraphics = base.gameObject.GetComponentsInChildren<Graphic>();
	}

	protected void onShow()
	{
		for (int i = 0; i < _allGraphics.Length; i++)
		{
			Graphic graphic = _allGraphics[i];
			if (!graphic.raycastTarget)
			{
				GraphicRegistry.UnregisterGraphicForCanvas(graphic.canvas, graphic);
			}
		}
	}
}
