using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace GameMagicCube
{
	public class CubeItem : MonoBehaviour
	{
		private static readonly int MainTexID;

		private static readonly int ColorID;

		private int _id;

		public Vector3Int _index;

		private Color _originColor;

		private Renderer _renderer;

		private BoxCollider _collider;

		private bool _selected;

		private Transform tfParent;

		private readonly Color colorSelect = new Color(0.1f, 0.9f, 0.1f, 1f);

		private readonly Color colorHint = new Color(1f, 0f, 0f, 1f);

		public int ID => _id;

		public Vector3Int Index
		{
			get
			{
				return _index;
			}
			set
			{
				_index = value;
				OriginPoint = DataMgr.I.GetCubePoint(new Vector3(_index.x, _index.y, _index.z));
			}
		}

		public Vector3 OriginPoint { get; private set; }

		public bool IsEmty => !_collider.enabled;

		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				_selected = value;
			}
		}

		static CubeItem()
		{
			MainTexID = Shader.PropertyToID("_MainTex");
			ColorID = Shader.PropertyToID("_Color");
		}

		public void SetHintShow(bool b_sure)
		{
			_renderer.material.SetColor(ColorID, b_sure ? colorHint : _originColor);
		}

		public void SetSelectColor(bool b_sure)
		{
			_renderer.material.SetColor(ColorID, b_sure ? colorSelect : _originColor);
		}

		public void SetColliderEnabled(bool b_sure)
		{
			_collider.enabled = b_sure;
		}

		private void Awake()
		{
			_collider = gameObject.GetComponent<BoxCollider>();
			gameObject.layer = DataMgr.CubeLayerID;
			_collider.isTrigger = true;
			_renderer = gameObject.GetComponent<Renderer>();
			_originColor = _renderer.material.GetColor(ColorID);
			tfParent = transform.parent;
		}

		public void SetIndex(Vector3Int index)
		{
			Index = index;
			_collider.enabled = true;
			transform.SetScale3(DataMgr.I.pCfg.size);
			transform.localPosition = OriginPoint;
		}

		public void SetData(int id)
		{
			_id = id;
		}

		public IEnumerator LoadTexture()
		{
			ResourceRequest textureLoader = Resources.LoadAsync($"MagicCubes/CubeIcons/img{_id}");
			yield return textureLoader;
			_renderer.material.SetTexture(MainTexID, textureLoader.asset as Texture2D);
		}

		public void Eliminate()
		{
			_selected = false;
			transform.SetParent(tfParent, false);
			gameObject.SetActive(false);
			transform.DOKill();
		}
	}
}
