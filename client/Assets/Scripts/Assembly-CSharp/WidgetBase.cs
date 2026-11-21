using UnityEngine;

public class WidgetBase : MonoBehaviour
{
	public enum EnumMode
	{
		None = 0,
		ScaleX = 1,
		ScaleY = 2,
		ScaleMin = 3,
		ScaleMax = 4
	}

	public enum EnumRule
	{
		Uniform = 0,
		notUniform = 1,
		notUniformX = 2,
		notUniformY = 3
	}

	public EnumRule kScaleRule;

	public EnumMode kScaleMode;

	public EnumMode kSizeMode;

	private void Awake()
	{
		initScale();
		initSize();
	}

	private void GetScaleRatio(EnumMode kmode, out float scalex, out float scaley)
	{
		scalex = 1f;
		scaley = 1f;
		switch (kmode)
		{
		case EnumMode.ScaleX:
			scalex = SceneMgr.scale.x;
			if (kScaleRule == EnumRule.Uniform)
			{
				scaley = scalex;
			}
			break;
		case EnumMode.ScaleY:
			scaley = SceneMgr.scale.y;
			if (kScaleRule == EnumRule.Uniform)
			{
				scalex = scaley;
			}
			break;
		case EnumMode.ScaleMin:
			if (kScaleRule == EnumRule.notUniformY)
			{
				scaley = SceneMgr.scale.min;
			}
			else if (kScaleRule == EnumRule.notUniformX)
			{
				scalex = SceneMgr.scale.min;
			}
			else
			{
				scalex = SceneMgr.scale.min;
				scaley = SceneMgr.scale.min;
			}
			break;
		case EnumMode.ScaleMax:
			if (kScaleRule == EnumRule.notUniformY)
			{
				scaley = SceneMgr.scale.max;
			}
			else if (kScaleRule == EnumRule.notUniformX)
			{
				scalex = SceneMgr.scale.max;
			}
			else
			{
				scalex = SceneMgr.scale.max;
				scaley = SceneMgr.scale.max;
			}
			break;
		}
	}

	private void initScale()
	{
		if (kScaleMode != EnumMode.None)
		{
			float scalex;
			float scaley;
			GetScaleRatio(kScaleMode, out scalex, out scaley);
			// UNVERIFIED: pseudo-C loses the multiply; localScale.x *= scalex, .y *= scaley assumed by analogy with initSize (RVA 0x1165850)
			Vector3 localScale = base.transform.localScale;
			localScale.x *= scalex;
			localScale.y *= scaley;
			base.transform.localScale = localScale;
		}
	}

	private void initSize()
	{
		if (kSizeMode != EnumMode.None)
		{
			float scalex;
			float scaley;
			GetScaleRatio(kSizeMode, out scalex, out scaley);
			RectTransform rectTransform = (RectTransform)base.transform;
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x * scalex, rectTransform.sizeDelta.y * scaley);
		}
	}
}
