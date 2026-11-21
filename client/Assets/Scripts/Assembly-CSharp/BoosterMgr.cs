using UnityEngine;

public class BoosterMgr
{
	public static BoosterMgr I = new BoosterMgr();

	public void SetGear(int va, bool isDirect = false)
	{
		if (!JudgeUnlock())
		{
			return;
		}
		int[,] array = _GetGearData();
		if (array == null)
		{
			return;
		}
		int length = array.GetLength(0);
		if (isDirect)
		{
			if (va < 0 || length <= va)
			{
				Debug.LogError(string.Format("booster set gear error VALUE:{0}", va));
			}
			else
			{
				DB.Booster.CurGear = va;
			}
		}
		else if (length >= DB.Booster.CurGear + va)
		{
			DB.Booster.CurGear = DB.Booster.CurGear + va;
		}
	}

	public bool JudgeUnlock()
	{
		return cf_const.boosterUnlockLv <= DB.User.Curlv + 1;
	}

	public int GetValue()
	{
		int[,] array = _GetGearData();
		int length = array.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			if (array[i, 0] == DB.Booster.CurGear)
			{
				return array[i, 1];
			}
		}
		return 0;
	}

	public void GetProgress(out int cur, out int max)
	{
		max = 0;
		cur = DB.Booster.CurGear;
		int[,] array = _GetGearData();
		int length = array.GetLength(0);
		if (length > 0)
		{
			max = length;
			return;
		}
		max = 1;
		Debug.LogError("booster get progress error: gear data length 0");
	}

	private int[,] _GetGearData()
	{
		return cf_const.boosterGear.ToInt(2, '|', ',');
	}
}
