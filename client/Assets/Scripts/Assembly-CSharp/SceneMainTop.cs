using System;
using UnityEngine;

public class SceneMainTop : MonoBehaviour
{
	public UpdateEnergy pEnergy;

	public UpdateProp pPropGold;

	private void OnEnable()
	{
		DispatcherMgr.On("Update_Account", new Action(UpdateData));
	}

	private void OnDisable()
	{
		DispatcherMgr.Off("Update_Account", new Action(UpdateData));
	}

	private void UpdateData()
	{
		pEnergy.Refresh();
		pPropGold.Refresh();
	}
}
