using UnityEngine;

public class MedalGuideMain : MonoBehaviour
{
	[SerializeField]
	private Transform tfNode;

	private Transform tfTarget;

	public static MedalGuideMain I;

	public static void Show(Transform tfTargetNode)
	{
		if (!(I != null))
		{
			MedalMgr.I.IsShowFirstStepGuide = true;
			I = SceneMgr.AddTopNode<MedalGuideMain>("Prefabs/Medal/MedalGuideMain", false);
			I.SetTargetNode(tfTargetNode);
		}
	}

	private void SetTargetNode(Transform tfTargetNode)
	{
		tfTarget = tfTargetNode;
		Vector3 vector = tfNode.parent.InverseTransformPoint(tfTargetNode.position);
		tfNode.transform.localPosition = new Vector3(vector.x, vector.y, 0f);
		tfTarget.gameObject.SetActive(false);
	}

	public void OnClick(MButton button)
	{
		tfTarget.gameObject.SetActive(true);
		MedalMgr.I.IsShowFirstStepGuide = false;
		this.DestroySelf();
		I = null;
		SceneMgr.AddTopNode<SelectMedalDialog>("Prefabs/Medal/SelectMedalDialog", false);
	}
}
