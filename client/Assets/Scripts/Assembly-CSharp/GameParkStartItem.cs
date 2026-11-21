using System;
using UnityEngine;
using UnityEngine.UI;

public class GameParkStartItem : MonoBehaviour
{
	[SerializeField]
	private GameParkType type;

	[SerializeField]
	private DialogShow pDialog;

	public Text count;

	public GameObject countNodeObj;

	public MButton playBtn;

	public MButton freeBtn;

	private void Awake()
	{
		base.gameObject.SetActive(DB.User.Maxlv >= cf_const.parkOneUnlockLv[(int)type]);
	}

	private void Start()
	{
		RefreshTickets();
		GameParkTickets parkTickets = DB.Other.GetParkTickets(type);
		count.text = parkTickets.playCount.ToString();
		freeBtn.gameObject.SetActive(true);
		freeBtn.interactable = parkTickets.playCount > 0;
		countNodeObj.SetActive(parkTickets.playCount > 0);
		freeBtn.onClick.AddListener(FreeBtnClick);
		playBtn.gameObject.SetActive(false);
	}

	private void RefreshTickets()
	{
		GameParkTickets parkTickets = DB.Other.GetParkTickets(type);
		int dayOfYear = DateTime.Now.DayOfYear;
		if (dayOfYear != parkTickets.day)
		{
			parkTickets.playCount = cf_const.parkPlayDayCount;
			parkTickets.day = dayOfYear;
			DB.Other.SetParkTickets(type.ToString(), parkTickets);
		}
		parkTickets.playCount = Mathf.Clamp(parkTickets.playCount, 0, cf_const.parkPlayDayCount);
	}

	private void FreeBtnClick(MButton btn)
	{
		GameParkTickets parkTickets = DB.Other.GetParkTickets(type);
		parkTickets.playCount--;
		DB.Other.SetParkTickets(type.ToString(), parkTickets);
		SceneMgr.LoadScene(string.Format("Game{0}Scene", type));
		pDialog.ClickClose();
	}

	private void PlayBtnClick(MButton btn)
	{
		Global.ShowRewardedVideo(delegate(string info)
		{
			if (info.Equals("succ"))
			{
				GameParkTickets parkTickets = DB.Other.GetParkTickets(type);
				parkTickets.playCount--;
				DB.Other.SetParkTickets(type.ToString(), parkTickets);
				SceneMgr.LoadScene(string.Format("Game{0}Scene", type));
				pDialog.ClickClose();
			}
		}, REWARD_PLACE.AdMagicCube);
	}
}
