public interface IAdCallback
{
	void StartLoad(string msg);

	void LoadComplete(string msg);

	void LoadFailed(string msg);

	void OnAdShow(string msg);

	void OnAdClick(string msg);

	void ClearCache(string msg);

	void OnImpression(string msg);
}
