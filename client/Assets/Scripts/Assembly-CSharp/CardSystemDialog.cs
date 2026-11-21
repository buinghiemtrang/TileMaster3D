using UnityEngine;
using UnityEngine.UI;

public class CardSystemDialog : MonoBehaviour
{
	[SerializeField]
	private GameObject objLockView;

	[SerializeField]
	private GameObject objCardGroupView;

	[SerializeField]
	private GameObject objNotOpen;

	[SerializeField]
	private GameObject objLevelLock;

	[SerializeField]
	private MButton btnHint;

	[SerializeField]
	private Text _clockText;

	private void Awake()
	{
		btnHint.onClick.AddListener(delegate
		{
			SceneMgr.AddTopNode<CardSystemGuid>("Prefabs/CardSystem/CardSystemGuid");
		});
	}

	private void Start()
	{
		if (!cf_const.cardSystemClose)
		{
			_clockText.SetLBy(cf_const.cardSystemUnlockLevel);
			objNotOpen.SetActive(false);
			objLockView.SetActive(DB.User.Maxlv < cf_const.cardSystemUnlockLevel);
			objCardGroupView.SetActive(!objLockView.activeSelf);
			if (objCardGroupView.activeSelf && !CardSystemManager.I.IsGuid)
			{
				SceneMgr.AddTopNode<CardSystemGuid>("Prefabs/CardSystem/CardSystemGuid");
				CardSystemManager.I.ChangeGuidStage();
			}
		}
		else
		{
			objNotOpen.SetActive(true);
			objLevelLock.SetActive(false);
			objCardGroupView.SetActive(false);
		}
	}
}
