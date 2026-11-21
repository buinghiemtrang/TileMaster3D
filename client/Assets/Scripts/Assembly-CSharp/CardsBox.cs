using UnityEngine;
using UnityEngine.UI;

public class CardsBox : MonoBehaviour
{
	[SerializeField]
	private MButton _close;

	[SerializeField]
	private Text _name;

	[SerializeField]
	private MButton _stroe;

	[SerializeField]
	private RewardItem[] _rewards;

	[SerializeField]
	private MButton _getRewardButton;

	[SerializeField]
	private MButton _truenLeft;

	[SerializeField]
	private MButton _turnRight;

	[SerializeField]
	private CardSystemCardsGridController _cardListView;

	[SerializeField]
	private DialogShow _cardInfoView;

	[SerializeField]
	private CardItem _showItem;

	[SerializeField]
	private Text _cardGroupDec;

	private CardSystemGroup _cardGroup;

	private DialogShow _pDialog;

	private void Awake()
	{
		_pDialog = GetComponent<DialogShow>();
		DispatcherMgr.On<CardSystemCards>("ShowCardInfo", ShowCardInfo);
		_stroe.onClick.AddListener(delegate
		{
			SceneMgr.AddTopNode<StoreDialog>("Prefabs/StorePopDialog");
		});
		_truenLeft.onClick.AddListener(TurnLeft);
		_turnRight.onClick.AddListener(TurnRight);
		_getRewardButton.onClick.AddListener(GetReward);
		_close.onClick.AddListener(CloseBox);
		UpdateInfo();
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off<CardSystemCards>("ShowCardInfo", ShowCardInfo);
	}

	private void ShowCardInfo(CardSystemCards cardInfo)
	{
		_cardInfoView.gameObject.SetActive(true);
		_cardInfoView._Open();
		_showItem.ShowCardInfo(cardInfo);
	}

	private void UpdateInfo()
	{
		_cardGroup = CardSystemManager.I.SelectedGroup;
		LanguageExtend.SetLKey(_name, _cardGroup.name);
		bool flag = CardSystemManager.I.IsCollectComplete(_cardGroup.id);
		_stroe.gameObject.SetActive(!flag);
		for (int i = 0; i < _rewards.Length; i++)
		{
			_rewards[i].gameObject.SetActive(false);
		}
		bool isGet = CardSystemManager.I.IsGetRewardInGroup(_cardGroup.id);
		_getRewardButton.gameObject.SetActive(flag && !isGet);
		int length = _cardGroup.reward.GetLength(0);
		for (int j = 0; j < length; j++)
		{
			_rewards[j].gameObject.SetActive(true);
			_rewards[j].SetInfo(_cardGroup.reward[j, 0], _cardGroup.reward[j, 1], isGet);
		}
		int unlockCardGroupCount = CardSystemManager.I.GetUnlockCardGroupCount();
		_truenLeft.gameObject.SetActive(unlockCardGroupCount > 1);
		_turnRight.gameObject.SetActive(unlockCardGroupCount > 1);
	}

	private void GetReward(MButton arg0)
	{
		RewardUnifyDialog rewardUnifyDialog = Global.CreateReward(_cardGroup.reward, RewardType.None, REWARD_PLACE.None, "SetCardGroup", false, false);
		rewardUnifyDialog.SetComplete(delegate
		{
			DispatcherMgr.Emit("GetCardGroupReward", _cardGroup);
		});
		CardSystemManager.I.RecordGruopIDGetReward(_cardGroup.id);
		LogEvent.I.Track("Card_Receive")
			.Add("Reward", _cardGroup.reward.ToStr(',', '|'))
			.Add("CardGroup", _cardGroup.id)
			.Add("OwnedCardGroup", DB.CardSystem.GetRewardGroup.Count)
			.CommonAttr(true)
			.Send();
		LogEvent.I.Track("")
			.Add("CardGroup", DB.CardSystem.GetRewardGroup.Count)
			.UserSet();
		_pDialog._Close();
	}

	private void CloseBox(MButton button)
	{
		_pDialog._Close();
	}

	private void TurnRight(MButton button)
	{
		CardSystemManager.I.TurnNextGroup(-1);
		UpdateInfo();
		_cardListView.UpdateInfo();
	}

	private void TurnLeft(MButton button)
	{
		CardSystemManager.I.TurnNextGroup(1);
		UpdateInfo();
		_cardListView.UpdateInfo();
	}
}
