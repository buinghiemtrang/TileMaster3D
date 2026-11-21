using UnityEngine;
using UnityEngine.UI;

public class UpdateAchive : MonoBehaviour
{
	public Image imgIco;

	private void Awake()
	{
		DispatcherMgr.On("Update_Achive_Title", Refresh);
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off("Update_Achive_Title", Refresh);
	}

	private void Start()
	{
		Refresh();
	}

	public void Refresh()
	{
		cfg_achiveScore achiveScoreCfgByReachCount = AchiveMgr.I.GetAchiveScoreCfgByReachCount(AchiveMgr.I.GetCurAchieveReachCount());
		imgIco.sprite = LoadMgr.LoadSprite("AchiveIcon/" + achiveScoreCfgByReachCount.Icon);
	}
}
