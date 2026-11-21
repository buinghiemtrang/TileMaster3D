// [rebuild] Fake IAP (quy tắc phạm vi: không cắt IAP; fake sau core).
// Bật bằng scripting define FAKE_SDK. StoreUtility._Call chuyển lệnh Java com.fuqian.StoreKit.GoogleBilling
// sang đây; FakeBilling gọi ngược lại đúng các callback UnitySendMessage mà plugin thật gọi
// (OnGooglePlayConnectSuccess, OnQueryProductSuccess, OnPurchaseSuccess, ...).
// Sản phẩm lấy từ DataCfg.cfbt_Storess (giá = cfg.price, PriceAmount = cfg.usd).
// Mua: hiện màn "FAKE PURCHASE" với nút BUY / CANCEL. Xác minh đơn server bị bỏ qua ở
// IAPAndroidOrderValidateManager.OrderValidate (#if FAKE_SDK). Gỡ define FAKE_SDK để về SDK thật.
#if FAKE_SDK
using System.Collections;
using System.Collections.Generic;
using FQDev.StoreKit;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class FakeBilling : MonoBehaviour
{
	private static FakeBilling _instance;

	private bool _showing;

	private static FakeBilling I
	{
		get
		{
			if (_instance == null)
			{
				GameObject go = new GameObject("FakeBilling");
				Object.DontDestroyOnLoad(go);
				_instance = go.AddComponent<FakeBilling>();
			}
			return _instance;
		}
	}

	public static void Call(string method, object[] args)
	{
		Debug.Log("[FakeBilling] " + method);
		switch (method)
		{
		case "init":
			I.StartCoroutine(I._Next(delegate
			{
				StoreUtility.Instance.OnGooglePlayConnectSuccess("fake");
			}));
			break;
		case "queryInAppProducts":
		{
			string json = _ProductsJson((args != null && args.Length > 0) ? (args[0] as string[]) : null);
			I.StartCoroutine(I._Next(delegate
			{
				StoreUtility.Instance.OnQueryProductSuccess(json);
			}));
			break;
		}
		case "purchase":
			I._Purchase(_Arg(args, 0), _Arg(args, 1));
			break;
		case "queryIncompleteOrder":
			I.StartCoroutine(I._Next(delegate
			{
				StoreUtility.Instance.OnQueryIncompleteOrderFinish("[]");
			}));
			break;
		case "consumePurchaseAsync":
			I.StartCoroutine(I._Next(delegate
			{
				StoreUtility.Instance.OnConsumeSuccess("fake");
			}));
			break;
		}
		// setPublicKey, acknowledgePurchaseAsync: không cần làm gì.
	}

	private static string _Arg(object[] args, int i)
	{
		if (args == null || args.Length <= i || args[i] == null)
		{
			return string.Empty;
		}
		return args[i].ToString();
	}

	// Bảng VND: [đo] = thấy trên shop/popup bản gốc; [ước] = chưa thấy, nội suy ~23.000 ₫/USD.
	private static readonly Dictionary<int, long> VndByUsdCents = new Dictionary<int, long>
	{
		{ 99, 22000 },      // [ước]
		{ 199, 44000 },     // [đo]
		{ 299, 68000 },     // [đo]
		{ 499, 113000 },    // [đo]
		{ 599, 136000 },    // [đo]
		{ 699, 160000 },    // [đo]
		{ 799, 182000 },    // [ước]
		{ 899, 208000 },    // [đo]
		{ 999, 228000 },    // [đo]
		{ 1199, 275000 },   // [ước]
		{ 1299, 297000 },   // [đo]
		{ 1499, 344000 },   // [ước]
		{ 1699, 390000 },   // [ước]
		{ 1999, 460000 },   // [đo]
		{ 2099, 482000 },   // [ước]
		{ 2499, 575000 },   // [đo]
		{ 2599, 598000 },   // [ước]
		{ 2999, 690000 },   // [ước]
		{ 3999, 920000 },   // [đo]
		{ 4999, 1150000 },  // [ước]
		{ 5999, 1380000 },  // [ước]
		{ 9999, 2300000 },  // [đo]
		{ 19999, 4550000 }  // [đo]
	};

	private static long _Vnd(string productId, float usd)
	{
		if (productId != null && productId.EndsWith(".mega_bundle"))
		{
			return 920000; // [đo]
		}
		int cents = (int)System.Math.Round(usd * 100.0);
		long v;
		if (VndByUsdCents.TryGetValue(cents, out v))
		{
			return v;
		}
		return (long)System.Math.Round((usd + 0.01) * 23000.0 / 1000.0) * 1000; // [ước]
	}

	private static string _FormatVnd(long vnd)
	{
		return vnd.ToString("#,0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".") + " \u20ab";
	}

	private static cfbt_Stores _Cfg(string productId)
	{
		if (DataCfg.cfbt_Storess == null)
		{
			return null;
		}
		return DataCfg.cfbt_Storess.Find((cfbt_Stores a) => a.id != null && a.id.Split('|')[0] == productId);
	}

	private static string _ProductsJson(string[] ids)
	{
		List<FQDev.StoreKit.Product> list = new List<FQDev.StoreKit.Product>();
		if (ids != null)
		{
			foreach (string id in ids)
			{
				cfbt_Stores cfg = _Cfg(id);
				FQDev.StoreKit.Product p = new FQDev.StoreKit.Product();
				p.ID = id;
				p.ProductType = (cfg != null) ? (FQDev.StoreKit.ProductType)cfg.productType : FQDev.StoreKit.ProductType.Consumable;
				p.Type = (p.ProductType == FQDev.StoreKit.ProductType.Subscription) ? "subs" : "inapp";
				p.Title = (cfg != null) ? cfg.name : id;
				p.Description = p.Title;
				// Giá giống Google Play VN (đọc từ shop bản gốc trên A50s 2026-11-11). Play trả giá theo product ID:
				// phần lớn theo mức USD, riêng mega_bundle ($49.99 trong config) bán mức 920.000 ₫.
				long vnd = (cfg != null) ? _Vnd(id, cfg.usd) : 0;
				p.Price = (vnd > 0) ? _FormatVnd(vnd) : ((cfg != null) ? "$" + cfg.usd.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) : string.Empty);
				p.PriceCurrencyCode = (vnd > 0) ? "VND" : "USD";
				p.PriceAmount = (vnd > 0) ? (decimal)vnd : ((cfg != null) ? (decimal)cfg.usd : 0m);
				list.Add(p);
			}
		}
		return JsonConvert.SerializeObject(list);
	}

	private IEnumerator _Next(System.Action action)
	{
		yield return null;
		action();
	}

	private void _Purchase(string productId, string extraInfo)
	{
		if (_showing)
		{
			StoreUtility.Instance.OnPurchaseFailed("fake: busy");
			return;
		}
		StartCoroutine(_ShowPurchase(productId, extraInfo));
	}

	private IEnumerator _ShowPurchase(string productId, string extraInfo)
	{
		_showing = true;
		cfbt_Stores cfg = _Cfg(productId);
		FQDev.StoreKit.Product prod = StoreUtility.Instance.GetProduct(productId);
		string price = (prod != null) ? prod.Price : "?";
		GameObject canvas = _Canvas("FakePurchaseScreen", 32767);
		Image bg = _Rect<Image>(canvas.transform, "Bg", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
		bg.color = new Color(0.05f, 0.08f, 0.05f, 0.95f);
		Text label = _Rect<Text>(bg.transform, "Label", new Vector2(0.05f, 0.45f), new Vector2(0.95f, 0.8f), Vector2.zero, Vector2.zero);
		_Style(label, "FAKE PURCHASE\n" + productId + "\n" + price, 56);
		int result = 0; // 1 = buy, 2 = cancel
		_Button(bg.transform, "Buy", "BUY", new Vector2(0.15f, 0.25f), new Vector2(0.48f, 0.32f), new Color(0.2f, 0.7f, 0.3f, 1f), delegate
		{
			result = 1;
		});
		_Button(bg.transform, "Cancel", "CANCEL", new Vector2(0.52f, 0.25f), new Vector2(0.85f, 0.32f), new Color(0.7f, 0.25f, 0.2f, 1f), delegate
		{
			result = 2;
		});
		while (result == 0)
		{
			yield return null;
		}
		Object.Destroy(canvas);
		_showing = false;
		if (result == 2)
		{
			StoreUtility.Instance.OnPurchaseCanceled("fake: canceled");
			yield break;
		}
		string token = "fake_token_" + System.Guid.NewGuid().ToString("N");
		FQDev.StoreKit.Purchase purchase = new FQDev.StoreKit.Purchase();
		purchase.ProductID = productId;
		purchase.TransactionID = "GPA.FAKE-" + System.DateTime.UtcNow.Ticks;
		purchase.PurchaseToken = token;
		purchase.PurchaseState = 1;
		purchase.ObfuscatedAccountId = string.Empty;
		purchase.SignatureFlag = "true";
		purchase.IsAcknowledged = "false";
		StoreUtility.Instance.OnPurchaseSuccess(JsonConvert.SerializeObject(purchase));
	}

	private static void _Button(Transform parent, string name, string text, Vector2 aMin, Vector2 aMax, Color color, UnityEngine.Events.UnityAction onClick)
	{
		Image img = _Rect<Image>(parent, name, aMin, aMax, Vector2.zero, Vector2.zero);
		img.color = color;
		Text t = _Rect<Text>(img.transform, "Text", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
		_Style(t, text, 48);
		img.gameObject.AddComponent<Button>().onClick.AddListener(onClick);
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

	private static void _Style(Text t, string text, int size)
	{
		t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
		t.fontSize = size;
		t.alignment = TextAnchor.MiddleCenter;
		t.color = Color.white;
		t.text = text;
		t.raycastTarget = false;
	}
}
#endif
