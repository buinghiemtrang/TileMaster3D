namespace BestHTTP.Forms
{
	public sealed class HTTPMultiPartForm : HTTPFormBase
	{
		private string Boundary;

		private byte[] CachedData;

		public override void PrepareRequest(HTTPRequest request)
		{
			if (Boundary == null)
			{
				Boundary = "BestHTTP_HTTPMultiPartForm_" + System.Guid.NewGuid().ToString("N");
			}
			request.SetHeader("Content-Type", "multipart/form-data; boundary=" + Boundary);
		}

		public override byte[] GetData()
		{
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			System.Text.Encoding enc = System.Text.Encoding.UTF8;
			if (Fields != null)
			{
				foreach (HTTPFieldData f in Fields)
				{
					string head = "--" + Boundary + "\r\nContent-Disposition: form-data; name=\"" + f.Name + "\"" + ((f.Binary != null) ? ("; filename=\"" + f.FileName + "\"\r\nContent-Type: " + f.MimeType) : string.Empty) + "\r\n\r\n";
					byte[] hb = enc.GetBytes(head);
					ms.Write(hb, 0, hb.Length);
					byte[] pb = f.Payload;
					ms.Write(pb, 0, pb.Length);
					ms.Write(enc.GetBytes("\r\n"), 0, 2);
				}
			}
			byte[] tail = enc.GetBytes("--" + Boundary + "--\r\n");
			ms.Write(tail, 0, tail.Length);
			CachedData = ms.ToArray();
			return CachedData;
		}
	}
}
