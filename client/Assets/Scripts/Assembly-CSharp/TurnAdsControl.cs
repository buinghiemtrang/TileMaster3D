using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnAdsControl : MonoBehaviour
{
	private enum _Status
	{
		None = 0,
		Start = 1,
		End = 2
	}

	private Action<int> func_callback_end;

	public Transform tfContent;

	public Image imgCellNum;

	public Sprite[] spNums;

	private List<Image> _ltCellNums = new List<Image>();

	private List<int> _ltMultiples = new List<int>();

	private _Status nStatus;

	private float _fSpeed = 500f;

	private int _nCellIndex;

	private int _nInterval = 82;

	private int _nMul = 2;

	private float _nHeight;

	private void Awake()
	{
		int num = -90;
		for (int i = 0; i < 5; i++)
		{
			Image image = UnityEngine.Object.Instantiate<Image>(this.imgCellNum);
			image.transform.SetParent(this.tfContent, false);
			image.transform.SetLocPosY((float)num);
			this._ltCellNums.Add(image);
			num -= this._nInterval;
		}
		if (this.imgCellNum != null && this.imgCellNum.gameObject != null)
		{
			this.imgCellNum.gameObject.SetActive(false);
			this._nHeight = this.tfContent.Rect().height;
		}
	}

	private void Start()
	{
		if (cf_const.victoryStarAdsMul != null)
		{
			int length = cf_const.victoryStarAdsMul.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				int min = cf_const.victoryStarAdsMul[i, 1];
				int max = cf_const.victoryStarAdsMul[i, 2];
				int num = MMath.Random(min, max + 1);
				for (int j = 0; j < num; j++)
				{
					this._ltMultiples.Add(cf_const.victoryStarAdsMul[i, 0]);
				}
			}
		}
		this._ltMultiples.SortR();
		this.UpdateCell();
		this.nStatus = _Status.Start;
	}

	private int GetMul(int i)
	{
		int count = this._ltMultiples.Count;
		int index = (this._nCellIndex + i) % count;
		return this._ltMultiples[index];
	}

	private void SetImage(int i)
	{
		this._ltCellNums[i].sprite = this.spNums[this.GetMul(i) - 2];
	}

	private void UpdateCell()
	{
		for (int i = 0; i < this._ltCellNums.Count; i++)
		{
			this.SetImage(i);
		}
	}

	private void Update()
	{
		if (this.nStatus == _Status.End)
		{
			float num = this._fSpeed * Time.deltaTime * 0.8f;
			for (int i = 0; i < this._ltCellNums.Count; i++)
			{
				Image image = this._ltCellNums[i];
				float y = image.transform.localPosition.y;
				if (y <= 0f)
				{
					float num2 = Math.Min(0f, y + num);
					image.transform.SetLocPosY(num2);
					if (num2 == 0f)
					{
						int mul = this.GetMul(i);
						this._nMul = mul;
						if (this.func_callback_end != null)
						{
							this.func_callback_end(mul);
						}
						this.nStatus = _Status.None;
						return;
					}
				}
				else
				{
					image.transform.SetLocPosY(y + num);
				}
			}
		}
		else if (this.nStatus == _Status.Start)
		{
			float num3 = this._fSpeed * Time.deltaTime;
			foreach (Image ltCellNum in this._ltCellNums)
			{
				ltCellNum.transform.SetLocPosY(ltCellNum.transform.localPosition.y + num3);
			}
			int count = this._ltCellNums.Count;
			int index = count - 1;
			if (count < 1)
			{
				return;
			}
			while (this._ltCellNums[0].transform.localPosition.y >= this._nHeight)
			{
				Image image2 = this._ltCellNums[0];
				image2.transform.SetLocPosY(this._ltCellNums.Last().transform.localPosition.y - (float)this._nInterval);
				this._ltCellNums.RemoveAt(0);
				this._ltCellNums.Add(image2);
				this._nCellIndex++;
				if (this._nCellIndex >= this._ltMultiples.Count)
				{
					this._nCellIndex = 0;
				}
				this.SetImage(index);
				count--;
				if (count == 0)
				{
					break;
				}
			}
		}
	}

	public void SetEnd(Action<int> callback)
	{
		if (this.nStatus == _Status.End)
		{
			if (callback != null)
			{
				callback(this._nMul);
			}
			return;
		}
		this.func_callback_end = callback;
		this.nStatus = _Status.End;
	}
}
