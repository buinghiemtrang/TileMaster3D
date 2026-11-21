using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class UpdateMedal : MonoBehaviour
{
	public Image imageMedalIcon;

	[CanBeNull]
	public Transform tfMedalAnim;

	public bool bIsShowAnmim;

	private void Awake()
	{
		DispatcherMgr.On("Update_User_Medal", Refresh);
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off("Update_User_Medal", Refresh);
	}

	private void Start()
	{
		Refresh();
	}

	private void Refresh()
	{
		cfg_medal cfgById = MedalMgr.I.GetCfgById(DB.User.Medal);
		if (cfgById == null)
		{
			return;
		}
		bool flag = bIsShowAnmim && !string.IsNullOrEmpty(cfgById.IconPrefab);
		if (imageMedalIcon != null)
		{
			imageMedalIcon.gameObject.SetActive(!flag);
		}
		if (tfMedalAnim != null)
		{
			tfMedalAnim.gameObject.SetActive(flag);
		}
		if (flag)
		{
			if (tfMedalAnim != null)
			{
				tfMedalAnim.DestroysInChildren();
				SceneMgr.AddNode<Transform>("Prefabs/Medal/IconPrefab/" + cfgById.IconPrefab, tfMedalAnim);
			}
		}
		else if (imageMedalIcon != null)
		{
			imageMedalIcon.sprite = LoadMgr.LoadSprite("MedalIcon/" + cfgById.Icon);
			imageMedalIcon.SetNativeSize();
		}
	}
}
