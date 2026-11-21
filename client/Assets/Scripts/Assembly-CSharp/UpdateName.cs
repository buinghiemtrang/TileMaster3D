using System;
using UnityEngine;
using UnityEngine.UI;

public class UpdateName : MonoBehaviour
{
	public Text nameTex;

	private void Awake()
	{
		DispatcherMgr.On("Update_User_Name", new Action(_refresh));
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off("Update_User_Name", new Action(_refresh));
	}

	private void Start()
	{
		_refresh();
	}

	private void _refresh()
	{
		nameTex.text = DB.Account.GetName();
	}
}
