using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace _Game._Functions.LuckyPrize.Scripts
{
	public class LuckyPrizeGamePanel
	{
		public readonly Transform tfRoot;

		private readonly List<SlotItem> _aySlot;

		private readonly ParticleSystemControl _psPrefab;

		private readonly Queue<ParticleSystemControl> _ayEliminateEffPool;

		private readonly LuckyPrizeGameControl _pGameCtrl;

		public LuckyPrizeGamePanel(Transform parent, LuckyPrizeGameControl gameControl)
		{
			_pGameCtrl = gameControl;
			_ayEliminateEffPool = new Queue<ParticleSystemControl>();
			tfRoot = parent.Find("ObjPanel");
			_aySlot = new List<SlotItem>();
			for (int i = 0; i < 4; i++)
			{
				SlotItem slotItem = new SlotItem();
				slotItem.index = i;
				slotItem.point = tfRoot.GetChild(i).localPosition;
				slotItem.block = null;
				_aySlot.Add(slotItem);
			}
			UnityExtend.DestroysInChildren(tfRoot);
		}

		public void Destructor()
		{
			DOTween.Kill("GameBlockMove");
		}

		public void RefreshData()
		{
			foreach (SlotItem item in _aySlot)
			{
				item.block = null;
			}
		}

		public bool AddBlock(LuckyPrizeGameBlockItem block)
		{
			SlotItem slot = _aySlot.Find(a => a.block == null);
			if (slot == null)
			{
				return false;
			}
			SlotItem lastSameSlot = _aySlot.FindLast(a => a.block != null && a.block.nKind == block.nKind);
			if (lastSameSlot != null)
			{
				SlotItem nextSlot = _aySlot[lastSameSlot.index + 1];
				MoveSlotToRightOnce(nextSlot.index);
				slot = nextSlot;
			}
			block.tfRoot.SetParent(tfRoot, true);
			block.IsActive = false;
			slot.block = block;
			block.SetIcon();
			MoveBlockToSlot(slot);
			return true;
		}

		public bool GetSlotFirstThree(out LuckyPrizeGameBlockItem item1, out LuckyPrizeGameBlockItem item2, out LuckyPrizeGameBlockItem item3)
		{
			item1 = null;
			item2 = null;
			item3 = null;
			SlotItem slot0 = _aySlot[0];
			if (slot0.block == null)
			{
				return false;
			}
			SlotItem slot1 = _aySlot[1];
			SlotItem slot2 = _aySlot[2];
			item1 = slot0.block;
			slot0.block = null;
			item2 = slot1.block;
			slot1.block = null;
			item3 = slot2.block;
			slot2.block = null;
			JudgeSlotToLeft();
			MoveSlotToSlotAll();
			return true;
		}

		public bool GetSlotLastThree(out LuckyPrizeGameBlockItem item1, out LuckyPrizeGameBlockItem item2, out LuckyPrizeGameBlockItem item3)
		{
			item1 = null;
			item2 = null;
			item3 = null;
			int count = _aySlot.Count;
			SlotItem slot0 = _aySlot[count - 3];
			if (slot0.block == null)
			{
				return false;
			}
			SlotItem slot1 = _aySlot[count - 2];
			SlotItem slot2 = _aySlot[count - 1];
			item1 = slot0.block;
			slot0.block = null;
			item2 = slot1.block;
			slot1.block = null;
			item3 = slot2.block;
			slot2.block = null;
			JudgeSlotToLeft();
			MoveSlotToSlotAll();
			return true;
		}

		private void SwapSlot(SlotItem slot1, SlotItem slot2)
		{
			LuckyPrizeGameBlockItem block = slot1.block;
			slot1.block = slot2.block;
			slot2.block = block;
			Vector3 endPos = slot1.endPos;
			slot1.endPos = slot2.endPos;
			slot2.endPos = endPos;
		}

		private void MoveBlockToSlot(SlotItem slot)
		{
			slot.endPos = slot.point;
			Tweener tweener = slot.block.tfRoot.DOBlendableLocalMoveBy(slot.endPos - slot.block.tfRoot.localPosition, 0.4f)
				.SetId("GameBlockMove")
				.SetEase(Ease.OutQuad);
			LuckyPrizeGameBlockItem item1;
			LuckyPrizeGameBlockItem item2;
			LuckyPrizeGameBlockItem item3;
			if (CheckPlaneEliminate(out item1, out item2, out item3))
			{
				JudgeSlotToLeft();
				tweener.OnComplete(() =>
				{
					item1.Eliminate();
					item2.Eliminate();
					item3.Eliminate();
					MoveSlotToSlotAll();
					_pGameCtrl.GroupCount--;
					if (_pGameCtrl.GroupCount <= 0)
					{
						_pGameCtrl.SetGameStatus(GameStatus.Victory);
					}
				});
			}
			else if (CheckGameFailure())
			{
				tweener.OnComplete(() =>
				{
					_pGameCtrl.SetGameStatus(GameStatus.Failure);
				});
			}
		}

		private void MoveSlotToSlotOne(SlotItem slot)
		{
			if (slot.block != null)
			{
				if (Vector3.SqrMagnitude(slot.point - slot.endPos) >= 1E-10f)
				{
					if (Vector3.SqrMagnitude(slot.block.tfRoot.localPosition - slot.point) >= 1E-10f)
					{
						slot.block.tfRoot.DOBlendableLocalMoveBy(slot.point - slot.endPos, 0.2f)
							.SetId("GameBlockMove")
							.SetEase(Ease.OutQuad);
						slot.endPos = slot.point;
					}
				}
			}
		}

		private void MoveSlotToRightOnce(int start)
		{
			int count = _aySlot.Count;
			if (start < count - 1)
			{
				for (int i = count - 2; i >= start; i--)
				{
					if (_aySlot[i + 1].block == null)
					{
						SlotItem slot1 = _aySlot[i];
						SlotItem slot2 = _aySlot[i + 1];
						SwapSlot(slot1, slot2);
						MoveSlotToSlotOne(slot1);
						MoveSlotToSlotOne(slot2);
					}
				}
			}
		}

		private void MoveSlotToSlotAll()
		{
			foreach (SlotItem item in _aySlot)
			{
				MoveSlotToSlotOne(item);
			}
		}

		private void JudgeSlotToLeft()
		{
			int count = _aySlot.Count;
			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					SlotItem slot1 = _aySlot[i];
					if (slot1.block == null)
					{
						for (int j = i + 1; j < count; j++)
						{
							SlotItem slot2 = _aySlot[j];
							if (slot2.block != null)
							{
								SwapSlot(slot1, slot2);
								break;
							}
						}
					}
				}
			}
		}

		private ParticleSystemControl PopEliminateEff()
		{
			if (_ayEliminateEffPool.Count != 0)
			{
				return _ayEliminateEffPool.Dequeue();
			}
			return Object.Instantiate(_psPrefab, tfRoot, false);
		}

		private void PushEliminateEff(ParticleSystemControl item)
		{
			item.gameObject.SetActive(false);
			_ayEliminateEffPool.Enqueue(item);
		}

		private void EliminateDiamondOne(LuckyPrizeGameBlockItem item)
		{
			item.tfRoot.gameObject.SetActive(false);
			item.IsActive = false;
		}

		private void EliminateDiamond(LuckyPrizeGameBlockItem item1, LuckyPrizeGameBlockItem item2, LuckyPrizeGameBlockItem item3)
		{
			item1.Eliminate();
			item2.Eliminate();
			item3.Eliminate();
			MoveSlotToSlotAll();
			if (_pGameCtrl != null)
			{
				_pGameCtrl.GroupCount--;
				if (_pGameCtrl.GroupCount <= 0)
				{
					_pGameCtrl.SetGameStatus(GameStatus.Victory);
				}
			}
		}

		private LuckyPrizeGameBlockItem popSlotBlock(SlotItem slot)
		{
			LuckyPrizeGameBlockItem block = slot.block;
			slot.block = null;
			return block;
		}

		private bool CheckPlaneEliminate(out LuckyPrizeGameBlockItem item1, out LuckyPrizeGameBlockItem item2, out LuckyPrizeGameBlockItem item3)
		{
			item1 = null;
			item2 = null;
			item3 = null;
			int count = _aySlot.Count - 2;
			for (int i = 0; i < count; i++)
			{
				SlotItem slot0 = _aySlot[i];
				if (slot0.block != null)
				{
					SlotItem slot1 = _aySlot[i + 1];
					if (slot1.block != null && slot0.block.nKind == slot1.block.nKind)
					{
						SlotItem slot2 = _aySlot[i + 2];
						if (slot2.block != null && slot0.block.nKind == slot2.block.nKind)
						{
							item1 = slot0.block;
							slot0.block = null;
							item2 = slot1.block;
							slot1.block = null;
							item3 = slot2.block;
							slot2.block = null;
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool CheckGameFailure()
		{
			if (_pGameCtrl != null && _pGameCtrl.kGameStatus != GameStatus.Playing)
			{
				return false;
			}
			return _aySlot.TrueForAll(a => a.block != null);
		}
	}
}
