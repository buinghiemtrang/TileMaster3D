using UnityEngine;
using UnityEngine.UI;

public class SelectMedalCelltem : MonoBehaviour
{
	[SerializeField]
	private Transform tfMask;

	[SerializeField]
	private Transform tfProgress;

	[SerializeField]
	private Image imageIcon;

	[SerializeField]
	private Image imageProgress;

	[SerializeField]
	private Image imageSelect;

	[SerializeField]
	private Image imageUsed;

	[SerializeField]
	private Text textProgress;

	[SerializeField]
	private MButton BtnClick;

	[SerializeField]
	private Image imageLock;

	[SerializeField]
	private GameObject redObj;

	private int groupIndex;

	private MedalData medalData;

	private void Awake()
	{
		if (imageSelect != null)
		{
			imageSelect.gameObject.SetActive(value: false);
		}
		if (tfMask != null)
		{
			tfMask.gameObject.SetActive(value: false);
		}
		if (tfProgress != null)
		{
			tfProgress.gameObject.SetActive(value: false);
		}
		if (imageUsed != null)
		{
			imageUsed.gameObject.SetActive(value: false);
		}
		if (BtnClick != null)
		{
			BtnClick.onClick.AddListener(OnClickCellItem);
		}
	}

	public void Init(int groupIndex, MedalData data)
	{
		this.groupIndex = groupIndex;
		medalData = data;
		UpdateUI();
	}

	private void UpdateUI()
	{
		redObj.SetActive(DB.Bag.NewGetMedals.Contains(medalData.cfg.ID));
		imageSelect.gameObject.SetActive(medalData.cfg.ID == MedalMgr.I.CurSelectId);
		imageUsed.gameObject.SetActive(medalData.cfg.ID == DB.User.Medal);
		imageIcon.sprite = LoadMgr.LoadSprite("MedalIcon/" + medalData.cfg.Icon);
		bool flag = IsUnlock();
		tfMask.gameObject.SetActive(!flag);
		if (!flag)
		{
			bool flag2 = medalData.cfg.Type == 1 && medalData.cfg.Lv == DB.User.LvMedal + 1;
			tfProgress.gameObject.SetActive(flag2);
			if (imageLock != null)
			{
				imageLock.gameObject.SetActive(!flag2);
			}
			if (flag2)
			{
				int sumStar = DB.User.SumStar;
				int count = medalData.cfg.Count;
				textProgress.text = string.Format("{0}/{1}", DB.User.SumStar, medalData.cfg.Count);
				imageProgress.fillAmount = (float)sumStar / (float)count;
			}
		}
	}

	private bool IsUnlock()
	{
		if (medalData.cfg.Type == 2)
		{
			return DB.Bag.Medal.Contains(medalData.cfg.ID);
		}
		if (medalData.cfg.Type == 1 && medalData.cfg.Lv <= DB.User.LvMedal)
		{
			return true;
		}
		return false;
	}

	public void OnClickCellItem(MButton button)
	{
		if (MedalGuideInside.I != null)
		{
			MedalGuideInside.I.DestroySelf();
			MedalGuideInside.I = null;
		}
		MedalMgr.I.CurSelectId = medalData.cfg.ID;
		if (redObj.activeSelf)
		{
			redObj.SetActive(value: false);
			DB.Bag.NewGetMedals.Remove(medalData.cfg.ID);
			DispatcherMgr.Emit("Update_User_MedalRed");
		}
		DispatcherMgr.Emit("Show_MedalDesc", groupIndex, medalData.cellIndex);
	}
}
