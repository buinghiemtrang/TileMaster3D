using Festival;
using Language;
using UnityEngine;

public class SettingsDialogFitSize : MonoBehaviour
{
	public RectTransform Background;

	public RectTransform MiddleContent;

	public GameObject Theme;

	[SerializeField]
	private LanguageText festivalName;

	public void Awake()
	{
		DataCenterLoginMgr.I.GetPlatformID();
		// UNVERIFIED: pseudo-C có kiểm null DataManager.I (null thì throw) nhưng không có lời gọi hay truy cập nào dùng nó; có thể là lời gọi method rỗng đã bị inline. Chưa dịch.
		if (FestivalMgr.I.HasFestival)
		{
			Background.sizeDelta = new Vector2(584f, 1008f);
			MiddleContent.sizeDelta = new Vector2(540f, 454f);
		}
		else
		{
			Background.sizeDelta = new Vector2(584f, 930f);
			MiddleContent.sizeDelta = new Vector2(540f, 384f);
		}
		Theme.gameObject.SetActive(FestivalMgr.I.HasFestival);
	}
}
