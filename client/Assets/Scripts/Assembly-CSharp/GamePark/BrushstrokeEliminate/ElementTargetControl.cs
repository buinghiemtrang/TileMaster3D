using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace GamePark.BrushstrokeEliminate
{
	public class ElementTargetControl : MonoBehaviour
	{
		[SerializeField]
		private Transform[] tfFbx = new Transform[6];

		[SerializeField]
		private Text[] ttCounts = new Text[6];

		private void Awake()
		{
			for (int i = 0; i < ttCounts.Length; i++)
			{
				ttCounts[i].gameObject.SetActive(false);
			}
		}

		public void InitTarget()
		{
			System.Collections.Generic.List<BaseData.TargetData> targetDatas = Singleton<DataMgr>.I.Level.TargetDatas;
			for (int i = 0; i < targetDatas.Count; i++)
			{
				CreateFbx(targetDatas[i].ModelId, tfFbx[i]);
				ttCounts[i].text = targetDatas[i].Count.ToString();
				ttCounts[i].gameObject.SetActive(i < 3);
			}
		}

		public void NextTarget()
		{
			int curLayerIndex = Singleton<DataMgr>.I.Level.CurLayerIndex;
			int num = curLayerIndex - 1;
			tfFbx[num].DOScale(0f, 0.3f).OnComplete(tfFbx[num].Hide);
			ttCounts[num].DOFade(0f, 0.3f).OnComplete(ttCounts[num].Hide);
			for (int i = curLayerIndex; i < tfFbx.Length; i++)
			{
				Transform transform = tfFbx[i];
				transform.DOLocalMoveX(transform.localPosition.x + -1.5f, 0.3f);
				Text text = ttCounts[i];
				text.transform.DOLocalMoveX(text.transform.localPosition.x + -116f, 0.3f);
				text.gameObject.SetActive(i - curLayerIndex < 3);
			}
		}

		public void ModifyTargetCount(int value)
		{
			int curLayerIndex = Singleton<DataMgr>.I.Level.CurLayerIndex;
			System.Collections.Generic.List<BaseData.TargetData> targetDatas = Singleton<DataMgr>.I.Level.TargetDatas;
			targetDatas[curLayerIndex].Count += value;
			ttCounts[curLayerIndex].text = targetDatas[curLayerIndex].Count.ToString();
		}

		private void CreateFbx(int modelId, Transform tfParent)
		{
			GameObject gameObject = new GameObject();
			gameObject.transform.SetParent(tfParent, false);
			gameObject.layer = 9;
			SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = LoadMgr.LoadSprite(string.Format("Activity/Textures/{0}", modelId));
		}
	}
}
