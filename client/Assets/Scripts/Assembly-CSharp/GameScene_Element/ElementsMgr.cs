using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GameScene_Element_Create;
using GameScene_Element_Data;
using GameScene;
using GameScene_Element_Display;
using GameScene_Level;
using GameScene_Record;
using UnityEngine;

namespace GameScene_Element
{
	public class ElementsMgr : MonoBehaviour
	{
		public static ElementsMgr I;

		public ElementCreateControl createControl;

		public Transform starNode;

		private Action _initCompletedCallback;

		private GameStageProgress _regenerateView;

		public ElementsData Data { get; private set; }

		private void Awake()
		{
			I = this;
			Data = new ElementsData();
		}

		public void Init(GameStageProgress regenerateView, bool isAsyCreate, Action completed)
		{
			_regenerateView = regenerateView;
			_initCompletedCallback = completed;
			Data.Init();
			if (regenerateView != null)
			{
				regenerateView.ResetStageProgress();
				if (Singleton<RecordMgr>.I.IsLoading)
				{
					StartCoroutine("IEReplyRecord");
					return;
				}
			}
			List<cfgModel> modelConfigs = LevelMgr.Data.ModelConfigs;
			if (isAsyCreate)
			{
				createControl.RealAsyCreate(modelConfigs, delegate(List<Element> elements, object value)
				{
					_CreateCompleted(elements);
				}, true);
			}
			else
			{
				List<Element> elements2 = createControl.SynCreate(modelConfigs);
				_WaveDivision(elements2);
				_DisplayElements();
				BlackCutDialog.Hide();
			}
		}

		public void RemoveElements(List<Element> elements, Vector3 startPosition)
		{
			GameMgr.I.regionRankGameScoreLayer.AddScore(elements, startPosition);
			Data.Remove(elements);
			if (Data.Elements.Count == 0)
			{
				DispatcherMgr.Emit("GameScene_Elements_DestroyAll");
			}
			_regenerateView.UpdateProductProgress(doAnimation: true);
			if (Data.DisplayedElementsId.Count <= 0)
			{
				int count = Data.WaveElements.Count;
				_DisplayElements();
				if (count != 0)
				{
					_regenerateView.OnLevelStagePass(0, 1);
				}
			}
		}

		public bool AssimilateElements(int assimilationElementId)
		{
			List<Element> list = new List<Element>();
			foreach (KeyValuePair<int, List<Element>> item in Data.DisplayedElementsId)
			{
				foreach (Element item2 in item.Value)
				{
					if (item2.Type == ElementType.Ordinary && !item2.IsLocked && !item2.IsSelected && !item2.IsGuided)
					{
						list.Add(item2);
						if (list.Count >= 3)
						{
							break;
						}
					}
				}
				if (list.Count > 2)
				{
					break;
				}
				list.Clear();
			}
			if (list.Count != 3)
			{
				return false;
			}
			List<Element> list2 = new List<Element>();
			for (int i = 0; i < list.Count; i++)
			{
				Element element = DisplayInFixedPosition(assimilationElementId, list[i].Self.position, list[i].Self.localEulerAngles);
				if (element != null)
				{
					list2.Add(element);
				}
			}
			if (list2.Count == 3)
			{
				Data.AddDisplayedElements(list2, isNew: true);
				RemoveElements(list, Vector3.zero);
				return true;
			}
			for (int num = list2.Count - 1; num >= 0; num--)
			{
				list2[num].Destroy();
			}
			return false;
		}

		private Element DisplayInFixedPosition(int id, Vector3 position, Vector3 angle)
		{
			Element element = createControl.SynCreate(id);
			if (element == null)
			{
				return null;
			}
			element.Self.gameObject.SetActive(true);
			element.Self.position = position;
			element.Self.localEulerAngles = angle;
			element.Freeze(RigidbodyConstraints.None);
			return element;
		}

		private void _CreateCompleted(List<Element> elements)
		{
			_WaveDivision(elements);
			_DisplayElements();
			BlackCutDialog.Hide();
		}

		private void _WaveDivision(List<Element> elements)
		{
			List<int> wave = LevelMgr.Data.Wave;
			for (int i = 0; i < wave.Count; i++)
			{
				int num = Mathf.Min(wave[i] * 3, elements.Count);
				if (num > 0)
				{
					List<Element> list = new List<Element>();
					for (int j = 0; j < num; j++)
					{
						list.Add(elements[0]);
						elements.RemoveAt(0);
					}
					Data.AddWaveElements(list);
				}
			}
			if (elements.Count > 0)
			{
				Data.AddWaveElements(elements);
			}
		}

		private void _DisplayElements()
		{
			List<Element> elements = Data.GetNewWaveElementsAndRemove();
			Data.ResetDisplayNumber();
			Data.AddDisplayedElements(elements, isNew: false);
			if (LevelMgr.Data.Type == LevelType.Ordinary && LevelMgr.Data.Lv == 1)
			{
				FirstLevelDisplay display = new FirstLevelDisplay();
				int guideNumber = 3;
				List<Element> guideElements = new List<Element>();
				display.Display(elements, delegate(Element element)
				{
					display.SetPositionAndAngle(element);
					if (guideNumber == 0)
					{
						if (TeatherHand.I != null)
						{
							TeatherHand.I.AddElements(guideElements);
						}
					}
					else
					{
						guideElements.Add(element);
						guideNumber--;
					}
				}, _DisplayCompleted);
			}
			else
			{
				FallFromSkyDisplay display2 = new FallFromSkyDisplay();
				display2.Display(elements, delegate(Element element)
				{
					element.Self.localPosition = display2.GetPosition();
					element.Self.localEulerAngles = display2.GetRandomAngle();
					element.Self.localScale = new Vector3(1.2f, 1.2f, 1.2f);
				}, _DisplayCompleted);
			}
		}

		private void _DisplayCompleted()
		{
			_initCompletedCallback.InvokeSafe();
			_initCompletedCallback = null;
		}

		private void _ReplyRecord(int index)
		{
			RecordData data = Singleton<RecordMgr>.I.Data;
			if (index < data.WaveData.Count)
			{
				createControl.RealAsyCreate(data.WaveData[index], delegate(List<Element> elements, object value)
				{
					Data.AddWaveElements(elements);
					_ReplyRecord((int)value + 1);
				}, isGroup: false, index);
			}
			else
			{
				if (data.DisplayedData == null)
				{
					return;
				}
				if (data.DisplayedData.Count == 0)
				{
					Data.SetTotalNumber(Singleton<RecordMgr>.I.Data.ElementTotalNumber);
					_regenerateView.OnLevelStagePass(0, 1);
					_DisplayElements();
					return;
				}
				List<RecordElement> displayedData = data.DisplayedData;
				if (Data.WaveElements.Count == 0)
				{
					_regenerateView.OnLevelStagePass(0, 1);
				}
				List<int> list = new List<int>();
				for (int i = 0; i < displayedData.Count; i++)
				{
					list.Add(displayedData[i].Id);
				}
				createControl.RealAsyCreate(list, delegate(List<Element> elements, object value)
				{
					for (int j = 0; j < elements.Count; j++)
					{
						Element element = elements[j];
						RecordElement recordElement = displayedData[j];
						element.Self.gameObject.SetActive(true);
						element.ModifyPlaceStatus(recordElement.IsPlaced == 1);
						element.Self.localPosition = recordElement.Position;
						element.Self.localEulerAngles = recordElement.Angle;
						element.Freeze(RigidbodyConstraints.None);
					}
					Data.AddDisplayedElements(elements, isNew: true);
					Data.SetTotalNumber(Singleton<RecordMgr>.I.Data.ElementTotalNumber);
					_initCompletedCallback.InvokeSafe();
					_initCompletedCallback = null;
					BlackCutDialog.Hide();
				}, isGroup: false, index);
			}
		}

		private IEnumerator IEReplyRecord()
		{
			int number = 0;
			List<Element> tempElements = new List<Element>();
			foreach (List<int> ids in Singleton<RecordMgr>.I.Data.WaveData)
			{
				tempElements.Clear();
				int count = ids.Count;
				for (int i = 0; i < count; i++)
				{
					number++;
					Element element = createControl.SynCreate(ids[i]);
					if (element != null)
					{
						tempElements.Add(element);
						if (number >= 20)
						{
							number = 0;
							yield return new WaitForFixedUpdate();
						}
					}
				}
				Data.AddWaveElements(tempElements);
			}
			if (Singleton<RecordMgr>.I.Data.DisplayedData.Count != 0)
			{
				tempElements.Clear();
				if (Data.WaveElements.Count == 0)
				{
					_regenerateView.OnLevelStagePass(0, 1);
				}
				foreach (RecordElement item in Singleton<RecordMgr>.I.Data.DisplayedData)
				{
					number++;
					Element element2 = createControl.SynCreate(item.Id);
					if (element2 != null)
					{
						element2.Self.gameObject.SetActive(true);
						element2.ModifyPlaceStatus(item.IsPlaced == 1);
						element2.Self.localPosition = item.Position;
						element2.Self.localEulerAngles = item.Angle;
						element2.Freeze(RigidbodyConstraints.None);
						tempElements.Add(element2);
						if (number >= 20)
						{
							number = 0;
							yield return new WaitForFixedUpdate();
						}
					}
				}
				Data.AddDisplayedElements(tempElements, isNew: true);
				Data.SetTotalNumber(Singleton<RecordMgr>.I.Data.ElementTotalNumber);
				_initCompletedCallback.InvokeSafe();
				_initCompletedCallback = null;
			}
			else
			{
				Data.SetTotalNumber(Singleton<RecordMgr>.I.Data.ElementTotalNumber);
				_regenerateView.OnLevelStagePass(0, 1);
				_DisplayElements();
			}
			BlackCutDialog.Hide();
		}
	}
}
