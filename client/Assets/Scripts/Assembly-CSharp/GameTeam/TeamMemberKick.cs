using Language;
using UnityEngine;
using UnityEngine.UI;

namespace GameTeam
{
	public class TeamMemberKick : MonoBehaviour
	{
		[SerializeField]
		private Image ImaggeBg;

		private string userId;

		private string userName;

		private void Awake()
		{
			DispatcherMgr.On<ServerDataRootBase>("TeamHttp_KickTeam", _OnKickTeam);
		}

		public void Show(string userId, string userName, Transform tf)
		{
			this.userId = userId;
			this.userName = userName;
			Vector3 vector = transform.InverseTransformPoint(tf.position);
			vector.y -= 80f;
			ImaggeBg.transform.localPosition = vector;
		}

		public void OnClickClose(MButton btn)
		{
			UnityExtend.DestroySelf(this);
		}

		public void OnClickKick(MButton btn)
		{
			gameObject.SetActive(false);
			string text = "";
			LanguageData languageDataByKey = LanguageManager.Instance.GetLanguageDataByKey("Language157");
			if (languageDataByKey != null)
			{
				text = string.Format(languageDataByKey.value, userName);
			}
			Global.CreateMsgBoxDialog(text, (isOk) =>
			{
				if (isOk)
				{
					TeamMaskCtl.I.Show(1f, true);
					TeamDataMgr.I.CurKickUserId = userId;
					TeamHttpMgr.I.KickTeam(userId, userName, -1);
				}
				else
				{
					UnityExtend.DestroySelf(this);
				}
			}, true, "Language202");
		}

		private void _OnKickTeam(ServerDataRootBase root)
		{
			TeamMaskCtl.I.Hide(true);
			UnityExtend.DestroySelf(this);
			if (root.code == 1)
			{
				DispatcherMgr.Emit("MyTeam_Kick");
			}
			else
			{
				Global.ShowMsgTip("Language55");
			}
		}

		private void OnDestroy()
		{
			DispatcherMgr.Off<ServerDataRootBase>("TeamHttp_KickTeam", _OnKickTeam);
		}
	}
}
