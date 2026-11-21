using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Com.TheFallenGames.OSA.Core.Data.Gallery
{
	[Serializable]
	public class GalleryRotation : GalleryAnimation
	{
		[SerializeField]
		[Tooltip("From which to which value to interpolate the rotation, per component")]
		[FormerlySerializedAs("_ScaleSpace")]
		private Vector3Space _TransformSpace = new Vector3Space(Vector3.zero, Vector3.zero);

		public override Vector3Space TransformSpace
		{
			get
			{
				return _TransformSpace;
			}
			set
			{
			_TransformSpace = value;
			}
		}
	}
}
