using System.Collections;
using UnityEngine;

public interface UIGridViewController
{
	GameObject GetItemPrefab();

	IEnumerator OnItemShown(GameObject item, int index);

	int GetItemCount();
}
