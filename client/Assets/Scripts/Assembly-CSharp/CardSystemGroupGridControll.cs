using System.Collections;
using UnityEngine;

public class CardSystemGroupGridControll : MonoBehaviour, UIGridViewController
{
	[SerializeField]
	private GameObject _prefab;

	[SerializeField]
	private UIGridView _gridView;

	private void Awake()
	{
		_gridView.Controller = this;
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		_gridView.ReloadContent(false);
	}

	public GameObject GetItemPrefab()
	{
		return _prefab;
	}

	public IEnumerator OnItemShown(GameObject item, int index)
	{
		CardGroupItem component = item.GetComponent<CardGroupItem>();
		yield return component.SetInfo(DataCfg.CardSystemGroups.Get(index + 1));
	}

	public int GetItemCount()
	{
		return DataCfg.CardSystemGroups.Count;
	}
}
