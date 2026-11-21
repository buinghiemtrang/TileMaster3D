namespace UnityEngine.Purchasing.Security
{
	public class AppleStoreKitTestTangle
	{
		private static byte[] data = System.Convert.FromBase64String("i/bgK3aoYY8OpkFAZOSefUZYjlEHLKe2j4nTOv0KjThyVBDMMh+Af/ZxHn3dbPcdP2Aak5w/NU0EB+reN8kyMjU0NLIGIDA1YysTNzfJMjrlQCbExl/a+1SvR89WvLJ+DvKLvTo+ZUJZRFN9X0IHPQY/MDVjMjAlfaWNPM6XFwj+V+8S/mgpQwjspe0rJTc3yTIzBjU3N8kGODA1Yys5N+HSTp6ZnluR9+xeG1D6+R89ZhrVBmkHJwY5MDVjMjU6PmVCWURTfV8q2T++TlP4t8S7RNExLFgQJMEEhzI1Oj5lQllEU31fQgcnBjkwNWMyBycGOTA1YzI9Oj5lQllEU31fQgckteCxTluvi2iFPNau+MDc9bhbX3eTSD+7jAsL+0Zh7tvZ4+xB/qUyFAY7MD8csH6wwTs3NzczNjW0NznBOzc3PTM2NbQ3NzaENtYKx97fVDYGtDc8NLQ3NzbtSaYKGkU57aEQ9M3QaST3NDU3NjeVDQYPBjkwNWPdgXa3C7ELRmfn8mu7k+pJXG4s6LrXR+18c8UbL6A1YLeQcthDQW1lfrDBOzc/NyA+ZUJZRFN9X0IGtDcDBAUDbCE7AgYGBQQBBwEDBAUDbFWdoawd9Rzu5oTX42w7Dy1ZUR6fO40Opl0w8so706Nm+6f+jBPCbv9CBycGOTA1YzI8Oj5lQllEU31fQr2aw6mGPw6VWE39EGY8LNe/50RAUROPnVoFU5vu9KxOfjnf+vcRO/CvzYW8Y6nZKx6OaRznRakDcQGNcPKfPYX5VDnNgorB7bJoHt1xO8fbBjwwPh0wNzMzMTU1BjswPxywfrCzTn749vMkJ1w5OpgZM/xZTFJJFhywfrDBOzc3PTM2BmkHJwY5MDVjGkPQeock5cvVKWg7S45u4ZZK0kllQllEU31fQgYoITsEBgYCBgcHAdFZ1i3QO177E1j1QD7DfD2YTEIKPQY/MDVjMjAlNGNlByEGIzA/HLAGtDVCBrQ0apY1NDc0NDc3BjswP1ZAAWTayZovazojGbw5w7/+4R0rPDo+ZUJZRFN9X0IHJwY5MDVjMj00Y2UHIQYjMD8csH6wwTs3PzcgPj0Pev15WPaFaYGjjI+ea3xCndlZ+VIKW7ZvBbr8ADAcJy27f/g+yVD5TpprjAhpZh1qH7rr9OEcIekUgijaHaczkQt5");

		private static int[] order = new int[]
		{
			32, 16, 37, 29, 34, 34, 15, 28, 26, 26,
			22, 37, 22, 41, 21, 41, 31, 21, 27, 42,
			24, 22, 28, 40, 40, 26, 42, 35, 33, 35,
			30, 38, 38, 39, 41, 39, 37, 38, 43, 39,
			41, 41, 43, 43, 44
		};

		private static int key = 54;

		public static readonly bool IsPopulated = true;

		public static byte[] Data()
		{
			if (!IsPopulated)
			{
				return null;
			}
			return Obfuscator.DeObfuscate(data, order, key);
		}
	}
}
