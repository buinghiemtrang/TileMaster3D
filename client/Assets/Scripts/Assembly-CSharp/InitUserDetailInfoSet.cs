using UnityEngine;
using UnityEngine.UI;

public class InitUserDetailInfoSet : MonoBehaviour
{
	[SerializeField]
	private Text textName;

	[SerializeField]
	private Text textMaxLv;

	[SerializeField]
	private Text textStar;

	[SerializeField]
	private Text textGold;

	[SerializeField]
	private Text textProp1;

	[SerializeField]
	private Text textProp2;

	[SerializeField]
	private Text textProp3;

	[SerializeField]
	private Text textProp4;

	[SerializeField]
	private Image loginPlatformImg;

	[SerializeField]
	private Sprite[] platformIcons;

	private void Awake()
	{
		DispatcherMgr.On("Update_UserData", this._Update_UserData);
		this._Update_UserData();
	}

	private void OnDisable()
	{
		DispatcherMgr.Off("Update_UserData", this._Update_UserData);
	}

	private void _Update_UserData()
	{
		if (DB.Account != null)
		{
			this.textName.text = DB.Account.GetName();
			this.textMaxLv.SetLBy(DB.User.Maxlv + 1);
			this.textStar.text = "x" + DB.User.SumStar.ToString();
			if (DB.Bag != null)
			{
				this.textGold.text = "x" + DB.Bag.GetProp(PropID.Money).ToString();
				this.textProp1.text = "x" + DB.Bag.GetProp(PropID.Back).ToString();
				this.textProp2.text = "x" + DB.Bag.GetProp(PropID.Refresh).ToString();
				this.textProp3.text = "x" + DB.Bag.GetProp(PropID.Freeze).ToString();
				this.textProp4.text = "x" + DB.Bag.GetProp(PropID.Hint).ToString();
			}
			if (this.loginPlatformImg != null)
			{
				this.loginPlatformImg.gameObject.SetActive(DataManager.I.bLogined);
				if (DataManager.I.bLogined && DataCenterLoginMgr.I != null)
				{
					int platformID = DataCenterLoginMgr.I.GetPlatformID();
					if (this.platformIcons != null && platformID + 1 >= 0 && platformID + 1 < this.platformIcons.Length)
					{
						this.loginPlatformImg.sprite = this.platformIcons[platformID + 1];
					}
				}
			}
		}
	}
}
