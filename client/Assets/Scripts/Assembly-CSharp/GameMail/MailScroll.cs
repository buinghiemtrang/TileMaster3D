using System.Collections;
using System.Collections.Generic;
using Com.TheFallenGames.OSA.Core;

namespace GameMail
{
	public class MailScroll : OSA<MailPwp, MailIvh>
	{
		private bool _isInit;

		private List<MailData> _mails;

		protected override void Awake()
		{
			base.Awake();
			_isInit = false;
		}

		protected override void Start()
		{
			base.Start();
			_Params.DefaultItemHeight = _Params.ItemPrefab.sizeDelta.y;
			_isInit = true;
		}

		public void RefreshItems(List<MailData> mails)
		{
			_mails = mails;
			StopCoroutine("IERefresh");
			StartCoroutine("IERefresh");
		}

		private IEnumerator IERefresh()
		{
			while (!_isInit)
			{
				yield return null;
			}
			ResetItems(_mails != null ? _mails.Count : 0);
		}

		protected override MailIvh CreateViewsHolder(int itemIndex)
		{
			MailIvh mailIvh = new MailIvh();
			mailIvh.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
			return mailIvh;
		}

		protected override void UpdateViewsHolder(MailIvh newOrRecycled)
		{
			MailData mail = null;
			if (newOrRecycled.ItemIndex < _mails.Count)
			{
				mail = _mails[newOrRecycled.ItemIndex];
			}
			newOrRecycled.PItem.Init(mail);
		}
	}
}
