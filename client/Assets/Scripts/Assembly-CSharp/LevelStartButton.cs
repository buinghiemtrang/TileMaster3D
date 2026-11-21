using GameScene_Level;
using GameScene_Record;
using UnityEngine;
using UnityEngine.UI;

public class LevelStartButton : MonoBehaviour
{
	[SerializeField]
	private MButton button;

	[SerializeField]
	private GameObject normalRoot;

	[SerializeField]
	private GameObject hardRoot;

	[SerializeField]
	private GameObject veryHardRoot;

	[SerializeField]
	private Text normalStartText;

	[SerializeField]
	private Text hardStartText;

	[SerializeField]
	private Text veryHardStartText;

	[SerializeField]
	private Text regionRankStartText;

	[SerializeField]
	private Image iconImg;

	[SerializeField]
	private Sprite[] iconSprites;

	[SerializeField]
	private GameObject[] regionRankObjects;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Text crownCountTxt;

	private Text _valueTex;

	private static LevelStartButton _instance;

	private bool _isIgnoreBgSpriteSet;

	private void Awake()
	{
		_instance = this;
		if (button != null)
		{
			button.onClick.AddListener(_Click);
			DispatcherMgr.On("Update_Account", _RefreshUI);
			GSingleton<RegionRankActivityManager>.ins.OnNewActivityStart += NewRegionRankActivity;
			GSingleton<RegionRankActivityManager>.ins.OnActivityStateEnding += _RefreshUI;
		}
	}

	private void OnDestroy()
	{
		_instance = null;
		DispatcherMgr.Off("Update_Account", _RefreshUI);
		GSingleton<RegionRankActivityManager>.ins.OnNewActivityStart -= NewRegionRankActivity;
		GSingleton<RegionRankActivityManager>.ins.OnActivityStateEnding -= _RefreshUI;
	}

	private void Start()
	{
		_RefreshUI();
	}

	private void NewRegionRankActivity()
	{
		_isIgnoreBgSpriteSet = true;
		_RefreshUI();
		_isIgnoreBgSpriteSet = false;
	}

	public static void PlayRegionRankAnim()
	{
		if (!(_instance == null))
		{
			_instance.animator.Play(Animator.StringToHash("crown_transition"));
		}
	}

	private void _RefreshUI()
	{
		normalRoot.SetActive(value: false);
		hardRoot.SetActive(value: false);
		veryHardRoot.SetActive(value: false);
		if (_isIgnoreBgSpriteSet)
		{
			normalRoot.SetActive(value: true);
			_valueTex = normalStartText;
		}
		else
		{
			switch (Mathf.Clamp(LevelMgr.GetLevelHardLevel(), 0, 2))
			{
			case 1:
				hardRoot.SetActive(value: true);
				_valueTex = hardStartText;
				break;
			case 2:
				veryHardRoot.SetActive(value: true);
				_valueTex = veryHardStartText;
				break;
			default:
				normalRoot.SetActive(value: true);
				_valueTex = normalStartText;
				break;
			}
			LevelMgr.GetLevelStatus(out var isRapidly, out var isRegenerate);
			Sprite sprite = (isRapidly ? iconSprites[0] : (isRegenerate ? iconSprites[1] : null));
			iconImg.gameObject.SetActive(sprite);
			if ((bool)sprite)
			{
				iconImg.sprite = sprite;
			}
		}
		if (GSingleton<RegionRankActivityManager>.ins.GetChallengeCount() < 1)
		{
			GameObject[] array = regionRankObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
		}
		else
		{
			GameObject[] array = regionRankObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			crownCountTxt.text = "x" + GSingleton<RegionRankActivityManager>.ins.GetWinningStreak();
		}
		animator.Play("start1");
		string key = (Singleton<RecordMgr>.I.JudgeHasRecord() ? "Language41" : "Language7");
		if (_valueTex != null)
		{
			_valueTex.SetLBaseKey(key);
			_valueTex.SetLBy();
		}
		regionRankStartText.SetLBaseKey(key);
		regionRankStartText.SetLBy();
	}

	private void _Click(MButton btn)
	{
		Singleton<NetworkInspectionManager>.I.CheckNetwork(delegate
		{
			if (!DB.Bag.JudgeHasEnergy())
			{
				EnergyDialog.Create(_EnterGame, _EnterShop);
			}
			else
			{
				_EnterGame();
			}
		});
		LogEvent.I.HomeButtonClick("Level");
	}

	private void _EnterGame()
	{
		BlackCutDialog.EnterGame();
	}

	private void _EnterShop()
	{
		if (SceneMainBottom.instance != null)
		{
			SceneMainBottom.instance.btnShop.ClickSelf();
		}
	}
}
