// [rebuild] Fake SDK quảng cáo (quy tắc phạm vi: không cắt ads; fake sau core).
// Bật bằng scripting define FAKE_SDK. AdvertisingProxy.CallAndroidFunc chuyển lệnh sang đây
// thay vì gọi Java com.fuqian.ads.common.AdvertisingProxy; FakeAds gọi ngược lại đúng các
// callback UnitySendMessage mà SDK thật gọi (OnRewardAdClose, OnBannerLoaded, ...).
// Gỡ define FAKE_SDK để quay về SDK thật.
#if FAKE_SDK
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FakeAds : MonoBehaviour
{
	private const float AdSeconds = 2f;

	private static FakeAds _instance;

	private GameObject _bannerBottom;

	private GameObject _bannerTop;

	private bool _showing;

	private static FakeAds I
	{
		get
		{
			if (_instance == null)
			{
				GameObject go = new GameObject("FakeAds");
				Object.DontDestroyOnLoad(go);
				_instance = go.AddComponent<FakeAds>();
			}
			return _instance;
		}
	}

	public static object Call(string method, object[] args)
	{
		Debug.Log("[FakeAds] " + method);
		switch (method)
		{
		case "isReadyRewardAd":
		case "isReadyInterstitial":
			return true;
		case "initialize":
			I.StartCoroutine(I._Init());
			break;
		case "showRewardAds":
			I._Show(true, Arg(args, 0));
			break;
		case "showInterstitialAds":
			I._Show(false, Arg(args, 0));
			break;
		case "showBanner":
			I._Banner(ref I._bannerBottom, true, false);
			break;
		case "hideBanner":
			I._Banner(ref I._bannerBottom, false, false);
			break;
		case "showBannerTop":
			I._Banner(ref I._bannerTop, true, true);
			break;
		case "hideBannerTop":
			I._Banner(ref I._bannerTop, false, true);
			break;
		}
		return null;
	}

	private static string Arg(object[] args, int i)
	{
		if (args == null || args.Length <= i || args[i] == null)
		{
			return "None";
		}
		return args[i].ToString();
	}

	private static string Json(string adsType, string placement, bool reward)
	{
		return "{\"AdPlatform\":1,\"AdsType\":\"" + adsType + "\",\"Mediation\":\"fake\",\"AdCreativeId\":\"fake\",\"AdUnitId\":\"fake\",\"Value\":0.0,\"Precision\":\"fake\",\"Placement\":\"" + placement + "\",\"CurrencyCode\":\"USD\",\"Country\":\"VN\",\"HasReward\":\"" + (reward ? "succ" : "fail") + "\",\"HasClick\":\"fail\",\"Error\":\"\"}";
	}

	private IEnumerator _Init()
	{
		yield return null;
		AdvertisingProxy p = AdvertisingProxy.Instance;
		p.OnInitFinish("");
		p.OnRewardAdLoaded(Json("Reward", "None", false));
		p.OnInterstitialAdLoaded(Json("Interstitial", "None", false));
	}

	private void _Show(bool isReward, string placement)
	{
		if (_showing)
		{
			return;
		}
		StartCoroutine(_ShowAd(isReward, placement));
	}

	private IEnumerator _ShowAd(bool isReward, string placement)
	{
		_showing = true;
		AdvertisingProxy p = AdvertisingProxy.Instance;
		string type = (isReward ? "Reward" : "Interstitial");
		if (isReward)
		{
			p.OnRewardAdShow(Json(type, placement, false));
		}
		else
		{
			p.OnInterstitialAdShow(Json(type, placement, false));
		}
		GameObject canvas = _Canvas("FakeAdScreen", 32767);
		Image bg = _Rect<Image>(canvas.transform, "Bg", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
		bg.color = new Color(0.05f, 0.05f, 0.1f, 0.97f);
		Text label = _Text(bg.transform, "Label", "FAKE " + type.ToUpper() + " AD\n" + placement, 60);
		Button close = null;
		bool closed = false;
		float t = AdSeconds;
		while (t > 0f)
		{
			label.text = "FAKE " + type.ToUpper() + " AD\n" + placement + "\n\n" + Mathf.CeilToInt(t);
			t -= Time.unscaledDeltaTime;
			yield return null;
		}
		label.text = "FAKE " + type.ToUpper() + " AD\n" + placement;
		Image btnImg = _Rect<Image>(bg.transform, "Close", new Vector2(0.3f, 0.15f), new Vector2(0.7f, 0.22f), Vector2.zero, Vector2.zero);
		btnImg.color = new Color(0.2f, 0.7f, 0.3f, 1f);
		_Text(btnImg.transform, "Text", "CLOSE", 48);
		close = btnImg.gameObject.AddComponent<Button>();
		close.onClick.AddListener(delegate
		{
			closed = true;
		});
		while (!closed)
		{
			yield return null;
		}
		Object.Destroy(canvas);
		_showing = false;
		string json = Json(type, placement, true);
		if (isReward)
		{
			p.OnRewardAdImpression(json);
			p.OnRewardAdClose(json);
			p.OnRewardAdLoaded(Json(type, "None", false));
		}
		else
		{
			p.OnInterstitialAdImpression(json);
			p.OnInterstitialAdClose(json);
			p.OnInterstitialAdLoaded(Json(type, "None", false));
		}
	}

	private void _Banner(ref GameObject banner, bool show, bool top)
	{
		if (!show)
		{
			if (banner != null)
			{
				banner.SetActive(false);
			}
			return;
		}
		if (banner == null)
		{
			// Banner chuẩn AdMob/MAX: 320x50 dp, giữa mép dưới (hoặc trên), như banner thật của bản gốc.
			banner = new GameObject(top ? "FakeBannerTop" : "FakeBanner");
			Object.DontDestroyOnLoad(banner);
			Canvas c = banner.AddComponent<Canvas>();
			c.renderMode = RenderMode.ScreenSpaceOverlay;
			c.sortingOrder = 32000;
			CanvasScaler cs = banner.AddComponent<CanvasScaler>();
			cs.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
			cs.scaleFactor = 1f;
			float density = (Screen.dpi > 0f) ? (Screen.dpi / 160f) : (Screen.width / 411f);
			Vector2 size = new Vector2(320f * density, 50f * density);
			float y = top ? 1f : 0f;
			Image img = _Rect<Image>(banner.transform, "Bar", new Vector2(0.5f, y), new Vector2(0.5f, y), Vector2.zero, Vector2.zero);
			RectTransform rt = img.rectTransform;
			rt.pivot = new Vector2(0.5f, y);
			rt.sizeDelta = size;
			Rect safe = Screen.safeArea;
			// SDK gốc (com.fuqian.sdk.ads.max, lớp b.a): MaxAdView LayoutParams(MATCH_PARENT, dpToPx(55), gravity BOTTOM/TOP),
			// mẫu 320x50 nằm sát mép TRÊN khung 55 dp → banner đáy cách mép dưới 5 dp (A50s đo được 13 px), banner trên sát mép trên.
			rt.anchoredPosition = new Vector2(0f, top ? -(Screen.height - safe.yMax) : (safe.yMin + 5f * density));
			img.color = Color.white;
			img.raycastTarget = false;
			Image badge = _Rect<Image>(img.transform, "AdBadge", new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
			badge.rectTransform.pivot = new Vector2(0f, 1f);
			badge.rectTransform.sizeDelta = new Vector2(18f * density, 12f * density);
			badge.color = new Color(1f, 0.75f, 0.1f, 1f);
			badge.raycastTarget = false;
			Text bt = _Text(badge.transform, "Text", "Ad", Mathf.RoundToInt(9f * density));
			bt.color = Color.white;
			Text tx = _Text(img.transform, "Text", "Fake Banner Ad  320x50", Mathf.RoundToInt(14f * density));
			tx.color = new Color(0.2f, 0.2f, 0.2f, 1f);
		}
		banner.SetActive(true);
		AdvertisingProxy.Instance.OnBannerLoaded(Json("Banner", top ? "Top" : "Bottom", false));
		AdvertisingProxy.Instance.OnBannerImpression(Json("Banner", top ? "Top" : "Bottom", false));
	}

	private static GameObject _Canvas(string name, int order)
	{
		GameObject go = new GameObject(name);
		Object.DontDestroyOnLoad(go);
		Canvas c = go.AddComponent<Canvas>();
		c.renderMode = RenderMode.ScreenSpaceOverlay;
		c.sortingOrder = order;
		CanvasScaler s = go.AddComponent<CanvasScaler>();
		s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		s.referenceResolution = new Vector2(1080f, 1920f);
		s.matchWidthOrHeight = 0.5f;
		go.AddComponent<GraphicRaycaster>();
		return go;
	}

	private static T _Rect<T>(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax) where T : Graphic
	{
		GameObject go = new GameObject(name, typeof(RectTransform));
		go.transform.SetParent(parent, false);
		RectTransform rt = (RectTransform)go.transform;
		rt.anchorMin = aMin;
		rt.anchorMax = aMax;
		rt.offsetMin = oMin;
		rt.offsetMax = oMax;
		return go.AddComponent<T>();
	}

	private static Text _Text(Transform parent, string name, string text, int size)
	{
		Text t = _Rect<Text>(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
		t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		t.fontSize = size;
		t.alignment = TextAnchor.MiddleCenter;
		t.color = Color.white;
		t.text = text;
		t.raycastTarget = false;
		return t;
	}
}
#endif
