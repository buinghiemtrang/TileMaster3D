using RoyalPass;
using UnityEngine;
using UnityEngine.UI;

namespace _Game._Functions.LuckyPrize.Scripts
{
	public class LuckyPrizeRewardItem
	{
		public int nPosIndex;

		public int nCfgIndex;

		public readonly Transform tfRoot;

		private readonly Image _imgBg;

		private readonly Image _imgIcon;

		private readonly Image _imgLock;

		private readonly Image _imgHint;

		private readonly Image _imgSure;

		private readonly Text _ttCount;

		private readonly MButton _btnItem;

		private cfbt_Stores _cfgStores;

		private cfgLuckyPrize _cfgLuckyPrize;

		public LuckyPrizeRewardItem(Transform tf)
		{
			tfRoot = tf;
			_imgBg = tf.FindC<Image>("Background");
			_imgIcon = _imgBg.FindC<Image>("Icon");
			_imgLock = _imgBg.FindC<Image>("Lock");
			_imgHint = _imgBg.FindC<Image>("Hint");
			_imgSure = _imgBg.FindC<Image>("Sure");
			_ttCount = _imgBg.FindC<Text>("Count");
			_btnItem = _imgBg.GetComponent<MButton>();
			_btnItem.onClick.AddListener(ClickItem);
		}

		public void Initialized(cfgLuckyPrize cfg, int status, Sprite[] sprites)
		{
			_cfgLuckyPrize = cfg;
			if (cfg.storeIndex == -1)
			{
				int[] array = cfg.rewards.ToInt('|');
				Global.GetContents((PropID)array[0], array[1], out Sprite icon, out string count);
				_imgIcon.sprite = icon;
				_imgBg.sprite = sprites[1];
				_ttCount.text = count;
				_ttCount.gameObject.SetActive(true);
				SetStatus(status);
			}
			else
			{
				_cfgStores = DataCfg.cfbt_Storess.Find(c => c.index == cfg.storeIndex);
				_imgIcon.sprite = sprites[0];
				_imgBg.sprite = sprites[2];
				_ttCount.gameObject.SetActive(false);
				SetStatus(status);
			}
		}

		public void SetStatus(int status)
		{
			_imgLock.gameObject.SetActive(status == -1);
			_btnItem.interactable = (status == 0);
			_imgSure.gameObject.SetActive(status == 1);
			_imgHint.gameObject.SetActive(status == 0 && _cfgLuckyPrize.storeIndex != -1);
		}

		public void Destroy()
		{
			_imgHint.gameObject.SetActive(false);
		}

		private void ClickItem(MButton button)
		{
			if (_cfgLuckyPrize.storeIndex == -1)
			{
				return;
			}
			RoyalPassActivityRewardTip royalPassActivityRewardTip = RoyalPassActivityRewardTip.Create();
			Rect rect = button.transform.Rect();
			royalPassActivityRewardTip.yOffset = rect.height * -0.7f;
			royalPassActivityRewardTip.xOffset = rect.width * 0.5f;
			royalPassActivityRewardTip.ShowReward(button.transform, _cfgStores.item.ToInt(2, ',', '|'), false);
			royalPassActivityRewardTip.SetContentsOffPosition(50f, 0f);
		}
	}
}
