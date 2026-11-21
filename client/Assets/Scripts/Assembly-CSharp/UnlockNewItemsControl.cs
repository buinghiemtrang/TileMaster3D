using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UnlockNewItemsControl : MonoBehaviour
{
	public Transform content;

	public Action EndCall;

	private readonly Vector3[] _eightPositions = new Vector3[8]
	{
		new Vector3(-1f, 0f, 3f),
		new Vector3(1f, 0f, 3f),
		new Vector3(-1f, 0f, 1.5f),
		new Vector3(1f, 0f, 1.5f),
		new Vector3(-1f, 0f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(-1f, 0f, -1.5f),
		new Vector3(1f, 0f, -1.5f)
	};

	private readonly Vector3[] _sixPositions = new Vector3[6]
	{
		new Vector3(-1f, 0f, 3.2f),
		new Vector3(1f, 0f, 3.2f),
		new Vector3(-1f, 0f, 1.2f),
		new Vector3(1f, 0f, 1.2f),
		new Vector3(-1f, 0f, -1f),
		new Vector3(1f, 0f, -1f)
	};

	private void OnDestroy()
	{
		StopAllCoroutines();
	}

	public void unlockModels()
	{
		int num = DB.User.Curlv + 1;
		for (int i = 0; i < DataCfg.cfgLevelUnlocks.Count; i++)
		{
			cfgLevelUnlock cfgLevelUnlock = DataCfg.cfgLevelUnlocks[i];
			if (cfgLevelUnlock.id != num)
			{
				continue;
			}
			Vector3[] positions = ((cfgLevelUnlock.unlockIds.Count >= 8) ? _eightPositions : _sixPositions);
			for (int j = 0; j < cfgLevelUnlock.unlockIds.Count; j++)
			{
				int num2 = cfgLevelUnlock.unlockIds[j];
				try
				{
					StartCoroutine(IECreateElement(DataCfg.cfgModels[num2], j, positions));
				}
				catch (Exception) // UNVERIFIED: kiểu exception của catch (asm 0x11dd5d4..0x11dd608 tra bằng metadata, chưa phân giải tên)
				{
					Debug.LogError(string.Format("ID:{0} no find last in cfgSynthetic!", num2));
				}
			}
			break;
		}
	}

	private IEnumerator IECreateElement(cfgModel config, int index, Vector3[] positions)
	{
		if (index < positions.Length)
		{
			yield return new WaitForSeconds(0.1f);
		}
		Transform transform = UnityEngine.Object.Instantiate(LoadMgr.Load<Transform>("Effect/3D_Unlock_new"), content);
		transform.position = positions[index];
		transform.localScale = Vector3.one;
		yield return new WaitForSeconds(0.1f);
		GameObject gameObject = LoadMgr.Load<GameObject>("Elements/" + config.prefab);
		if (!gameObject)
		{
			yield break;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, content);
		Outline3D component = gameObject2.GetComponent<Outline3D>();
		Rigidbody component2 = gameObject2.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.enabled = false;
		}
		if (component2 != null)
		{
			component2.isKinematic = true;
		}
		gameObject2.layer = LayerMask.NameToLayer("House3D");
		gameObject2.transform.Find("Model").gameObject.layer = LayerMask.NameToLayer("House3D");
		gameObject2.AddComponent<UnlockNewItem>();
		gameObject2.transform.position = positions[index];
		gameObject2.transform.localScale = Utils.Vec3(config.unlockScale);
		gameObject2.transform.localEulerAngles = config.unlockAngle;
	}

	private void Start()
	{
		_ = DB.User.Curlv;
		unlockModels();
	}

	public void InitData(int level)
	{
		unlockModels();
	}

	public void SetItemInfo(GameObject obj, cfgModel newCfg)
	{
		Logs.Common.Info("SetItemInfo ");
		Image component = obj.transform.GetComponent<Image>();
		component.sprite = LoadMgr.LoadSprite(newCfg.sFile);
	}

	public void ClickReceive(MButton btn)
	{
		btn.interactable = false;
		content.gameObject.SetActive(false);
		GetComponent<DialogShow>()._Close(delegate
		{
			UnlockMgr.Instance.ClearUnlockModeIDs();
			if (EndCall != null)
			{
				EndCall();
			}
		});
	}
}
