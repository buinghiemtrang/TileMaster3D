using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExtraFoundation.Components
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(CanvasRenderer))]
	[AddComponentMenu("UI/Effects/UILineRenderer")]
	public class UILineRenderer : MaskableGraphic
	{
		public enum PositionType
		{
			Relative = 0,
			Absolute = 1
		}

		public enum JoinType
		{
			None = 0,
			Bevel = 1,
			Miter = 2
		}

		[Serializable]
		private struct Point
		{
			[SerializeField]
			private Vector2 position;

			[SerializeField]
			private Transform target;

			[SerializeField]
			private bool isTarget;

			public Vector2 Position
			{
				get
				{
					return position;
				}
				set
				{
					position = value;
				}
			}

			public Transform Target
			{
				get
				{
					return target;
				}
				set
				{
					target = value;
				}
			}

			public bool IsTarget
			{
				get
				{
					return isTarget;
				}
				set
				{
					isTarget = value;
				}
			}
		}

		private enum SegmentType
		{
			Start = 0,
			Middle = 1,
			End = 2
		}

		private const float MinMiterJoin = (float)Math.PI / 12f;

		private const float MinBevelNiceJoin = 0f;

		[SerializeField]
		private Texture texture;

		[SerializeField]
		private Rect uvRect = new Rect(0f, 0f, 1f, 1f);

		[SerializeField]
		private float lineWidth = 2f;

		[SerializeField]
		[Range(1f, 10f)]
		private float uvScaleX = 1f;

		[SerializeField]
		private List<Point> points = new List<Point>();

		[SerializeField]
		private bool lineList;

		[SerializeField]
		private bool lineCaps;

		[SerializeField]
		private JoinType lineJoin = JoinType.Bevel;

		[SerializeField]
		private PositionType posType;

		private Vector3 cachePos;

		private static readonly Vector2 UVTopLeft = Vector2.zero;

		private static readonly Vector2 UVBottomLeft = new Vector2(0f, 1f);

		private static readonly Vector2 UVTopCenter = new Vector2(0.5f, 0f);

		private static readonly Vector2 UVBottomCenter = new Vector2(0.5f, 1f);

		private static readonly Vector2 UVTopRight = new Vector2(1f, 0f);

		private static readonly Vector2 UVBottomRight = new Vector2(1f, 1f);

		private static readonly Vector2[] StartUvs = new Vector2[4] { UVTopLeft, UVBottomLeft, UVBottomCenter, UVTopCenter };

		private static readonly Vector2[] MiddleUvs = new Vector2[4] { UVTopLeft, UVBottomLeft, UVBottomRight, UVTopRight };

		private static readonly Vector2[] EndUvs = new Vector2[4] { UVTopCenter, UVBottomCenter, UVBottomRight, UVTopRight };

		public Texture MainTexture
		{
			get
			{
				return texture;
			}
			set
			{
				texture = value;
			}
		}

		public float LineWidth
		{
			get
			{
				return lineWidth;
			}
			set
			{
				lineWidth = value;
			}
		}

		public PositionType Space
		{
			get
			{
				return posType;
			}
			set
			{
				posType = value;
			}
		}

		public bool LineList
		{
			get
			{
				return lineList;
			}
			set
			{
				lineList = value;
			}
		}

		public bool LineCaps
		{
			get
			{
				return lineCaps;
			}
			set
			{
				lineCaps = value;
			}
		}

		public JoinType LineJoin
		{
			get
			{
				return lineJoin;
			}
			set
			{
				lineJoin = value;
			}
		}

		public override Texture mainTexture
		{
			get
			{
				if (!(texture == null))
				{
					return texture;
				}
				return Graphic.s_WhiteTexture;
			}
		}

		public Texture Texture
		{
			get
			{
				return texture;
			}
			set
			{
				if (!(texture == value))
				{
					texture = value;
					SetVerticesDirty();
					SetMaterialDirty();
				}
			}
		}

		public Rect UVRect
		{
			get
			{
				return uvRect;
			}
			set
			{
				if (!(uvRect == value))
				{
					uvRect = value;
					SetVerticesDirty();
				}
			}
		}

		public void AddVector2Point(Vector2 pos)
		{
			Point item = default(Point);
			item.Position = pos;
			item.IsTarget = false;
			points.Add(item);
		}

		public void AddTransformPoint(Transform trans)
		{
			Point item = default(Point);
			item.Target = trans;
			item.IsTarget = true;
			points.Add(item);
		}

		public void InsertVector2PointAt(int index, Vector2 pos)
		{
			Point item = default(Point);
			item.Position = pos;
			item.IsTarget = false;
			points.Insert(index, item);
		}

		public void RemovePointAt(int index)
		{
			if (index >= 0 && index < points.Count)
			{
				points.RemoveAt(index);
			}
		}

		public void ClearPoints()
		{
			points.Clear();
		}

		private List<Vector2> GetTruePoins()
		{
			List<Vector2> list = new List<Vector2>();
			if (posType == PositionType.Absolute)
			{
				Vector3 position = base.transform.position;
				for (int i = 0; i < points.Count; i++)
				{
					Point point = points[i];
					if (!point.IsTarget)
					{
						list.Add(new Vector2(point.Position.x - position.x, point.Position.y - position.y));
					}
					else if ((bool)point.Target)
					{
						Vector3 position2 = point.Target.position;
						list.Add(new Vector2(position2.x - position.x, position2.y - position.y));
					}
				}
			}
			else
			{
				for (int j = 0; j < points.Count; j++)
				{
					Point point2 = points[j];
					if (!point2.IsTarget)
					{
						list.Add(point2.Position);
					}
					else if ((bool)point2.Target)
					{
						Vector3 position3 = point2.Target.position;
						Vector3 position4 = base.transform.position;
						list.Add(new Vector2(position3.x - position4.x, position3.y - position4.y));
					}
				}
			}
			return list;
		}

		private List<UIVertex[]> LineListSegments(List<Vector2> points)
		{
			List<UIVertex[]> list = new List<UIVertex[]>();
			for (int i = 1; i < points.Count; i += 2)
			{
				Vector2 start = points[i - 1];
				Vector2 end = points[i];
				if (lineCaps)
				{
					list.Add(CreateLineCap(start, end, SegmentType.Start));
				}
				list.Add(CreateLineSegment(start, end, SegmentType.Middle));
				if (lineCaps)
				{
					list.Add(CreateLineCap(start, end, SegmentType.End));
				}
			}
			return list;
		}

		private List<UIVertex[]> StraightLineSegments(List<Vector2> points)
		{
			List<UIVertex[]> list = new List<UIVertex[]>();
			for (int i = 0; i < points.Count - 1; i++)
			{
				Vector2 start = points[i];
				Vector2 end = points[i + 1];
				if (i == 0 && lineCaps)
				{
					list.Add(CreateLineCap(start, end, SegmentType.Start));
				}
				list.Add(CreateLineSegment(start, end, SegmentType.Middle));
				if (lineCaps && i == points.Count - 2)
				{
					list.Add(CreateLineCap(start, end, SegmentType.End));
				}
			}
			return list;
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			if (points == null)
			{
				return;
			}
			vh.Clear();
			List<Vector2> truePoins = GetTruePoins();
			List<UIVertex[]> list = ((!lineList) ? StraightLineSegments(truePoins) : LineListSegments(truePoins));
			for (int i = 0; i < list.Count; i++)
			{
				if (!lineList && i < list.Count - 1 && lineJoin != JoinType.None)
				{
					SetJoinTypeMiterAndBevel(vh, i, list);
				}
				vh.AddUIVertexQuad(list[i]);
			}
		}

		private void SetJoinTypeNone(VertexHelper vh, int i, List<UIVertex[]> segments)
		{
		}

		private void SetJoinTypeMiterAndBevel(VertexHelper vh, int i, List<UIVertex[]> segments)
		{
			Vector3 vector = segments[i][1].position - segments[i][2].position;
			Vector3 vector2 = segments[i + 1][2].position - segments[i + 1][1].position;
			float num = Vector2.Angle(vector, vector2) * ((float)Math.PI / 180f);
			float num2 = Mathf.Sign(Vector3.Cross(vector.normalized, vector2.normalized).z);
			float num3 = lineWidth / (2f * Mathf.Tan(num / 2f));
			Vector3 position = segments[i][2].position - vector.normalized * num3 * num2;
			Vector3 position2 = segments[i][3].position + vector.normalized * num3 * num2;
			if (lineJoin == JoinType.Miter)
			{
				if (num3 < vector.magnitude / 2f && num3 < vector2.magnitude / 2f && num > (float)Math.PI / 12f)
				{
					segments[i][2].position = position;
					segments[i][3].position = position2;
					segments[i + 1][0].position = position2;
					segments[i + 1][1].position = position;
					return;
				}
			}
			else if (lineJoin != JoinType.Bevel)
			{
				return;
			}
			if (num3 < vector.magnitude / 2f && num3 < vector2.magnitude / 2f && num > 0f)
			{
				if (num2 < 0f)
				{
					segments[i][2].position = position;
					segments[i + 1][1].position = position;
				}
				else
				{
					segments[i][3].position = position2;
					segments[i + 1][0].position = position2;
				}
			}
			UIVertex[] verts = new UIVertex[4]
			{
				segments[i][2],
				segments[i][3],
				segments[i + 1][0],
				segments[i + 1][1]
			};
			vh.AddUIVertexQuad(verts);
		}

		private UIVertex[] CreateLineCap(Vector2 start, Vector2 end, SegmentType type)
		{
			switch (type)
			{
			case SegmentType.Start:
			{
				Vector2 start2 = start - (end - start).normalized * lineWidth / 2f;
				return CreateLineSegment(start2, start, SegmentType.Start);
			}
			case SegmentType.End:
			{
				Vector2 end2 = end + (end - start).normalized * lineWidth / 2f;
				return CreateLineSegment(end, end2, SegmentType.End);
			}
			default:
				Debug.LogError("Bad SegmentType passed in to CreateLineCap. Must be SegmentType.Start or SegmentType.End");
				return null;
			}
		}

		private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end, SegmentType type)
		{
			Vector2 vector = Utils.Vec2(start.y - end.y, end.x - start.x);
			Vector2[] array;
			switch (type)
			{
			case SegmentType.Start:
				array = StartUvs;
				break;
			case SegmentType.End:
				array = EndUvs;
				break;
			default:
			{
				array = MiddleUvs;
				float num = uvScaleX * Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y)) / (float)mainTexture.width;
				array[2].x = num;
				array[3].x = num;
				break;
			}
			}
			Vector2 vector2 = vector.normalized * lineWidth / 2f;
			Vector2[] vertices = new Vector2[4]
			{
				start - vector2,
				start + vector2,
				end + vector2,
				end - vector2
			};
			return SetVbo(vertices, array);
		}

		protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
		{
			UIVertex[] array = new UIVertex[4];
			for (int i = 0; i < vertices.Length; i++)
			{
				UIVertex simpleVert = UIVertex.simpleVert;
				simpleVert.color = color;
				simpleVert.position = vertices[i];
				simpleVert.uv0 = uvs[i];
				array[i] = simpleVert;
			}
			return array;
		}
	}
}
