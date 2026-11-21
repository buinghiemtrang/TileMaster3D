using UnityEngine;

public class InitUrlProvision : MonoBehaviour
{
	public void ClickUrlTermsOfUse(MButton button)
	{
		Application.OpenURL("https://higgsweb.com/eula.html");
	}

	public void ClickUrlPrivacyPolicy(MButton button)
	{
		Application.OpenURL("https://higgsweb.com/pp.html");
	}
}
