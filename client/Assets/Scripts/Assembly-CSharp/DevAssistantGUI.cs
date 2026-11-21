using System;
using System.Collections.Generic;
using UnityEngine;

public class DevAssistantGUI : MonoBehaviour
{
	public delegate void StringDelegate(string str);

	private int i;

	private GameObject _logPrefabs;

	private int _clearServerNeedClick;

	private int _regionRankStartGroupIndex;

	private float m_LastUpdateShowTime;

	private float m_UpdateShowDeltaTime;

	private int m_FrameUpdate;

	private float m_FPS;

	private int HorizontalCount;

	private int VerticalCount;

	private int x;

	private int y;

	private int w;

	private int h;

	private int index;

	private Dictionary<int, bool> enableList;

	private GUIStyle fontStyle;

	private GUIStyle fontStyle2;

	private bool isShow;

	private Dictionary<string, string> dicString;

	private Color[] colors;

	public static bool IsUseTestBannerId => false;

	private void Awake()
	{
	}

	private void OnGUI()
	{
	}

	private bool GetEnable(int index)
	{
		return false;
	}

	private void SwitchEnable(int index)
	{
	}

	private void Start()
	{
	}

	public List<string> Static_ReadTxtFormUnity(string itemName)
	{
		return null;
	}

	public void Static_SaveTxtFormUnity(string itemName, List<string> content)
	{
	}

	private void Update()
	{
	}

	public Rect getPos(int index)
	{
		return default;
	}

	public Rect getPos2(int index)
	{
		return default;
	}

	private void MyInput(StringDelegate fun, string buttonDes)
	{
	}

	private void Label(string Des)
	{
	}

	private void Button(Action fun, string buttonDes, string log = null)
	{
	}

	private void ButtonSwitch(Action fun, string buttonDes, string log = null)
	{
	}
}
