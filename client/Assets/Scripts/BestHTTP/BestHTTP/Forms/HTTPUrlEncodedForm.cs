namespace BestHTTP.Forms
{
	public sealed class HTTPUrlEncodedForm : HTTPFormBase
	{
		private const int EscapeTreshold = 256;

		private byte[] CachedData;

		public override void PrepareRequest(HTTPRequest request)
		{
			request.SetHeader("Content-Type", "application/x-www-form-urlencoded");
		}

		public override byte[] GetData()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			if (Fields != null)
			{
				for (int i = 0; i < Fields.Count; i++)
				{
					if (i > 0)
					{
						sb.Append('&');
					}
					sb.Append(EscapeString(Fields[i].Name)).Append('=').Append(EscapeString(Fields[i].Text ?? string.Empty));
				}
			}
			CachedData = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
			return CachedData;
		}

		public static string EscapeString(string originalString)
		{
			return System.Uri.EscapeDataString(originalString ?? string.Empty);
		}
	}
}
