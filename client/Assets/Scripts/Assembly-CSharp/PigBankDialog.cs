using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PigBankDialog : DialogBase
{
	public static PigBankDialog instance;

	public Text nowGoldText;

	public Text mixGoldRewardText;

	public Text maxGoldRewardText;

	public Text contentText;

	public Image goldProgressImage;

	public Transform progress;

	public Transform progressLineTf;

	public Transform progressMixTf;

	public MButton closeButton;

	public MButton okButton;

	public Text smallPriceText;

	public Text bigPriceText;

	public GameObject minReward;

	public GameObject objFullHint;

	public static int NowGold;

	private int mixGoldReward;

	private int maxGoldReward;

	private int price = 400;

	private bool isBuy;

	private string goldPigProductId;

	private cfbt_Stores goldPigStoreCfg;

	protected override void Awake()
	{
		base.Awake();
		instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		instance = null;
		DispatcherMgr.Off<GiftType>("BuySuccessCallBack", _iAPBuySuc);
		DispatcherMgr.Off("IAPInitFinishedCallBack", _IAPInitSuc);
	}

	public static bool CanShow(int offLe = 0)
	{
		return DB.User.Maxlv + offLe > 3;
	}

	private void Start()
	{
		List<cfbt_Stores> cfgs = Global.pStores.GetCfgs(BuySpotType.Pig, GiftType.Pig);
		if (cfgs != null)
		{
			goldPigStoreCfg = cfgs[0];
		}
		mixGoldReward = PigBankData.Instance.MixGoldReward;
		maxGoldReward = PigBankData.Instance.MaxGoldReward;
		closeButton.onClick.AddListener(delegate
		{
			_Close();
		});
		okButton.onClick.AddListener(OnOkButtonClick);
		InitShow();
		DispatcherMgr.On<GiftType>("BuySuccessCallBack", _iAPBuySuc);
		DispatcherMgr.On("IAPInitFinishedCallBack", _IAPInitSuc);
		_Open();
		LogEvent.I.HomeFrameOpen("GoldPig", "Click");
	}

	private void InitShow()
	{
		mixGoldRewardText.text = mixGoldReward.ToString();
		maxGoldRewardText.text = maxGoldReward.ToString();
		contentText.text = string.Format(contentText.text, mixGoldReward);
		float t = (float)mixGoldReward / (float)maxGoldReward;
		Vector2 arcPoint = GetArcPoint(t);
		SetAnchorPos(progressMixTf, arcPoint);
		UpdateGold();
	}

	private Vector2 GetArcPoint(float t)
	{
		float num = 1f - t;
		float num2 = 2f * num * t;
		float x = num2 * 232f + t * t * 464f;
		float y = num * num * 48f - num2 * 52f + t * t * 48f;
		return new Vector2(x, y);
	}

	private Vector2 Slerp(Vector2 start, Vector2 end, float t)
	{
		float num = Mathf.Sqrt(start.x * start.x + start.y * start.y);
		Vector2 vector = Vector2.zero;
		if (num > 1E-05f)
		{
			vector = start / num;
		}
		float num2 = Mathf.Sqrt(end.x * end.x + end.y * end.y);
		Vector2 vector2 = Vector2.zero;
		if (num2 > 1E-05f)
		{
			vector2 = end / num2;
		}
		float num3 = Mathf.Clamp(vector.x * vector2.x + vector.y * vector2.y, -1f, 1f);
		float num4 = Mathf.Acos(num3);
		if (Mathf.Max(Mathf.Abs(num4) * 1E-06f, Mathf.Epsilon * 8f) <= Mathf.Abs(num4))
		{
			float num5 = Mathf.Sin(num4);
			float num6 = Mathf.Sin((1f - t) * num4) / num5;
			float num7 = Mathf.Sin(num4 * t) / num5;
			return vector * num6 + vector2 * num7;
		}
		return vector;
	}

	public static void SetAnchorPos(Transform self, Vector2 nowPos)
	{
		((RectTransform)self).anchoredPosition = nowPos;
	}

	private void UpdateGold()
	{
		NowGold = PigBankData.Instance.NowGold;
		nowGoldText.text = NowGold.ToString();
		goldProgressImage.fillAmount = (float)NowGold / (float)maxGoldReward;
		bool flag = NowGold >= mixGoldReward && IAPManager.instance.isInitialized;
		okButton.interactable = flag;
		okButton.Find("gray").gameObject.SetActive(!flag);
		if (goldPigStoreCfg != null)
		{
			string text = (string.IsNullOrEmpty(goldPigStoreCfg.price) ? string.Format("${0}", goldPigStoreCfg.usd) : goldPigStoreCfg.price);
			smallPriceText.text = text;
			bigPriceText.text = text;
		}
		minReward.SetActive(NowGold < maxGoldReward);
		objFullHint.SetActive(NowGold >= maxGoldReward);
	}

	private void _iAPBuySuc(GiftType type)
	{
		if (type == GiftType.Pig)
		{
			isBuy = true;
			ResetPigBankGold();
			DB.User.IsSkipPigAnim = false;
			_Close();
		}
	}

	private void _IAPInitSuc()
	{
		UpdateGold();
	}

	private void OnOkButtonClick(MButton mButton)
	{
		if (goldPigStoreCfg != null)
		{
			SceneMgr.OpenWaiting(30f, 0f);
			if (!IAPManager.instance.Buy(goldPigStoreCfg))
			{
				SceneMgr.CloseWaiting();
			}
		}
	}

	public void AddPigBankGold(int gold)
	{
		PigBankData.Instance.SetPigBankGold(NowGold + gold);
		UpdateGold();
	}

	public void ResetPigBankGold()
	{
		GameObject gameObject = GameObject.Find("ButtonPig");
		if (gameObject != null)
		{
			gameObject.transform.Find("ImageAnimator").GetComponent<Animator>().enabled = false;
		}
		PigBankData.Instance.SetPigBankGold(0);
		UpdateGold();
	}
}
