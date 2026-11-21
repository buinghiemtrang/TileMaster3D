using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/ToJ Effects/Character Spacing", 7)]
[RequireComponent(typeof(Text))]
public class CharacterSpacing : BaseMeshEffect
{
	private const string REGEX_TAGS = "<b>|</b>|<i>|</i>|<size=.*?>|</size>|<color=.*?>|</color>|<material=.*?>|</material>";

	[SerializeField]
	private float m_Offset;

	public float offset
	{
		get
		{
			return m_Offset;
		}
		set
		{
			if (m_Offset != value)
			{
				m_Offset = value;
				if (base.graphic != null)
				{
					base.graphic.SetVerticesDirty();
				}
			}
		}
	}

	protected CharacterSpacing()
	{
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (!IsActive())
		{
			return;
		}
		List<UIVertex> list = new List<UIVertex>();
		vh.GetUIVertexStream(list);
		Text component = GetComponent<Text>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < component.cachedTextGenerator.lineCount; i++)
		{
			int startCharIdx = component.cachedTextGenerator.lines[i].startCharIdx;
			int num = ((i < component.cachedTextGenerator.lineCount - 1) ? component.cachedTextGenerator.lines[i + 1].startCharIdx : component.text.Length);
			list2.Add(component.text.Substring(startCharIdx, num - startCharIdx));
		}
		float num2 = m_Offset;
		int fontSize = component.fontSize;
		float num3 = 0f;
		switch (component.alignment)
		{
		case TextAnchor.LowerLeft:
		case TextAnchor.MiddleLeft:
		case TextAnchor.UpperLeft:
			num3 = 0f;
			break;
		case TextAnchor.LowerCenter:
		case TextAnchor.MiddleCenter:
		case TextAnchor.UpperCenter:
			num3 = 0.5f;
			break;
		case TextAnchor.LowerRight:
		case TextAnchor.MiddleRight:
		case TextAnchor.UpperRight:
			num3 = 1f;
			break;
		}
		int num4 = 0;
		num2 = num2 * (float)fontSize / 100f;
		IEnumerator enumerator = null;
		Match match = null;
		for (int j = 0; j < list2.Count; j++)
		{
			string text = list2[j];
			int lengthWithoutTags = text.Length;
			if (component.cachedTextGenerator.characterCountVisible - num4 < text.Length)
			{
				int num5 = component.cachedTextGenerator.characterCountVisible - num4;
				text = text.Substring(0, num5) + " ";
				lengthWithoutTags = num5 + 1;
			}
			if (component.supportRichText)
			{
				MatchCollection regexMatchedTags = GetRegexMatchedTags(text, out lengthWithoutTags);
				enumerator = regexMatchedTags.GetEnumerator();
				match = (enumerator.MoveNext() ? ((Match)enumerator.Current) : null);
			}
			int num6;
			if (list2[j].Length < 1)
			{
				num6 = 0;
			}
			else if (list2[j][list2[j].Length - 1] == ' ')
			{
				num6 = 1;
			}
			else
			{
				num6 = ((list2[j][list2[j].Length - 1] == '\n') ? 1 : 0);
			}
			int k = 0;
			if (text.Length > 0)
			{
				float num7 = 0f;
				int num8 = num6 - lengthWithoutTags;
				do
				{
					if (component.supportRichText && match != null && match.Index == k)
					{
						int num9 = match.Length - 1;
						num7 -= 1f;
						k += num9;
						num4 += num9;
						match = (enumerator.MoveNext() ? ((Match)enumerator.Current) : null);
					}
					int num10 = num4 * 6;
					if (list.Count <= num10 + 5)
					{
						break;
					}
					float num11 = num3 * num2 * (float)(num8 + 1) + num2 * num7;
					for (int l = 0; l < 6; l++)
					{
						UIVertex value = list[num10 + l];
						value.position += Vector3.right * num11;
						list[num10 + l] = value;
					}
					num7 += 1f;
					k++;
					num4++;
				}
				while (k < text.Length);
			}
			if (k < text.Length)
			{
				break;
			}
		}
		vh.Clear();
		vh.AddUIVertexTriangleStream(list);
	}

	private MatchCollection GetRegexMatchedTags(string text, out int lengthWithoutTags)
	{
		MatchCollection matchCollection = Regex.Matches(text, "<b>|</b>|<i>|</i>|<size=.*?>|</size>|<color=.*?>|</color>|<material=.*?>|</material>");
		lengthWithoutTags = 0;
		int num = 0;
		foreach (Match item in matchCollection)
		{
			num += item.Length;
		}
		lengthWithoutTags = text.Length - num;
		return matchCollection;
	}
}
