using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class CardSystemManager : Singleton<CardSystemManager>
{
	private class GuaranteeInfo
	{
		public int weightOrigin;

		public int weightAdd;

		public int guarantee;

		public int weight;

		public void UpdateWeight(int count, bool lessenPercent)
		{
			weight = weightOrigin + weightAdd * count;
			if (lessenPercent)
			{
				weight = cf_const.getCardWeight * weight / 100;
			}
		}
	}

	private class CardPoolInfo
	{
		public int id;

		public GuaranteeInfo info;
	}

	private class QualityCardPoolInfo : CardPoolInfo
	{
		public List<CardPoolInfo> cards;
	}

	private Dictionary<int, List<CardSystemCards>> _allCardsDifferentiateByGroups;

	private Dictionary<int, List<CardSystemCards>> _allCardsDifferentiateByQuality;

	private Dictionary<int, List<CardSystemCardBoxGroups>> _allCardSystemCardBoxGroupsConfigs;

	private CardSystemGroup _selectedGroup;

	private Dictionary<int, int> _cardGuaranteeInfo;

	private Dictionary<int, Dictionary<int, QualityCardPoolInfo>> _cardBoxCardPoolInfo;

	private const int HighestQuality = 6;

	private readonly List<CardSystemCards> _newCardList = new List<CardSystemCards>();

	private List<CardPoolInfo> _tempCardPoolInfo;

	private List<CardSystemCards> _cardsInBox;

	private Dictionary<int, int> _dictionaryCahce = new Dictionary<int, int>();

	private StringBuilder _cardSystemCardStringBuilder;

	private readonly List<int[]> _currentCardBoxIds = new List<int[]>(5);

	private int _lastUpdateCardBoxIdsLevel;

	private int _curCardBoxGroupId = -1;

	public CardSystemGroup SelectedGroup
	{
		get
		{
			return _selectedGroup;
		}
		set
		{
			_selectedGroup = value;
		}
	}

	public bool IsGuid => DB.CardSystem.IsGuid;

	public void TurnNextGroup(int direction)
	{
		int key;
		if (_selectedGroup.id + direction > DataCfg.CardSystemGroups.Count)
		{
			key = 1;
		}
		else
		{
			key = _selectedGroup.id + direction;
			if (key < 1)
			{
				key = GetUnlockCardGroupCount();
			}
		}
		_selectedGroup = DataCfg.CardSystemGroups.Get(key);
		if (DB.User.Maxlv < _selectedGroup.level)
		{
			_selectedGroup = DataCfg.CardSystemGroups.Get(1);
		}
	}

	public int GetUnlockCardGroupCount()
	{
		int num = 0;
		foreach (CardSystemGroup value in DataCfg.CardSystemGroups.Values)
		{
			if (value.level <= DB.User.Maxlv)
			{
				num++;
			}
		}
		return num;
	}

	public void ChangeGuidStage()
	{
		DB.CardSystem.IsGuid = true;
	}

	public void InitData()
	{
		_allCardsDifferentiateByGroups = new Dictionary<int, List<CardSystemCards>>();
		_allCardsDifferentiateByQuality = new Dictionary<int, List<CardSystemCards>>();
		_allCardSystemCardBoxGroupsConfigs = new Dictionary<int, List<CardSystemCardBoxGroups>>(2);
		_cardGuaranteeInfo = new Dictionary<int, int>();
		_cardBoxCardPoolInfo = new Dictionary<int, Dictionary<int, QualityCardPoolInfo>>();
		foreach (KeyValuePair<int, CardSystemCards> cardSystemCards in DataCfg.CardSystemCardss)
		{
			if (!_allCardsDifferentiateByGroups.TryGetValue(cardSystemCards.Value.group, out var value))
			{
				value = new List<CardSystemCards>();
				_allCardsDifferentiateByGroups.Add(cardSystemCards.Value.group, value);
			}
			value.Add(cardSystemCards.Value);
			if (!_allCardsDifferentiateByQuality.TryGetValue(cardSystemCards.Value.quality, out var value2))
			{
				value2 = new List<CardSystemCards>();
				_allCardsDifferentiateByQuality.Add(cardSystemCards.Value.quality, value2);
			}
			value2.Add(cardSystemCards.Value);
		}
		foreach (KeyValuePair<int, CardSystemCardBox> cardSystemCardBox in DataCfg.CardSystemCardBoxs)
		{
			string[] random_quality = cardSystemCardBox.Value.random_quality;
			if (!random_quality.isEmpty())
			{
				string[] quality_pool = cardSystemCardBox.Value.quality_pool;
				Dictionary<int, QualityCardPoolInfo> dictionary = new Dictionary<int, QualityCardPoolInfo>();
				DB.CardSystem.CardPoolQualityTimes.TryGetValue(cardSystemCardBox.Value.id, out var _);
				for (int i = 0; i < random_quality.Length; i++)
				{
					List<CardPoolInfo> list = new List<CardPoolInfo>();
					string[] array = quality_pool[i].Split(',');
					for (int j = 0; j < array.Length; j++)
					{
						GuaranteeInfo info;
						int guaranteeInfoOne = GetGuaranteeInfoOne(array[j], out info);
						CardPoolInfo item = new CardPoolInfo
						{
							id = guaranteeInfoOne,
							info = info
						};
						list.Add(item);
					}
					GuaranteeInfo info2;
					int guaranteeInfoOne2 = GetGuaranteeInfoOne(random_quality[i], out info2);
					QualityCardPoolInfo value3 = new QualityCardPoolInfo
					{
						id = guaranteeInfoOne2,
						info = info2,
						cards = list
					};
					dictionary.Add(guaranteeInfoOne2, value3);
				}
				_cardBoxCardPoolInfo.Add(cardSystemCardBox.Value.id, dictionary);
			}
		}
		foreach (KeyValuePair<int, CardSystemCardBoxGroups> cardSystemCardBoxGroups in DataCfg.CardSystemCardBoxGroupss)
		{
			if (!_allCardSystemCardBoxGroupsConfigs.TryGetValue(cardSystemCardBoxGroups.Value.groupType, out var value4))
			{
				value4 = new List<CardSystemCardBoxGroups>();
				_allCardSystemCardBoxGroupsConfigs.Add(cardSystemCardBoxGroups.Value.groupType, value4);
			}
			value4.Add(cardSystemCardBoxGroups.Value);
		}
	}

	private int GetGuaranteeInfoOne(string guaranteeInfos, out GuaranteeInfo info)
	{
		int[] array = guaranteeInfos.ToInt(':');
		info = new GuaranteeInfo
		{
			weightOrigin = array[1],
			weight = array[1],
			weightAdd = array[2],
			guarantee = array[3]
		};
		return array[0];
	}

	private CardSystemCards GetCardByQuality(int cardBoxId, int quality)
	{
		if (!_cardBoxCardPoolInfo.TryGetValue(cardBoxId, out var value))
		{
			return null;
		}
		if (!value.TryGetValue(quality, out var value2))
		{
			return null;
		}
		if (_tempCardPoolInfo == null)
		{
			_tempCardPoolInfo = new List<CardPoolInfo>();
		}
		_tempCardPoolInfo.Clear();
		foreach (CardPoolInfo card in value2.cards)
		{
			CardSystemCards card2 = DataCfg.CardSystemCardss.Get(card.id);
			if (CheckCardIsOk(card2))
			{
				_tempCardPoolInfo.Add(card);
			}
		}
		int num = 0;
		foreach (CardPoolInfo card3 in value2.cards)
		{
			int num2 = Math.Abs(card3.info.guarantee);
			if (num2 != 0)
			{
				DB.CardSystem.CardPoolCardTimes.TryGetValue(card3.id, out var value3);
				if (value3 >= num2 && (card3.info.guarantee >= 0 || !WhetherHaveCard(card3.id)))
				{
					num = card3.id;
					break;
				}
			}
		}
		if (num == 0)
		{
			foreach (CardPoolInfo item in _tempCardPoolInfo)
			{
				DB.CardSystem.CardPoolCardTimes.TryGetValue(item.id, out var value4);
				item.info.UpdateWeight(value4, WhetherHaveCard(item.id));
			}
			num = GetCardByWeight(_tempCardPoolInfo);
		}
		foreach (CardPoolInfo item2 in _tempCardPoolInfo)
		{
			if (item2.id != num)
			{
				DB.CardSystem.CardPoolCardTimes.TryGetValue(item2.id, out var value5);
				DB.CardSystem.SetCardPoolCardTimes(item2.id, value5 + 1);
			}
		}
		CardSystemCards cardSystemCards = DataCfg.CardSystemCardss.Get(num);
		AddCardCount(cardSystemCards, 1);
		return cardSystemCards;
	}

	private bool CheckCardIsOk(CardSystemCards card)
	{
		if (DB.User.Maxlv < card.level)
		{
			return false;
		}
		if (card.show_time.isEmpty())
		{
			return true;
		}
		string[] array = card.show_time.Split('-');
		DateTime byString = MDateTime.GetByString(array[0], "yyyyMMdd", isLocal: true);
		DateTime byString2 = MDateTime.GetByString(array[1], "yyyyMMdd", isLocal: true);
		if (DateTime.Now > byString)
		{
			return DateTime.Now < byString2;
		}
		return false;
	}

	private void AddCardCount(CardSystemCards cardConfig, int count)
	{
		if (cardConfig != null)
		{
			if (!DB.CardSystem.Cards.TryGetValue(cardConfig.id, out var value))
			{
				_newCardList.Add(cardConfig);
			}
			DB.CardSystem.SetCards(cardConfig.id, value + count);
			DB.CardSystem.SetCardPoolCardTimes(cardConfig.id, 0);
		}
	}

	public List<CardSystemCards> OpenCardBox(int cardBoxID)
	{
		if (_cardsInBox == null)
		{
			_cardsInBox = new List<CardSystemCards>();
		}
		_cardsInBox.Clear();
		DB.CardSystem.OpenCardBoxCount++;
		CardSystemCardBox cardSystemCardBox = DataCfg.CardSystemCardBoxs.Get(cardBoxID);
		if (cardSystemCardBox.fixed_get_card.isNotEmpty())
		{
			string[] fixed_get_card = cardSystemCardBox.fixed_get_card;
			for (int i = 0; i < fixed_get_card.Length; i++)
			{
				int[] array = fixed_get_card[i].ToInt(':');
				int key = array[0];
				int num = array[1];
				CardSystemCards cardSystemCards = DataCfg.CardSystemCardss.Get(key);
				AddCardCount(cardSystemCards, num);
				for (int j = 0; j < num; j++)
				{
					_cardsInBox.Add(cardSystemCards);
				}
			}
		}
		int num2 = cardSystemCardBox.card_count - _cardsInBox.Count;
		for (int k = 0; k < num2; k++)
		{
			int qualityByBox = GetQualityByBox(cardSystemCardBox);
			CardSystemCards cardConfig = GetCardByQuality(cardSystemCardBox.id, qualityByBox);
			if (cardConfig != null)
			{
				if (_cardsInBox.FindAll((CardSystemCards csc) => csc.id == cardConfig.id).Count > 1)
				{
					k--;
				}
				else
				{
					_cardsInBox.Add(cardConfig);
				}
			}
		}
		return _cardsInBox;
	}

	private int GetQualityByBox(CardSystemCardBox cardBoxInfo)
	{
		if (!_cardBoxCardPoolInfo.TryGetValue(cardBoxInfo.id, out var value))
		{
			return 0;
		}
		if (!DB.CardSystem.CardPoolQualityTimes.TryGetValue(cardBoxInfo.id, out var value2))
		{
			value2 = new Dictionary<int, int>();
			foreach (KeyValuePair<int, QualityCardPoolInfo> item in value)
			{
				value2.Add(item.Key, 0);
			}
		}
		int num = 0;
		foreach (KeyValuePair<int, QualityCardPoolInfo> item2 in value)
		{
			int num2 = Math.Abs(item2.Value.info.guarantee);
			if (num2 != 0)
			{
				value2.TryGetValue(item2.Key, out var value3);
				if (value3 >= num2)
				{
					num = item2.Key;
					break;
				}
			}
		}
		if (num == 0)
		{
			foreach (KeyValuePair<int, QualityCardPoolInfo> item3 in value)
			{
				value2.TryGetValue(item3.Key, out var value4);
				item3.Value.info.UpdateWeight(value4, lessenPercent: false);
			}
			num = GetCardByWeight(value.Values);
		}
		_dictionaryCahce.Clear();
		foreach (KeyValuePair<int, int> item4 in value2)
		{
			_dictionaryCahce.Add(item4.Key, value2[item4.Key] + 1);
		}
		value2 = _dictionaryCahce;
		if (num != 0)
		{
			value2[num] = 0;
		}
		DB.CardSystem.SetCardPoolQualityTimes(cardBoxInfo.id, value2);
		return num;
	}

	private int GetCardByWeight(IReadOnlyCollection<CardPoolInfo> cardPoolInfos)
	{
		int maxExclusive = cardPoolInfos.Sum((CardPoolInfo cardPool) => cardPool.info.weight);
		int num = UnityEngine.Random.Range(0, maxExclusive);
		int num2 = 0;
		for (int i = 0; i < cardPoolInfos.Count; i++)
		{
			num2 += cardPoolInfos.ElementAt(i).info.weight;
			if (num <= num2)
			{
				return cardPoolInfos.ElementAt(i).id;
			}
		}
		return 0;
	}

	public string GetCardSystemCardsString(List<CardSystemCards> cardSystemCardsList)
	{
		if (_cardSystemCardStringBuilder == null)
		{
			_cardSystemCardStringBuilder = new StringBuilder();
		}
		_cardSystemCardStringBuilder.Clear();
		int num = 0;
		foreach (CardSystemCards cardSystemCards in cardSystemCardsList)
		{
			if (cardSystemCards != null)
			{
				if (num != 0)
				{
					_cardSystemCardStringBuilder.Append(",");
				}
				_cardSystemCardStringBuilder.Append($"{30}|{cardSystemCards.id}");
				num++;
			}
		}
		return _cardSystemCardStringBuilder.ToString();
	}

	public bool IsGetRewardInGroup(int groupId)
	{
		return DB.CardSystem.GetRewardGroup.Contains(groupId);
	}

	public void RecordGruopIDGetReward(int groupId)
	{
		DB.CardSystem.GetRewardGroup.Add(groupId);
	}

	public bool IsCollectComplete(int groupId)
	{
		return (GetCardsInGroup(groupId)?.Count ?? 0) == GetCardCountInPocketByGroupId(groupId);
	}

	public int GetCardCountInPocketByGroupId(int groupId)
	{
		int num = 0;
		if (_allCardsDifferentiateByGroups.TryGetValue(groupId, out var value))
		{
			foreach (CardSystemCards item in value)
			{
				if (DB.CardSystem.Cards.TryGetValue(item.id, out var _))
				{
					num++;
				}
			}
		}
		return num;
	}

	public List<CardSystemCards> GetCardsInGroup(int groupId)
	{
		if (!_allCardsDifferentiateByGroups.TryGetValue(groupId, out var value))
		{
			return null;
		}
		return value;
	}

	public void RefreshCardBoxCD(int qualityId, int freeCdTime)
	{
		long num = DateTime.UtcNow.TotalSeconds();
		DB.CardSystem.SetCardBoxCD(qualityId, num + freeCdTime * 60);
	}

	public long GetCardBoxCD(int quality)
	{
		DB.CardSystem.CardBoxCD.TryGetValue(quality, out var value);
		return value;
	}

	public int GetCardBoxKeyCount(int quality)
	{
		DB.CardSystem.CardKeys.TryGetValue(quality, out var value);
		return value;
	}

	public void ConsumeCardBoxKey(int quality, int count)
	{
		DB.CardSystem.CardKeys.TryGetValue(quality, out var value);
		if (value >= count)
		{
			DB.CardSystem.SetCardKeys(quality, value - count);
		}
	}

	public bool WhetherHaveCard(int Id)
	{
		return DB.CardSystem.Cards.ContainsKey(Id);
	}

	public int GetCardCountInPocketById(int cardID)
	{
		if (!DB.CardSystem.Cards.TryGetValue(cardID, out var value))
		{
			return 0;
		}
		return value;
	}

	public CardSystemCards GetCardSystemCardsConfig(int cardID)
	{
		return DataCfg.CardSystemCardss.Get(cardID);
	}

	public bool IsNewCard(CardSystemCards cardInfo)
	{
		bool num = _newCardList.Contains(cardInfo);
		if (num)
		{
			_newCardList.Remove(cardInfo);
		}
		return num;
	}

	public void ShowCard(CardSystemCards cardInfo)
	{
		DispatcherMgr.Emit("ShowCardInfo", cardInfo);
	}

	public int GetCurrentCardBoxIds(int quality)
	{
		UpdateCurrentCardBoxIds();
		int[] array = _currentCardBoxIds[quality - 1];
		DB.CardSystem.CardBoxGroupIndex.TryGetValue(quality, out var value);
		int num = value % 100 % array.Length;
		int result = array[num];
		int num2 = (num + 1) % array.Length;
		int num3 = (int)((float)value / 100f) % array.Length;
		if (num3 == num2)
		{
			num3 = UnityEngine.Random.Range(0, array.Length);
			num2 = num3;
		}
		DB.CardSystem.SetCardBoxGroupIndex(quality, num3 * 100 + num2);
		return result;
	}

	private void UpdateCurrentCardBoxIds()
	{
		if (_lastUpdateCardBoxIdsLevel == DB.User.Maxlv && _currentCardBoxIds.Count > 0)
		{
			return;
		}
		_lastUpdateCardBoxIdsLevel = DB.User.Maxlv;
		CardSystemCardBoxGroups currentCardBoxGroupsConfig = GetCurrentCardBoxGroupsConfig();
		if (_curCardBoxGroupId != currentCardBoxGroupsConfig.id)
		{
			_curCardBoxGroupId = currentCardBoxGroupsConfig.id;
			_currentCardBoxIds.Clear();
			string[] array = currentCardBoxGroupsConfig.cardBoxGroup.Split('|');
			for (int i = 0; i < array.Length; i++)
			{
				int[] item = array[i].ToInt(',');
				_currentCardBoxIds.Add(item);
			}
		}
	}

	private CardSystemCardBoxGroups GetCurrentCardBoxGroupsConfig()
	{
		List<CardSystemCardBoxGroups> value;
		while (true)
		{
			int num = DB.CardSystem.CardBoxRandomGroupType;
			if (num == -1)
			{
				Dictionary<int, List<CardSystemCardBoxGroups>>.KeyCollection keys = _allCardSystemCardBoxGroupsConfigs.Keys;
				int index = UnityEngine.Random.Range(0, keys.Count - 1);
				num = keys.ElementAt(index);
				DB.CardSystem.CardBoxRandomGroupType = num;
			}
			if (_allCardSystemCardBoxGroupsConfigs.TryGetValue(num, out value))
			{
				break;
			}
			DB.CardSystem.CardBoxRandomGroupType = -1;
		}
		CardSystemCardBoxGroups result = value[0];
		for (int num2 = value.Count - 1; num2 >= 0; num2--)
		{
			CardSystemCardBoxGroups cardSystemCardBoxGroups = value[num2];
			if (DB.User.Maxlv > cardSystemCardBoxGroups.level)
			{
				return cardSystemCardBoxGroups;
			}
		}
		return result;
	}
}
