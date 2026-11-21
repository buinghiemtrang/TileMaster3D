using Festival;
using UnityEngine;
using UnityEngine.UI;

public class StarRankingActivityDanGradingDialog : MonoBehaviour
{
	private DialogShow _pDialog;

	public StarRankingActivityDanGradingItem danGradingItem;

	public Text ttTile;

	public Text ttDest;

	public static StarRankingActivityDanGradingDialog Create()
	{
		return Singleton<FestivalMgr>.I.AddTopNode<StarRankingActivityDanGradingDialog>("Prefabs/Activity/StarRanking/{0}_StarRankingActivityDanGradingDialog");
	}

	private void Awake()
	{
		_pDialog = GetComponent<DialogShow>();
	}

	public void Show(int lastLevel, int curLevel)
	{
		DB.StarRankingActivity.LevelToDanGrading(lastLevel, out var dan, out var grading);
		DB.StarRankingActivity.LevelToDanGrading(curLevel, out var dan2, out var grading2);
		danGradingItem.SetDanGrading(dan, grading);
		_pDialog._Open(delegate
		{
			danGradingItem.PlayChangeAni(dan, grading, dan2, grading2);
		});
		ttTile.SetLKey(string.Format("MasterLeague_Title{0}", dan));
		if (lastLevel < curLevel)
		{
			ttDest.SetLKey("MasterLeague_Information2");
		}
		else if (curLevel < lastLevel)
		{
			ttDest.SetLKey("MasterLeague_Information3");
		}
		else
		{
			ttDest.SetLKey("MasterLeague_Information1");
		}
	}
}
