using UnityEngine;
using UnityEngine.UI;

namespace GameMail
{
	public class MailItem : MonoBehaviour
	{
		[SerializeField]
		private MButton readBtn;

		[SerializeField]
		private GameObject readBgObj;

		[SerializeField]
		private GameObject unReadObj;

		[SerializeField]
		private GameObject newObj;

		[SerializeField]
		private Text titleTex;

		[SerializeField]
		private Text senderTex;

		[SerializeField]
		private Text sendingTimeTex;

		[SerializeField]
		private Text effectiveTimeTex;

		private MailData _mail;

		private void Awake()
		{
			readBtn.onClick.AddListener(_ClickRead);
		}

		public void Init(MailData mail)
		{
			_mail = mail;
			bool active;
			if (mail.Status == 0)
			{
				active = true;
			}
			else
			{
				active = mail.Status == 1 && !string.IsNullOrEmpty(mail.Rewards.Trim());
			}
			unReadObj.SetActive(active);
			readBgObj.SetActive(!active);
			newObj.SetActive(mail.Status == 0);
			var languageDataByKey = Language.LanguageManager.Instance.GetLanguageDataByKey(mail.Title);
			titleTex.text = languageDataByKey != null ? languageDataByKey.value : mail.DefaultTitle;
			senderTex.text = mail.Type == 2 ? mail.Content : mail.Sender;
			sendingTimeTex.text = mail.GetStartTime().ToString("yyyy-MM-dd");
			int num = (int)(mail.GetEndTime() - mail.GetStartTime()).TotalDays;
			effectiveTimeTex.SetLBy(num);
		}

		private void _ClickRead(MButton btn)
		{
			if (MailMgr.I.Read(_mail))
			{
				SingleMailDialog singleMailDialog = SceneMgr.AddTopNode<SingleMailDialog>("Prefabs/Mail/SingleMailDialog");
				singleMailDialog.CloseCallback = delegate
				{
					Init(_mail);
				};
				singleMailDialog.Show(_mail);
			}
		}
	}
}
