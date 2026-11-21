using System;
using Newtonsoft.Json;
using UnityEngine;

public class ToStringWriteJsonConverter : JsonConverter
{
	public override bool CanWrite => true;

	public override bool CanRead => false;

	public override bool CanConvert(Type objectType)
	{
		return true;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		object value2 = null;
		if (value is Vector3)
		{
			value2 = new JVector3((Vector3)value);
		}
		else if (value is Vector2)
		{
			value2 = new JVector2((Vector2)value);
		}
		else if (value is Color)
		{
			value2 = new JColor((Color)value);
		}
		else if (value is Quaternion)
		{
			value2 = new JQuaternion((Quaternion)value);
		}
		if (value2 != null)
		{
			serializer.Serialize(writer, value2);
		}
		else
		{
			Debug.LogError("缺少对应Json写入处理逻辑！！！！！！");
		}
	}
}
