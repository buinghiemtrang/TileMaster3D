using UnityEngine;

public class RenderSettings : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 1f)]
	private float _lambertCut = 0.3f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _shadowOcclusion;

	private static int LambertCutID = Shader.PropertyToID("_LambertCut");

	private static int ShadowOcclusionID = Shader.PropertyToID("_ShadowOcclusion");

	private void Apply()
	{
		Shader.SetGlobalFloat(LambertCutID, _lambertCut);
		Shader.SetGlobalFloat(ShadowOcclusionID, _shadowOcclusion);
	}

	private void Start()
	{
		Apply();
	}

	private void OnValidate()
	{
		Apply();
	}
}
