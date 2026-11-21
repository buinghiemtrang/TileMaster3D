using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class Outline3D : MonoBehaviour
{
	[Serializable]
	private class ListVector3
	{
		public List<Vector3> data;
	}

	private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

	[SerializeField]
	private CompareFunction maskMode;

	[SerializeField]
	private CompareFunction fillMode;

	[SerializeField]
	private Color outlineColor = Color.green;

	[SerializeField]
	[Range(0f, 10f)]
	private float outlineWidth = 3f;

	[Header("Optional")]
	[SerializeField]
	[Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
	private bool precomputeOutline;

	[SerializeField]
	[HideInInspector]
	private List<Mesh> bakeKeys = new List<Mesh>();

	[SerializeField]
	[HideInInspector]
	private List<ListVector3> bakeValues = new List<ListVector3>();

	private Renderer[] renderers;

	private Material outlineMaskMaterial;

	private Material outlineFillMaterial;

	private bool needsUpdate;

	private bool initFlag;

	public CompareFunction MaskMode
	{
		get
		{
			return maskMode;
		}
		set
		{
			maskMode = value;
			UpdateMaterialProperties();
		}
	}

	public CompareFunction FillMode
	{
		get
		{
			return fillMode;
		}
		set
		{
			fillMode = value;
			UpdateMaterialProperties();
		}
	}

	public Color OutlineColor
	{
		get
		{
			return outlineColor;
		}
		set
		{
			outlineColor = value;
			UpdateMaterialProperties();
		}
	}

	public float OutlineWidth
	{
		get
		{
			return outlineWidth;
		}
		set
		{
			outlineWidth = value;
			UpdateMaterialProperties();
		}
	}

	private void init()
	{
		initFlag = true;
		renderers = GetComponentsInChildren<Renderer>();
		outlineMaskMaterial = UnityEngine.Object.Instantiate(Resources.Load<Material>("Materials/OutlineMask"));
		outlineFillMaterial = UnityEngine.Object.Instantiate(Resources.Load<Material>("Materials/OutlineFill"));
		outlineMaskMaterial.name = "OutlineMask (Instance)";
		outlineFillMaterial.name = "OutlineFill (Instance)";
		LoadSmoothNormals();
		UpdateMaterialProperties();
	}

	private void OnEnable()
	{
		if (!initFlag)
		{
			init();
		}
		Renderer[] array = renderers;
		foreach (Renderer renderer in array)
		{
			List<Material> list = renderer.sharedMaterials.ToList();
			list.Add(outlineMaskMaterial);
			list.Add(outlineFillMaterial);
			renderer.materials = list.ToArray();
		}
	}

	private void OnValidate()
	{
		UpdateMaterialProperties();
		if ((!precomputeOutline && bakeKeys.Count != 0) || bakeKeys.Count != bakeValues.Count)
		{
			bakeKeys.Clear();
			bakeValues.Clear();
		}
		if (precomputeOutline && bakeKeys.Count == 0)
		{
			Bake();
		}
	}

	private void OnDisable()
	{
		Renderer[] array = renderers;
		foreach (Renderer renderer in array)
		{
			List<Material> list = renderer.sharedMaterials.ToList();
			list.Remove(outlineMaskMaterial);
			list.Remove(outlineFillMaterial);
			renderer.materials = list.ToArray();
		}
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(outlineMaskMaterial);
		UnityEngine.Object.Destroy(outlineFillMaterial);
	}

	private void Bake()
	{
		HashSet<Mesh> hashSet = new HashSet<Mesh>();
		MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
		foreach (MeshFilter meshFilter in componentsInChildren)
		{
			if (hashSet.Add(meshFilter.sharedMesh))
			{
				List<Vector3> data = SmoothNormals(meshFilter.sharedMesh);
				bakeKeys.Add(meshFilter.sharedMesh);
				bakeValues.Add(new ListVector3
				{
					data = data
				});
			}
		}
	}

	private void LoadSmoothNormals()
	{
		MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
		foreach (MeshFilter meshFilter in componentsInChildren)
		{
			if (registeredMeshes.Add(meshFilter.sharedMesh))
			{
				int num = bakeKeys.IndexOf(meshFilter.sharedMesh);
				List<Vector3> uvs = ((num >= 0) ? bakeValues[num].data : SmoothNormals(meshFilter.sharedMesh));
				meshFilter.sharedMesh.SetUVs(3, uvs);
			}
		}
		SkinnedMeshRenderer[] componentsInChildren2 = GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren2)
		{
			if (registeredMeshes.Add(skinnedMeshRenderer.sharedMesh))
			{
				skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];
			}
		}
	}

	private List<Vector3> SmoothNormals(Mesh mesh)
	{
		IEnumerable<IGrouping<Vector3, KeyValuePair<Vector3, int>>> enumerable = mesh.vertices.Select((Vector3 vertex, int index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy((KeyValuePair<Vector3, int> pair) => pair.Key);
		List<Vector3> list = new List<Vector3>(mesh.normals);
		foreach (IGrouping<Vector3, KeyValuePair<Vector3, int>> item in enumerable)
		{
			if (item.Count() != 1)
			{
				Vector3 zero = Vector3.zero;
				foreach (KeyValuePair<Vector3, int> item2 in item)
				{
					zero += mesh.normals[item2.Value];
				}
				zero.Normalize();
				foreach (KeyValuePair<Vector3, int> item3 in item)
				{
					list[item3.Value] = zero;
				}
			}
		}
		return list;
	}

	private void UpdateMaterialProperties()
	{
		if (base.gameObject.activeInHierarchy && !needsUpdate)
		{
			needsUpdate = true;
			StartCoroutine(IEUpdateMaterialProperties());
		}
	}

	private IEnumerator IEUpdateMaterialProperties()
	{
		yield return 0;
		outlineFillMaterial.SetColor("_OutlineColor", outlineColor);
		outlineMaskMaterial.SetFloat("_ZTest", (float)maskMode);
		outlineFillMaterial.SetFloat("_ZTest", (float)fillMode);
		outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
		needsUpdate = false;
	}
}
