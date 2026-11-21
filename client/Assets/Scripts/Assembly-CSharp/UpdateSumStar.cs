using System;
using UnityEngine;
using UnityEngine.UI;

public class UpdateSumStar : MonoBehaviour
{
	[SerializeField]
	private Text ttNum;

	[SerializeField]
	private Image imageIcon;

	protected void Start()
	{
		UpdateCount();
		if (!(imageIcon == null))
		{
			MButton component = imageIcon.gameObject.GetComponent<MButton>();
			if (component != null)
			{
				component.onClick.AddListener(OnClickIcon);
			}
		}
	}

	private void UpdateCount()
	{
		if (!(ttNum == null))
		{
			ttNum.text = DB.User.SumStar.ToString();
		}
	}

	private void Awake()
	{
		DispatcherMgr.On("Update_SumStar", new Action(UpdateCount));
		DispatcherMgr.On("Update_Account", new Action(UpdateCount));
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off("Update_SumStar", new Action(UpdateCount));
		DispatcherMgr.Off("Update_Account", new Action(UpdateCount));
	}

	private void OnClickIcon(MButton button)
	{
		Global.ShowBubbleTips("Language333", imageIcon.transform);
	}
}
