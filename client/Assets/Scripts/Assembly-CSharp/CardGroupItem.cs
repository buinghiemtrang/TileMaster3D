using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardGroupItem : MonoBehaviour
{
	[SerializeField]
	private GameObject _lock;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Text _name;

	[SerializeField]
	private Text _lockText;

	[SerializeField]
	private Text _progressText;

	[SerializeField]
	private Image progressBar;

	[SerializeField]
	private MButton _showButtom;

	[SerializeField]
	private GameObject _finished;

	[SerializeField]
	private GameObject _progressObject;

	private CardSystemGroup _cardGroupInfo;

	private void Awake()
	{
		DispatcherMgr.On<CardSystemGroup>("GetCardGroupReward", UpdateInfoAfterGetReward);
		_showButtom.onClick.AddListener(ShowGroupInfo);
	}

	private void OnDestroy()
	{
		DispatcherMgr.Off<CardSystemGroup>("GetCardGroupReward", UpdateInfoAfterGetReward);
	}

	private void UpdateInfoAfterGetReward(CardSystemGroup cardGroup)
	{
		if (_cardGroupInfo == cardGroup)
		{
			UpdateInfo();
		}
	}

	private void UpdateInfo()
	{
		LanguageExtend.ExecuteLEvent(_name);
		_finished.SetActive(false);
		var cardsInGroup = CardSystemManager.I.GetCardsInGroup(_cardGroupInfo.id);
		int num = (cardsInGroup != null) ? cardsInGroup.Count : 0;
		_icon.sprite = LoadMgr.LoadSprite("CardSystem/" + _cardGroupInfo.icon);
		if (DB.User.Maxlv < _cardGroupInfo.level)
		{
			_lock.gameObject.SetActive(true);
			LanguageExtend.SetLKey(_lockText, "Language408", _cardGroupInfo.level);
			progressBar.fillAmount = 0f / (float)num;
			_progressText.text = string.Format("{0}/{1}", 0, num);
			return;
		}
		_lock.gameObject.SetActive(false);
		int cardCountInPocketByGroupId = CardSystemManager.I.GetCardCountInPocketByGroupId(_cardGroupInfo.id);
		progressBar.fillAmount = (float)cardCountInPocketByGroupId / (float)num;
		_progressText.text = string.Format("{0}/{1}", cardCountInPocketByGroupId, num);
		if (num <= cardCountInPocketByGroupId && CardSystemManager.I.IsGetRewardInGroup(_cardGroupInfo.id))
		{
			_finished.SetActive(true);
			_progressObject.SetActive(false);
		}
		else
		{
			_progressObject.SetActive(true);
		}
	}

	public IEnumerator SetInfo(CardSystemGroup cardGroupInfo)
	{
		_cardGroupInfo = cardGroupInfo;
		LanguageExtend.SetLBaseKey(_name, _cardGroupInfo.name);
		UpdateInfo();
		yield return null;
	}

	private void ShowGroupInfo(MButton arg0)
	{
		if (DB.User.Maxlv < _cardGroupInfo.level)
		{
			return;
		}
		CardSystemManager.I.SelectedGroup = _cardGroupInfo;
		SceneMgr.AddTopNode<CardsBox>("Prefabs/CardSystem/CardSystemCardsBox");
	}
}
