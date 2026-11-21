using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardSystemGuid : DialogBase
{
	[SerializeField]
	private MButton _close;

	[SerializeField]
	private MButton _left;

	[SerializeField]
	private MButton _right;

	[SerializeField]
	private Image _point1;

	[SerializeField]
	private Image _point2;

	[SerializeField]
	private Sprite _yellow;

	[SerializeField]
	private Sprite _blue;

	[SerializeField]
	private RectTransform _content;

	[SerializeField]
	private RectTransform _packet1;

	private float _moveDistanceX;

	private DOTween _tween;

	protected override void Awake()
	{
		base.Awake();
		_close.onClick.AddListener(delegate
		{
			_Close();
		});
		_left.onClick.AddListener(TrunLeft);
		_right.onClick.AddListener(TrunRight);
		_left.interactable = false;
		_moveDistanceX = _content.rect.width + 22f;
		_Open();
	}

	private void TrunLeft(MButton button)
	{
		_right.interactable = true;
		_left.interactable = false;
		_point1.sprite = _yellow;
		_point2.sprite = _blue;
		_packet1.DOLocalMoveX(_packet1.localPosition.x + _moveDistanceX, 0.5f);
	}

	private void TrunRight(MButton button)
	{
		_left.interactable = true;
		_right.interactable = false;
		_point1.sprite = _blue;
		_point2.sprite = _yellow;
		_packet1.DOLocalMoveX(_packet1.localPosition.x - _moveDistanceX, 0.5f);
	}
}
