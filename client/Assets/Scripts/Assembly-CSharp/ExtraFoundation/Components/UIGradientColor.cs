using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExtraFoundation.Components
{
	[AddComponentMenu("UI/Effects/GradientColor")]
	public class UIGradientColor : BaseMeshEffect
	{
		public enum Type
		{
			Vertical = 0,
			Horizontal = 1
		}

		public Type GradientType;

		[Range(-1f, 1f)]
		public float Offset;

		public Gradient gradient;

		private List<UIVertex> vertexList = new List<UIVertex>();

		public override void ModifyMesh(VertexHelper helper)
		{
			if (!IsActive() || helper.currentVertCount == 0)
			{
				return;
			}
			vertexList.Clear();
			helper.GetUIVertexStream(vertexList);
			int count = vertexList.Count;
			switch (GradientType)
			{
			case Type.Horizontal:
			{
				float num = vertexList[0].position.x;
				float num2 = vertexList[0].position.x;
				for (int num3 = count - 1; num3 >= 1; num3--)
				{
					float x = vertexList[num3].position.x;
					if (x > num2)
					{
						num2 = x;
					}
					else if (x < num)
					{
						num = x;
					}
				}
				float num4 = 1f / (num2 - num);
				UIVertex vertex = default(UIVertex);
				for (int i = 0; i < helper.currentVertCount; i++)
				{
					helper.PopulateUIVertex(ref vertex, i);
					vertex.color = gradient.Evaluate((vertex.position.x - num) * num4 - Offset);
					helper.SetUIVertex(vertex, i);
				}
				break;
			}
			case Type.Vertical:
			{
				float num5 = vertexList[0].position.y;
				float num6 = vertexList[0].position.y;
				for (int num7 = count - 1; num7 >= 1; num7--)
				{
					float y = vertexList[num7].position.y;
					if (y > num6)
					{
						num6 = y;
					}
					else if (y < num5)
					{
						num5 = y;
					}
				}
				float num8 = 1f / (num6 - num5);
				UIVertex vertex2 = default(UIVertex);
				for (int j = 0; j < helper.currentVertCount; j++)
				{
					helper.PopulateUIVertex(ref vertex2, j);
					vertex2.color = gradient.Evaluate((vertex2.position.y - num5) * num8 - Offset);
					helper.SetUIVertex(vertex2, j);
				}
				break;
			}
			}
		}
	}
}
