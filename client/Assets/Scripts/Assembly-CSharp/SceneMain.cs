using DG.Tweening;
using UnityEngine;

public class SceneMain : MonoBehaviour
{
	public static SceneMain ins;

	public Transform tfTop;

	public Transform tfBottom;

	[HideInInspector]
	public bool isCheckEject;

	public Transform goldIcon;

	public Transform energyIcon;

	private void Awake()
	{
		ins = this;
		DispatcherMgr.On<bool>("MainMenuMoveUp", MainMenuMoveUp);
		DB.User.Curlv = DB.User.Maxlv;
		isCheckEject = true;
	}

	private void Start()
	{
		Global.HideBanner();
	}

	private void OnDestroy()
	{
		ins = null;
	}

	private void MainMenuMoveUp(bool b_up)
	{
		if (b_up)
		{
			if (tfTop != null)
			{
				Vector3 localPosition = tfTop.localPosition;
				Rect rect = tfTop.Rect();
				tfTop.DOLocalMoveY(localPosition.y + rect.height + 200f, 0.3f);
			}
			if (tfBottom != null)
			{
				Vector3 localPosition2 = tfBottom.localPosition;
				Rect rect2 = tfBottom.Rect();
				tfBottom.DOLocalMoveY(localPosition2.y - rect2.height, 0.3f);
			}
		}
		else
		{
			if (tfTop != null)
			{
				Vector3 localPosition3 = tfTop.localPosition;
				tfTop.SetAnchorPosY(0f);
				tfTop.DOLocalMoveY(localPosition3.y, 0.3f).From();
			}
			if (tfBottom != null)
			{
				Vector3 localPosition4 = tfBottom.localPosition;
				tfBottom.SetAnchorPosY(0f);
				tfBottom.DOLocalMoveY(localPosition4.y, 0.3f).From();
			}
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (!pauseStatus && !Global.IsShowAdOrPurchase)
		{
			Singleton<NetworkInspectionManager>.I.CheckNetwork(null);
		}
	}
}
