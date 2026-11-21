using FunctionEject;
using UnityEngine;
using UnityEngine.UI;

public class PigEntrance : EjectItem
{
	[SerializeField]
	private Sprite[] iconSprites;

	[SerializeField]
	private Image iconImg;

	private void Awake()
	{
		base.gameObject.SetActive(false);
		DispatcherMgr.On<GiftType>("BuySuccessCallBack", _IAPBuySuc);
	}

	public override void Init()
	{
		_RefreshUI();
	}

	public override bool Eject()
	{
		return false;
	}

	public void ClickEntrance(MButton btn)
	{
		SceneMgr.AddTopNode<Transform>("Prefabs/PigBankDialog");
		LogEvent.I.HomeIconClick("GoldPig");
	}

	private void _RefreshUI()
	{
		base.gameObject.SetActive(PigBankData.Instance.CanShow());
		iconImg.sprite = iconSprites[PigBankData.Instance.IsFull() ? 1 : 0];
	}

	private void _IAPBuySuc(GiftType type)
	{
		if (type == GiftType.Pig)
		{
			_RefreshUI();
		}
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _IAPBuySuc);
	}
}
