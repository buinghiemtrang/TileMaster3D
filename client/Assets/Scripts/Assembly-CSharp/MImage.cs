using UnityEngine.UI;

public class MImage : Image
{
	protected override void OnTransformParentChanged()
	{
		base.OnTransformParentChanged();
		if (!base.raycastTarget)
		{
			GraphicRegistry.UnregisterGraphicForCanvas(base.canvas, this);
		}
	}

	protected override void OnCanvasHierarchyChanged()
	{
		base.OnCanvasHierarchyChanged();
		if (!base.raycastTarget)
		{
			GraphicRegistry.UnregisterGraphicForCanvas(base.canvas, this);
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!base.raycastTarget)
		{
			GraphicRegistry.UnregisterGraphicForCanvas(base.canvas, this);
		}
	}
}
