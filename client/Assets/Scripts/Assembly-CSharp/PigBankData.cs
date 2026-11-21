public class PigBankData
{
	private static PigBankData _instance;

	private int _nowGold;

	public static PigBankData Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new PigBankData();
			}
			return _instance;
		}
	}

	public int MixGoldReward => pig_const.PigMixGoldReward;

	public int MaxGoldReward => pig_const.PigMaxGoldReward;

	public int PigReward => pig_const.PigReward;

	public int NowGold
	{
		get
		{
			_nowGold = DB.Other.PigBankGold;
			return _nowGold;
		}
		private set
		{
			_nowGold = value;
			DB.Other.PigBankGold = value;
		}
	}

	public bool NeedDoAnimation { get; set; }

	public int SetPigBankGold(int num)
	{
		int num2 = num;
		if (pig_const.PigMaxGoldReward <= num)
		{
			num2 = pig_const.PigMaxGoldReward;
		}
		int val = 0;
		if (num > -1)
		{
			val = num2;
		}
		NowGold = val;
		_nowGold = DB.Other.PigBankGold;
		return _nowGold;
	}

	public bool IsCanBuy()
	{
		return NowGold >= pig_const.PigMixGoldReward;
	}

	public bool IsFull()
	{
		return NowGold >= pig_const.PigMaxGoldReward;
	}

	public bool IsForceGoHome()
	{
		if (cf_const.IsVictoryBackHome < 1)
		{
			return false;
		}
		return cf_const.VictoryBackHomeLevel <= DB.User.Maxlv + 1;
	}

	public bool CanShow()
	{
		return pig_const.UnlockLevel < DB.User.Maxlv + 1;
	}

	public void WinAddPigBankGold()
	{
		int num = NowGold + pig_const.PigReward;
		int num2 = num;
		if (pig_const.PigMaxGoldReward <= num)
		{
			num2 = pig_const.PigMaxGoldReward;
		}
		int val = 0;
		if (num > -1)
		{
			val = num2;
		}
		NowGold = val;
		_nowGold = DB.Other.PigBankGold;
	}
}
