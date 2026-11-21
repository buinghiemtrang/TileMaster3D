using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Com.TheFallenGames.OSA.Core.Data.Gallery
{
	[Serializable]
	public class GalleryScale : GalleryAnimation
	{
		[Range(0f, 1f)]
		[SerializeField]
		[FormerlySerializedAs("_MinScale")]
		[Tooltip("No item will have its scale smaller than this")]
		private float _MinValue;

		[SerializeField]
		[Tooltip("From which to which value to interpolate the scale, per component")]
		[FormerlySerializedAs("_ScaleSpace")]
		private Vector3Space _TransformSpace = new Vector3Space(Vector3.zero, Vector3.one);

		public GalleryScale()
		{
			Amount = 1f;
		}

		public float MinValue
		{
			get
			{
				return _MinValue;
			}
			set
			{
			_MinValue = value;
			}
		}

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
