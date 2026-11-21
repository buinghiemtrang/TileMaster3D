using UnityEngine;
using UnityEngine.UI;

public class MedalGuideInside : MonoBehaviour
{
	[SerializeField]
	private Transform tfNode;

	[SerializeField]
	private MButton btnClick;

	[SerializeField]
	private MButton btnSeclect;

	[SerializeField]
	private Image imageMask;

	private Transform tfTarget;

	public static MedalGuideInside I;

	private void Awake()
	{
		I = this;
	}

	private void Start()
	{
		if (tfNode != null)
		{
			tfNode.gameObject.SetActive(false);
		}
		if (btnClick != null)
		{
			btnClick.onClick.AddListener(OnClickClose);
			btnClick.interactable = false;
		}
		if (btnSeclect != null)
		{
			btnSeclect.onClick.AddListener(OnClickSelect);
			btnSeclect.interactable = false;
		}
		if (imageMask != null)
		{
			imageMask.SetAlpha(0f);
		}
	}

	public void Show(Transform tfTarget)
	{
		this.tfTarget = tfTarget;
		Invoke("DelayShowMask", 0.5f);
	}

	private void DelayShowMask()
	{
		if (imageMask != null)
		{
			imageMask.SetAlpha(0.8f);
		}
		if (tfNode != null)
		{
			tfNode.gameObject.SetActive(true);
		}
		if (btnClick != null)
		{
			btnClick.interactable = true;
		}
		if (btnSeclect != null)
		{
			btnSeclect.interactable = true;
		}
		Vector3 vector = tfNode.parent.InverseTransformPoint(tfTarget.position);
		tfNode.transform.localPosition = new Vector3(vector.x, vector.y, 0f);
	}

	private void OnClickClose(MButton button)
	{
		this.DestroySelf();
		I = null;
	}

	private void OnClickSelect(MButton button)
	{
		this.DestroySelf();
		I = null;
		MedalMgr.I.CurSelectId = 1;
		DispatcherMgr.Emit("Show_MedalDesc", 0, 1);
	}
}
