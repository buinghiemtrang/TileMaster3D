using System.Linq;
using Festival;
using Language;
using UnityEngine;
using UnityEngine.UI;

namespace ActivityEntrance
{
	public class EntranceItem : MonoBehaviour
	{
		[SerializeField]
		protected MButton enterBtn;

		[SerializeField]
		protected Text descTex;

		[SerializeField]
		protected Text nameTex;

		[SerializeField]
		protected GameObject lockObj;

		[SerializeField]
		protected Image bgImg;

		[SerializeField]
		protected FunctionType type;

		[SerializeField]
		protected Text lvTex;

		protected cfgActivityEntranceInformation Cfg;

		protected virtual void Awake()
		{
			enterBtn.onClick.AddListener(ClickEnter);
		}

		protected virtual void Start()
		{
			Init(DataCfg.cfgActivityEntranceInformations.SingleOrDefault((cfgActivityEntranceInformation sd) => sd.functionType == (int)type));
		}

		public virtual void Init(cfgActivityEntranceInformation cfg)
		{
			if (cfg != null)
			{
				Cfg = cfg;
				LanguageData languageDataByKey = LanguageManager.Instance.GetLanguageDataByKey(cfg.name);
				nameTex.text = ((languageDataByKey != null) ? languageDataByKey.value : cfg.defaultName);
				Sprite sprite = FestivalMgr.I.LoadAsset<Sprite>("ActivityEntranceBg/{0}_" + cfg.bgName);
				if ((bool)sprite)
				{
					bgImg.sprite = sprite;
				}
			}
		}

		protected void SetDesc(string desc, string defaultDesc)
		{
			string text = defaultDesc.Replace("\\n", "\n");
			LanguageData languageDataByKey = LanguageManager.Instance.GetLanguageDataByKey(desc);
			if (languageDataByKey != null)
			{
				text = languageDataByKey.value;
			}
			descTex.text = text;
		}

		protected virtual void ClickEnter(MButton btn)
		{
		}
	}
}
