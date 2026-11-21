using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameScene_Element;
using UnityEngine;
using _FrameWork;

namespace GameScene
{
	public class PlatformMgr : MonoBehaviour
	{
		public static PlatformMgr I;

		public List<PlacePoint> points;

		[SerializeField]
		private GameObject fuseEffectObj;

		[SerializeField]
		private GameObject crownEffectObj;

		[SerializeField]
		private float pointOffset;

		private List<FusePoint> _fusePoints;

		private List<ParticleSystemInfo> _fuseEffects;

		private Transform _self;

		private Material _dangerousMaterial;

		private bool _showDangerous;

		private float _dangerousLightTimer;

		private float _delayTime;

		private const float DangerousLightBlinkDuration = 1.5f;

		private const float StartShowDelayTime = 0.25f;

		[SerializeField]
		private SpriteRenderer _dangerousSpriteRenderer;

		[SerializeField]
		private Color _originColor;

		[SerializeField]
		private Color _dangerousColorTest;

		private void Awake()
		{
			I = this;
			_self = base.transform;
			_fusePoints = new List<FusePoint>();
			_fuseEffects = new List<ParticleSystemInfo>();
		}

		public void Init()
		{
			_Init();
			CheckSpace();
		}

		public void OutPlacePoint()
		{
			for (int i = 0; i < points.Count; i++)
			{
				if (!points[i].IsFree)
				{
					_Out(points[i]);
					BackMgr.I.Out(points[i].PlacedElement);
				}
			}
			_Init();
			OperationTip.I.BackTip(0, true);
			OperationTip.I.CancelHighlightTip();
			OperationTip.I.CancelHintTip();
			CheckSpace();
		}

		public void OutAppointElement(Element element)
		{
			PlacePoint placePoint = null;
			for (int i = 0; i < points.Count; i++)
			{
				if (!points[i].IsFree && points[i].PlacedElement == element)
				{
					placePoint = points[i];
					break;
				}
			}
			if ((bool)placePoint)
			{
				_Out(placePoint);
				placePoint.Clear();
				_LeftMove();
				OperationTip.I.BackTip(0, true);
				OperationTip.I.CancelHighlightTip();
				OperationTip.I.CancelHintTip();
				CheckSpace();
			}
			else if (element != null && element.Config != null)
			{
				Debug.LogError(string.Format("ID:{0} no find in place point", element.Config.id));
			}
		}

		public bool JudgePlace(Element element)
		{
			int i = points.FindLastIndex((PlacePoint fl) => !fl.IsFree && fl.PlacedElement.Config.id == element.Config.id);
			while (++i < points.Count && !points[i].IsFree)
			{
			}
			return i < points.Count;
		}

		public void Place(Element element, bool isUp)
		{
			int num = points.FindIndex((PlacePoint fd) => fd.IsFree);
			if (num == -1)
			{
				Debug.LogError("no free point");
				return;
			}
			BackMgr.I.Record(element);
			element.ModifyLockStatus(true);
			element.ModifyPlaceStatus(true);
			int num2 = points.FindLastIndex((PlacePoint fl) => !fl.IsFree && fl.PlacedElement.Config.id == element.Config.id);
			PlacePoint placePoint;
			if (num2 == -1)
			{
				placePoint = points[num];
			}
			else
			{
				num2++;
				placePoint = points[num2];
				if (!placePoint.IsFree)
				{
					int num3 = -1;
					for (int num4 = points.Count - 1; num4 >= num2; num4--)
					{
						int num5 = num4;
						if (!points[num4].IsFree)
						{
							if (num3 == -1)
							{
								num5 = -1;
							}
							else
							{
								points[num4].PlacedElement.ModifyParent(_self, true);
								BackMgr.I.StartMove(points[num4].PlacedElement);
								points[num3].Move(points[num4].PlacedElement, delegate(Element placedElement)
								{
									BackMgr.I.StopMove(placedElement);
								});
								points[num4].Clear();
								num5 = num3 - 1;
							}
						}
						num3 = num5;
					}
				}
			}
			element.ModifyParent(_self, true);
			BackMgr.I.StartMove(element);
			placePoint.Place(element, delegate(Element placedElement)
			{
				BackMgr.I.StopMove(placedElement);
				_JudgeFuse();
			}, isUp);
		}

		private void _JudgeFuse()
		{
			List<List<PlacePoint>> list = new List<List<PlacePoint>>();
			List<PlacePoint> list2 = new List<PlacePoint>();
			int num = 0;
			for (int i = 0; i < points.Count; i++)
			{
				PlacePoint placePoint = points[i];
				if (!placePoint.IsFree && !placePoint.IsPlacing)
				{
					if (list2.Count < 1)
					{
						list2.Clear();
						num = placePoint.PlacedElement.Config.id;
					}
					else if (placePoint.PlacedElement.Config.id != num)
					{
						list2.Clear();
						num = placePoint.PlacedElement.Config.id;
					}
					list2.Add(placePoint);
					if (list2.Count == 3)
					{
						List<PlacePoint> list3 = new List<PlacePoint>();
						list3.Add(list2[0]);
						list3.Add(list2[1]);
						list3.Add(list2[2]);
						list.Add(list3);
						list2.Clear();
					}
				}
			}
			_Fuse(list);
			list.Clear();
			OperationTip.I.BackTip(_NoFuseNum(), false);
			CheckSpace();
		}

		private void CheckSpace()
		{
			int num = 0;
			for (int i = 0; i < points.Count; i++)
			{
				if (points[i].IsFree || points[i].IsPlacing)
				{
					num++;
				}
			}
			if (num == 0)
			{
				DispatcherMgr.Emit("GameScene_Platform_Fall");
			}
			OnLimitSpaceChanged(num < 2);
		}

		private void _Fuse(IReadOnlyList<List<PlacePoint>> allPoints)
		{
			int number = allPoints.Count;
			for (int i = 0; i < allPoints.Count; i++)
			{
				List<Element> list = new List<Element>();
				for (int j = 0; j < allPoints[i].Count; j++)
				{
					PlacePoint placePoint = allPoints[i][j];
					placePoint.PlacedElement.ModifyParent(placePoint.PlacedElement.DefaultParentNode, true);
					list.Add(placePoint.PlacedElement);
					BackMgr.I.Fuse(placePoint.PlacedElement);
					placePoint.Clear();
				}
				FusePoint fusePoint = _GetFusePoint();
				fusePoint.FuseCompleteCallback = delegate
				{
					number--;
					if (number <= 0)
					{
						_LeftMove();
					}
				};
				fusePoint.PlayFuseEffect = _PlayFuseEffect;
				fusePoint.Fuse(list);
			}
		}

		private void _LeftMove()
		{
			int moveNum = 0;
			int num = -1;
			for (int i = 0; i < points.Count; i++)
			{
				if (!points[i].IsFree)
				{
					if (num != -1)
					{
						moveNum++;
						points[i].PlacedElement.ModifyParent(_self, true);
						BackMgr.I.StartMove(points[i].PlacedElement);
						points[num].Move(points[i].PlacedElement, delegate(Element element)
						{
							BackMgr.I.StopMove(element);
							moveNum--;
							if (moveNum <= 0)
							{
								_JudgeFuse();
							}
						});
						points[i].Clear();
						num++;
					}
				}
				else if (num == -1)
				{
					num = i;
				}
			}
		}

		private int _NoFuseNum()
		{
			int num = 0;
			for (int i = 0; i < points.Count; i++)
			{
				if (!points[i].IsFree)
				{
					num++;
				}
			}
			return num;
		}

		private void _PlayFuseEffect(Vector3 position, ElementType elementType)
		{
			ParticleSystemInfo particleSystemInfo = null;
			if (elementType != ElementType.RegionRankCollect)
			{
				particleSystemInfo = _fuseEffects.FirstOrDefault((ParticleSystemInfo fd) => fd.status == ParticleSystemStatus.Stop);
			}
			if (!particleSystemInfo)
			{
				GameObject original = ((elementType == ElementType.RegionRankCollect) ? crownEffectObj : fuseEffectObj);
				particleSystemInfo = Object.Instantiate(original, _self).GetComponentInChildren<ParticleSystemInfo>();
				if (elementType != ElementType.RegionRankCollect)
				{
					_fuseEffects.Add(particleSystemInfo);
				}
			}
			if (particleSystemInfo != null)
			{
				particleSystemInfo.SetPosition(position, false);
				particleSystemInfo.PlayOnce(elementType == ElementType.RegionRankCollect, 0.5f, true);
			}
		}

		private FusePoint _GetFusePoint()
		{
			FusePoint fusePoint = _fusePoints.FirstOrDefault((FusePoint fd) => fd.IsFree);
			if (fusePoint != null)
			{
				return fusePoint;
			}
			fusePoint = new FusePoint();
			fusePoint.Clear();
			_fusePoints.Add(fusePoint);
			return fusePoint;
		}

		private void _Init()
		{
			for (int i = 0; i < points.Count; i++)
			{
				points[i].Init(i, pointOffset);
			}
			for (int j = 0; j < _fusePoints.Count; j++)
			{
				_fusePoints[j].Clear();
			}
		}

		private void _Out(PlacePoint point)
		{
			if (!point)
			{
				return;
			}
			Element element = point.PlacedElement;
			element.ModifyLockStatus(false);
			element.ModifyPlaceStatus(false);
			element.ModifyParent(element.DefaultParentNode, true);
			element.Freeze(RigidbodyConstraints.None);
			element.SetScale(1f);
			element.AddForce(new Vector3(0f, 4f, 10f), ForceMode.Impulse);
			element.Self.DelayCall(0.02f, delegate
			{
				element.SetLayer(10);
			});
		}

		public void RecoverData()
		{
			int num = 0;
			foreach (int item in Singleton<GameScene_Record.RecordMgr>.I.Data.PlatformData)
			{
				Element placedElementById = ElementsMgr.I.Data.GetPlacedElementById(item);
				if (placedElementById != null)
				{
					BackMgr.I.Record(placedElementById);
					points[num].SetElement(placedElementById);
					num++;
				}
			}
		}

		private void OnDestroy()
		{
			for (int i = 0; i < points.Count; i++)
			{
				points[i].Clear();
			}
			for (int j = 0; j < _fusePoints.Count; j++)
			{
				_fusePoints[j].Clear();
			}
		}

		private void OnLimitSpaceChanged(bool isShow)
		{
			_showDangerous = isShow;
			if (isShow)
			{
				_dangerousLightTimer = 0f;
				_delayTime = 0.25f;
				if (_dangerousMaterial == null && _dangerousSpriteRenderer != null)
				{
					_dangerousMaterial = _dangerousSpriteRenderer.material;
				}
			}
			else if (_dangerousMaterial != null)
			{
				_dangerousMaterial.color = _originColor;
			}
		}

		private void Update()
		{
			if (_showDangerous)
			{
				float deltaTime = Time.deltaTime;
				if (_delayTime <= 0f)
				{
					UpdateDangerous(deltaTime);
				}
				else
				{
					_delayTime -= deltaTime;
				}
			}
		}

		private void UpdateDangerous(float deltaTime)
		{
			if (_dangerousMaterial == null)
			{
				return;
			}
			_dangerousLightTimer += deltaTime;
			if (_dangerousLightTimer > 1.5f)
			{
				_dangerousLightTimer -= 1.5f;
			}
			_dangerousMaterial.color = Color.Lerp(_dangerousColorTest, _originColor, Mathf.Abs(_dangerousLightTimer / 1.5f * 2f - 1f));
		}
	}
}
