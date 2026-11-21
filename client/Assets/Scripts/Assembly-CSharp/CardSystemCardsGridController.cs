using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSystemCardsGridController : MonoBehaviour, UIGridViewController
{
	[SerializeField]
	private GameObject _prefab;

	[SerializeField]
	private UIGridView _gridView;

	private CardSystemGroup _cardGroup;

	private List<CardSystemCards> _cards;

	private void Awake()
	{
		_gridView.Controller = this;
	}

	private void OnEnable()
	{
		_gridView.ReloadContent(false);
	}

	public void UpdateInfo()
	{
		_gridView.ReloadContent(false);
	}

	public GameObject GetItemPrefab()
	{
		return _prefab;
	}

	public IEnumerator OnItemShown(GameObject item, int index)
	{
		CardItem component = item.GetComponent<CardItem>();
		yield return component.SetInfo(_cards[index]);
	}

	public int GetItemCount()
	{
		_cardGroup = CardSystemManager.I.SelectedGroup;
		_cards = CardSystemManager.I.GetCardsInGroup(_cardGroup.id);
		return _cards.Count;
	}
}
