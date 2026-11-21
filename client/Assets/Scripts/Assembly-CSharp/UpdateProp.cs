using UnityEngine;
using UnityEngine.UI;

public class UpdateProp : MonoBehaviour
{
	public Text ttNum;

	public PropID kProp;

	public GameObject objBtnBuy;

	public Image imageIcon;

	private MButton btnIcon;

	[SerializeField]
	private MButton addBtn;

	[HideInInspector]
	public bool isFree;

	protected virtual void Awake()
	{
		if ((bool)addBtn)
		{
			addBtn.onClick.AddListener(_ClickAdd);
		}
		if (ttNum != null)
		{
			ttNum.text = "";
		}
	}

	protected virtual void Start()
	{
		Refresh();
		if (imageIcon == null)
		{
			return;
		}
		MButton component = imageIcon.gameObject.GetComponent<MButton>();
		if (component != null)
		{
			component.onClick.AddListener(OnClickIcon);
		}
	}

	public void Refresh()
	{
		if (DB.Bag != null)
		{
			int prop = DB.Bag.GetProp(kProp);
			_setNum(prop);
			UpdateBtnBuy(prop);
		}
	}

	private void OnEnable()
	{
		DispatcherMgr.On<int, int>("Update" + kProp, UpdateText);
	}

	private void OnDisable()
	{
		DispatcherMgr.Off<int, int>("Update" + kProp, UpdateText);
	}

	protected virtual void UpdateBtnBuy(int count)
	{
		if (!(objBtnBuy == null))
		{
			objBtnBuy.gameObject.SetActive(count == 0);
		}
	}

	public virtual void UpdateText(int last = 0, int end = 0)
	{
		if (last != end)
		{
			TweenMgr.NumberTo(0.2f, last, end, _setNum);
		}
		UpdateBtnBuy(end);
	}

	private void _setNum(int v)
	{
		ttNum.text = v.ToString();
	}

	public virtual void ClickBtnStore(MButton button)
	{
	}

	private void OnClickIcon(MButton button)
	{
		if (kProp == PropID.Money)
		{
			Global.ShowBubbleTips("Language332", imageIcon.transform);
		}
	}

	private void _ClickAdd(MButton btn)
	{
		if (SceneMainBottom.instance != null)
		{
			SceneMainBottom.instance.ClickBtnShop(null);
			LogEvent.I.HomeButtonClick("ShopButton");
		}
	}
}
