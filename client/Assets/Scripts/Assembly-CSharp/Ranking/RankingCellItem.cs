using FQDev.WordBlocker;
using UnityEngine;
using UnityEngine.UI;

namespace Ranking
{
	public class RankingCellItem : MonoBehaviour
	{
		public Sprite[] spRootBg;

		public Sprite[] spStarBg;

		public Sprite[] spRankIco;

		public Color[] colors;

		public Image imgRootBg;

		public Image imgStarBg;

		public Image imgRankIco;

		public Image imgMedalIcon;

		public Image imgStarBg1;

		public Image teamBadgeImg;

		public Text ttRank;

		public Text ttName;

		public Text ttScore;

		public Text ttScore1;

		[SerializeField]
		private FilteredText _nameText;

		public bool bOwn;

		private int _nRanking;

		public int Ranking => _nRanking;

		public void InitData(RankingData rankData)
		{
			_nRanking = rankData.ranking;
			ttRank.text = ((rankData.ranking < rankData.limit) ? string.Format("{0}", rankData.ranking + 1) : string.Format("{0}+", rankData.limit));
			_nameText.Text = rankData.name ?? string.Empty;
			ttScore.text = rankData.score.ToString();
			ttScore1.text = rankData.score.ToString();
			imgStarBg.gameObject.SetActive(true);
			imgStarBg1.gameObject.SetActive(false);
			imgMedalIcon.gameObject.SetActive(rankData.rankingType == RankingType.PersonalSum || rankData.rankingType == RankingType.PersonalMonth);
			teamBadgeImg.gameObject.SetActive(rankData.rankingType == RankingType.TeamSum || rankData.rankingType == RankingType.TeamMonth);
			Vector3 localPosition = _nameText.transform.localPosition;
			if (rankData.rankingType == RankingType.PersonalSum || rankData.rankingType == RankingType.PersonalMonth)
			{
				localPosition.x = -96f;
				_nameText.transform.localPosition = localPosition;
				cfg_medal cfg_medal = ((rankData.medal != 0) ? MedalMgr.I.GetCfgById(rankData.medal) : null);
				if (cfg_medal == null)
				{
					cfg_medal = MedalMgr.I.GetMedalCfgByStar(rankData.sumScore);
				}
				if (cfg_medal != null)
				{
					imgMedalIcon.sprite = LoadMgr.LoadSprite("MedalIcon/" + cfg_medal.Icon);
				}
			}
			else
			{
				localPosition.x = -128f;
				_nameText.transform.localPosition = localPosition;
				TeamDataMgr.I.SetTeamBadge(rankData.teamBadge, teamBadgeImg);
			}
			bool flag = _nRanking < spRankIco.Length;
			imgRankIco.gameObject.SetActive(flag);
			if (flag)
			{
				imgRankIco.sprite = spRankIco[_nRanking];
			}
			int num = ((!bOwn && !rankData.isOwn) ? 1 : 0);
			imgRootBg.sprite = spRootBg[num];
			imgStarBg.sprite = spStarBg[num];
			imgStarBg1.sprite = spStarBg[num];
		}
	}
}
