using Function_Reward_Video;

public interface IRewardVideoCDFeature
{
	RewardCDType Type { get; }

	void SetConfig(cfgLevelRewardVideo config);

	bool IsOpen();

	bool IsLevelUnlock();

	bool IsUnlock();

	void UpdateData();

	void TryToReset();

	RewardVideoData GetData();

	void AddCount(int count);
}
