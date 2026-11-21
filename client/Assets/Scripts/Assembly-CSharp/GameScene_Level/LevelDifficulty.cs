namespace GameScene_Level
{
	public class LevelDifficulty
	{
		private LevelData _data;

		private PeriodicityDifficultyControl _periodicityDifficulty;

		private PayingUserDifficultyControl _payingUserDifficulty;

		private DynamicDifficultyControl _dynamicDifficulty;

		private int _remainingTime;

		private bool _isNewLv;

		public PeriodicityDifficultyControl PeriodicityDifficulty => _periodicityDifficulty;

		public PayingUserDifficultyControl PayingUserDifficulty => _payingUserDifficulty;

		public DynamicDifficultyControl DynamicDifficulty => _dynamicDifficulty;

		public void Init(LevelData data)
		{
			_data = data;
			_periodicityDifficulty = new PeriodicityDifficultyControl();
			_payingUserDifficulty = new PayingUserDifficultyControl();
			_dynamicDifficulty = new DynamicDifficultyControl();
			_remainingTime = 0;
			_isNewLv = false;
		}

		public void SetRemainingTime(int value)
		{
			_remainingTime = value;
		}

		public void SetIsNewLvState(bool isNewLv)
		{
			_isNewLv = isNewLv;
		}

		public void Victory()
		{
			_dynamicDifficulty.Victory(_data.Lv, _isNewLv, _remainingTime, _data.Time);
		}

		public void Fail(bool isTimeOut, int remainingModelNumber, int totalModelNumber)
		{
			_dynamicDifficulty.Fail(_data.Lv, isTimeOut, remainingModelNumber, totalModelNumber);
		}
	}
}
