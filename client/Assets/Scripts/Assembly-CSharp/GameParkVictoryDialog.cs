using UnityEngine;
using UnityEngine.UI;

public class GameParkVictoryDialog : VictoryBase
{
	private DialogShow _pDialog;

	public Text ttScore;

	private int _nScore;

	private int _adMultiple;

	public LeftRightMoveAdsCtl adsCrl;

	public Text adMultipleTex;

	public Sprite spStar;

	public static GameParkVictoryDialog Create(int gold)
	{
		GameParkVictoryDialog gameParkVictoryDialog = SceneMgr.AddTopNode<GameParkVictoryDialog>("Prefabs/GameParkVictoryDialog");
		gameParkVictoryDialog._nScore = gold;
		return gameParkVictoryDialog;
	}

	private void Start()
	{
		_pDialog = GetComponent<DialogShow>();
		ttScore.text = "+" + _nScore;
		ModifySumStar(_nScore);
		adsCrl.cback = delegate(int va)
		{
			_adMultiple = va;
			adMultipleTex.text = string.Format("X{0}", va);
		};
	}

	public void ClickHome(MButton button)
	{
		Global.ShowLvInterstitial(delegate
		{
			_home(openGamePark: false);
		}, INTER_PLACE.AdInterstitial);
	}

	public void ClickAdv(MButton button)
	{
		adsCrl.SetStop();
		Global.ShowRewardedVideo(delegate(string info)
		{
			if (!info.Equals("succ") || ttScore == null)
			{
				return;
			}
			int star = _nScore;
			int num = (_adMultiple - 1) * _nScore;
			ModifySumStar(num);
			Vector3 p = SceneMgr.tfTopRoot.InverseTransformPoint(button.transform.position);
			Vector3 p2 = SceneMgr.tfTopRoot.InverseTransformPoint(ttScore.transform.parent.position);
			Vector3 p3 = Utils.Vec3(p.x + (p2.x - p.x) * 0.5f, p2.y, 0f);
			int count = num / 20;
			if (count > 49)
			{
				count = 50;
			}
			if (count < 2)
			{
				count = 1;
			}
			int one = num / count;
			int remaining = count;
			TweenMgr.BezierFlyerH(spStar, 0.6f, p, p3, p2, count, Utils.Vec2(-100f, 100f), 1f, 0.01f, delegate
			{
				ttScore.text = (star + one).ToString();
				remaining--;
				if (remaining == 0)
				{
					ttScore.text = (_adMultiple * _nScore).ToString();
					_home(openGamePark: true);
				}
			});
		}, REWARD_PLACE.AdStarDouble);
	}

	public void ClickNext(MButton button)
	{
		Global.ShowLvInterstitial(delegate
		{
			_home(openGamePark: true);
		}, INTER_PLACE.AdInterstitial);
	}

	private void _home(bool openGamePark)
	{
		_pDialog._Close();
		BlackCutDialog.Show(delegate
		{
			AudioMgr.I.ResumeMusic();
			GiftDataMgr.ins.ResetShowPopStatus(true);
			if (openGamePark)
			{
				SceneMgr.SetData("Open" + FunctionType.GamePark);
			}
			SceneMgr.LoadScene("MainScene", "GameMagicCubeScene", delegate
			{
				BlackCutDialog.Hide();
			});
		});
	}
}
