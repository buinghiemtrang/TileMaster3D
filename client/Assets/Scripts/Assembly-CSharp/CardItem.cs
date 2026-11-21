using System.Collections;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class CardItem : MonoBehaviour
{
	[SerializeField]
	private SkeletonGraphic _background;

	[SerializeField]
	private GameObject[] _stars;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private Text _name;

	[SerializeField]
	private GameObject _has;

	[SerializeField]
	private GameObject frontLock;

	[SerializeField]
	private Text _cardCount;

	[SerializeField]
	private GameObject _isNew;

	[SerializeField]
	private MButton button;

	[SerializeField]
	private GameObject _countBg;

	[SerializeField]
	private Outline _nameOutline;

	private CardSystemCards _cardInfo;

	private Color[] _colors = new Color[6]
	{
		new Color(0.31764707f, 0.3372549f, 0.34901962f, 1f),
		new Color(0.0627451f, 0.34509805f, 0.20392157f, 1f),
		new Color(0.003921569f, 0.24705882f, 0.4627451f, 1f),
		new Color(0.3647059f, 0.11764706f, 0.5058824f, 1f),
		new Color(0.6627451f, 0.28627452f, 0.101960786f, 1f),
		new Color(0.4392157f, 0.06666667f, 0.06666667f, 1f)
	};

	public IEnumerator SetInfo(CardSystemCards cardInfo)
	{
		_cardInfo = cardInfo;
		ShowCardInfo(_cardInfo);
		int cardCountInPocketById = CardSystemManager.I.GetCardCountInPocketById(_cardInfo.id);
		button.interactable = cardCountInPocketById != 0;
		frontLock.SetActive(!button.interactable);
		_countBg.SetActive(cardCountInPocketById > 1);
		_cardCount.text = cardCountInPocketById.ToString();
		_isNew.SetActive(CardSystemManager.I.IsNewCard(_cardInfo));
		yield return null;
	}

	public void ShowCardInfo(CardSystemCards cardInfo)
	{
		LanguageExtend.SetLKey(_name, cardInfo.name);
		UnityExtend.SetShadowColors(_name, _colors[cardInfo.quality - 1]);
		_nameOutline.effectColor = _colors[cardInfo.quality - 1];
		_background.AnimationState.SetAnimation(0, cardInfo.background.ToString("00"), true);
		for (int i = 0; i < _stars.Length; i++)
		{
			_stars[i].SetActive(i < cardInfo.quality);
		}
		_icon.sprite = LoadMgr.LoadSprite("CardSystem/" + cardInfo.icon);
	}

	public void ShowCardInfo()
	{
		if (_has.activeSelf)
		{
			return;
		}
		CardSystemManager.I.ShowCard(_cardInfo);
	}
}
