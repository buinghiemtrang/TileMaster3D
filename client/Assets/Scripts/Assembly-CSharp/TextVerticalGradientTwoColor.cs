using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextVerticalGradientTwoColor : BaseMeshEffect
{
	public Color colorTop;

	public Color colorBottom;

	protected TextVerticalGradientTwoColor()
	{
		colorTop = new Color(1f, 0f, 0f, 1f);
		colorBottom = new Color(0f, 1f, 0f, 1f);
	}

	private static void setColor(List<UIVertex> verts, int index, Color32 c)
	{
		if (verts != null)
		{
			UIVertex value = verts[index];
			value.color = c;
			verts[index] = value;
		}
	}

	private void ModifyVertices(List<UIVertex> verts)
	{
		if (verts != null)
		{
			for (int i = 0; i < verts.Count; i += 6)
			{
				setColor(verts, i, colorTop);
				setColor(verts, i + 1, colorTop);
				setColor(verts, i + 2, colorBottom);
				setColor(verts, i + 3, colorBottom);
				setColor(verts, i + 4, colorBottom);
				setColor(verts, i + 5, colorTop);
			}
		}
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (IsActive())
		{
			List<UIVertex> list = new List<UIVertex>(vh.currentVertCount);
			vh.GetUIVertexStream(list);
			ModifyVertices(list);
			vh.Clear();
			vh.AddUIVertexTriangleStream(list);
		}
	}
}
