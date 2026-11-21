using DataCenter;
using UnityEngine;
using UnityEngine.UI;

public class UserBaseInfo : MonoBehaviour
{
	[SerializeField]
	private GameObject selectedObj;

	[SerializeField]
	private Image medalImg;

	[SerializeField]
	private Text nameTex;

	[SerializeField]
	private Image nameBackgroundImg;

	[SerializeField]
	private Text levelTex;

	[SerializeField]
	private Image levelBackgroundImg;

	[SerializeField]
	private Text starTex;

	[SerializeField]
	private Image starBackgroundImg;

	[SerializeField]
	private Text backTex;

	[SerializeField]
	private Image backBackgroundImg;

	[SerializeField]
	private Text refreshTex;

	[SerializeField]
	private Image refreshBackgroundImg;

	[SerializeField]
	private Text freezeTex;

	[SerializeField]
	private Image freezeBackgroundImg;

	[SerializeField]
	private Text hintTex;

	[SerializeField]
	private Image hintBackgroundImg;

	[SerializeField]
	private Image loginPlatformImage;

	public void Init(Account account, User user, Bag bag)
	{
	}
}
