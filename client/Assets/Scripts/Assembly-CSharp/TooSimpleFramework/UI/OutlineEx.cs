using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TooSimpleFramework.UI
{
	public class OutlineEx : BaseMeshEffect
	{
		[SerializeField]
		private Color _OutlineColor = Color.white;

		[Range(1f, 6f)]
		[SerializeField]
		private float _OutlineWidth = 1f;

		[SerializeField]
		private bool m_UseGraphicAlpha = true;

		private static List<UIVertex> m_VetexList = new List<UIVertex>();

		public Color OutlineColor
		{
			get
			{
				return _OutlineColor;
			}
			set
			{
				if (_OutlineColor != value)
				{
					_OutlineColor = value;
					_Refresh();
				}
			}
		}

		public float OutlineWidth
		{
			get
			{
				return _OutlineWidth;
			}
			set
			{
				if (_OutlineWidth != value)
				{
					_OutlineWidth = value;
					_Refresh();
				}
			}
		}

		protected override void Start()
		{
			base.Start();
			Shader shader = Shader.Find("TSF Shaders/UI/OutlineEx");
			base.graphic.material = new Material(shader);
			AdditionalCanvasShaderChannels additionalShaderChannels = base.graphic.canvas.additionalShaderChannels;
			if ((additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord1) != AdditionalCanvasShaderChannels.TexCoord1)
			{
				base.graphic.canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
			}
			if ((additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord2) != AdditionalCanvasShaderChannels.TexCoord2)
			{
				base.graphic.canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord2;
			}
			if (m_UseGraphicAlpha)
			{
				base.graphic.material.EnableKeyword("_CANVAS_GROUP_COMPATIBLE_ON");
			}
			else
			{
				base.graphic.material.DisableKeyword("_CANVAS_GROUP_COMPATIBLE_ON");
			}
			_Refresh();
		}

		private void _Refresh()
		{
			base.graphic.material.SetColor("_OutlineColor", _OutlineColor);
			base.graphic.material.SetFloat("_OutlineWidth", _OutlineWidth);
			base.graphic.SetVerticesDirty();
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			vh.GetUIVertexStream(m_VetexList);
			_ProcessVertices();
			vh.Clear();
			vh.AddUIVertexTriangleStream(m_VetexList);
		}

		private void _ProcessVertices()
		{
			int i = 0;
			for (int num = m_VetexList.Count - 3; i <= num; i += 3)
			{
				UIVertex pVertex = m_VetexList[i];
				UIVertex pVertex2 = m_VetexList[i + 1];
				UIVertex pVertex3 = m_VetexList[i + 2];
				Vector2 pTriangleX = new Vector2(pVertex2.position.x - pVertex.position.x, pVertex2.position.y - pVertex.position.y).normalized;
				Vector2 pTriangleY = new Vector2(pVertex3.position.x - pVertex2.position.x, pVertex3.position.y - pVertex2.position.y).normalized;
				Vector2 pPosCenter = default(Vector2);
				pPosCenter.x = (_Min(pVertex.position.x, pVertex2.position.x, pVertex3.position.x) + _Max(pVertex.position.x, pVertex2.position.x, pVertex3.position.x)) * 0.5f;
				pPosCenter.y = (_Min(pVertex.position.y, pVertex2.position.y, pVertex3.position.y) + _Max(pVertex.position.y, pVertex2.position.y, pVertex3.position.y)) * 0.5f;
				Vector2 vector = _Min(pVertex.uv0, pVertex2.uv0, pVertex3.uv0);
				Vector2 vector2 = _Max(pVertex.uv0, pVertex2.uv0, pVertex3.uv0);
				Vector4 pUVOrigin = new Vector4(vector.x, vector.y, vector2.x, vector2.y);
				m_VetexList[i] = _SetNewPosAndUV(pVertex, _OutlineWidth, pPosCenter, pTriangleX, pTriangleY, Vector2.zero, Vector2.zero, pUVOrigin);
				m_VetexList[i + 1] = _SetNewPosAndUV(pVertex2, _OutlineWidth, pPosCenter, pTriangleX, pTriangleY, Vector2.zero, Vector2.zero, pUVOrigin);
				m_VetexList[i + 2] = _SetNewPosAndUV(pVertex3, _OutlineWidth, pPosCenter, pTriangleX, pTriangleY, Vector2.zero, Vector2.zero, pUVOrigin);
			}
		}

		private static UIVertex _SetNewPosAndUV(UIVertex pVertex, float pOutLineWidth, Vector2 pPosCenter, Vector2 pTriangleX, Vector2 pTriangleY, Vector2 pUVX, Vector2 pUVY, Vector4 pUVOrigin)
		{
			Vector3 position = pVertex.position;
			float num = ((position.x > pPosCenter.x) ? pOutLineWidth : (0f - pOutLineWidth));
			float num2 = ((position.y > pPosCenter.y) ? pOutLineWidth : (0f - pOutLineWidth));
			position.x += num;
			position.y += num2;
			pVertex.position = position;
			pVertex.uv0 = new Vector4(pVertex.uv0.x, pVertex.uv0.y, 0f, 0f);
			pVertex.uv1 = new Vector4(pUVOrigin.x, pUVOrigin.y, 0f, 0f);
			pVertex.uv2 = new Vector4(pUVOrigin.z, pUVOrigin.w, 0f, 0f);
			return pVertex;
		}

		private static float _Min(float pA, float pB, float pC)
		{
			return Mathf.Min(Mathf.Min(pA, pB), pC);
		}

		private static float _Max(float pA, float pB, float pC)
		{
			return Mathf.Max(Mathf.Max(pA, pB), pC);
		}

		private static Vector2 _Min(Vector2 pA, Vector2 pB, Vector2 pC)
		{
			return new Vector2(_Min(pA.x, pB.x, pC.x), _Min(pA.y, pB.y, pC.y));
		}

		private static Vector2 _Max(Vector2 pA, Vector2 pB, Vector2 pC)
		{
			return new Vector2(_Max(pA.x, pB.x, pC.x), _Max(pA.y, pB.y, pC.y));
		}
	}
}
