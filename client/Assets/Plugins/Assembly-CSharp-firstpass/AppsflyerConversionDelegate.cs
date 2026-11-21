public interface AppsflyerConversionDelegate
{
	void OnConversionDataSuccess(string args, string getType);

	void OnConversionDataFail(string args);

	void OnAppOpenAttribution(string args);

	void OnAppOpenAttributionFailure(string args);
}
