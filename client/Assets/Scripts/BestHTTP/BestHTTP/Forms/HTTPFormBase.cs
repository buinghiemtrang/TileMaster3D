using System.Collections.Generic;
using System.Text;

namespace BestHTTP.Forms
{
	public class HTTPFormBase
	{
		private const int LongLength = 256;

		public List<HTTPFieldData> Fields { get; set; }

		public bool IsEmpty => Fields == null || Fields.Count == 0;

		public bool IsChanged { get; protected set; }

		public bool HasBinary { get; protected set; }

		public bool HasLongValue { get; protected set; }

		public void AddBinaryData(string fieldName, byte[] content)
		{
			AddBinaryData(fieldName, content, null, null);
		}

		public void AddBinaryData(string fieldName, byte[] content, string fileName)
		{
			AddBinaryData(fieldName, content, fileName, null);
		}

		public void AddBinaryData(string fieldName, byte[] content, string fileName, string mimeType)
		{
			if (Fields == null)
			{
				Fields = new List<HTTPFieldData>();
			}
			Fields.Add(new HTTPFieldData
			{
				Name = fieldName,
				FileName = fileName ?? (fieldName + ".dat"),
				MimeType = mimeType ?? "application/octet-stream",
				Binary = content
			});
			HasBinary = true;
			IsChanged = true;
		}

		public void AddField(string fieldName, string value)
		{
			AddField(fieldName, value, Encoding.UTF8);
		}

		public void AddField(string fieldName, string value, Encoding e)
		{
			if (Fields == null)
			{
				Fields = new List<HTTPFieldData>();
			}
			Fields.Add(new HTTPFieldData
			{
				Name = fieldName,
				Text = value,
				Encoding = e
			});
			if (value != null && value.Length > 256)
			{
				HasLongValue = true;
			}
			IsChanged = true;
		}

		public virtual void CopyFrom(HTTPFormBase fields)
		{
			Fields = (fields.Fields == null) ? new List<HTTPFieldData>() : new List<HTTPFieldData>(fields.Fields);
			HasBinary = fields.HasBinary;
			HasLongValue = fields.HasLongValue;
			IsChanged = true;
		}

		public virtual void PrepareRequest(HTTPRequest request)
		{
		}

		public virtual byte[] GetData()
		{
			return null;
		}
	}
}
