using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TMPLink : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshProUGUI _text;

	public void OnPointerClick(PointerEventData eventData)
	{
		Vector3 position = Input.mousePosition;
		Camera pressEventCamera = eventData.pressEventCamera;
		int linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, position, pressEventCamera);
		if (linkIndex < 0)
		{
			return;
		}
		if (linkIndex >= _text.textInfo.linkInfo.Length)
		{
			return;
		}
		TMP_LinkInfo linkInfo = _text.textInfo.linkInfo[linkIndex];
		RectTransform rectTransform = _text.rectTransform;
		Vector2 screenPoint = eventData.position;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, pressEventCamera, out var localPoint);
		Application.OpenURL(linkInfo.GetLinkID());
	}
}
