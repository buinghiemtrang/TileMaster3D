using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnloadUnusedAssets : MonoBehaviour
{
	private void Awake()
	{
		DispatcherMgr.OnG<bool>("Unload_Unused_Assets", _Unload);
	}

	private void OnDestroy()
	{
		DispatcherMgr.OffG<bool>("Unload_Unused_Assets", _Unload);
	}

	private void _Unload(bool isImmediately)
	{
		if (!isImmediately)
		{
			StartCoroutine("Unload");
			return;
		}
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	private IEnumerator Unload()
	{
		yield return new WaitForFixedUpdate();
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}
}
