using Newtonsoft.Json;
using UnityEngine;

namespace GameScene_Record
{
	public class RecordElement
	{
		public int Id;

		public int IsPlaced;

		[JsonConverter(typeof(ToStringWriteJsonConverter))]
		public Vector3 Position;

		[JsonConverter(typeof(ToStringWriteJsonConverter))]
		public Vector3 Angle;
	}
}
