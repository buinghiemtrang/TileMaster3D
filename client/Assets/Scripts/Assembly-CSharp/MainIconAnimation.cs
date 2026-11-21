using Scene_Entrance;
using UnityEngine;

public class MainIconAnimation : MonoBehaviour
{
	[SerializeField]
	private Transform _centerPoint;

	[SerializeField]
	protected Sprite _addIconSprite;

	[SerializeField]
	private EntranceDirection _direction;

	protected int _newAddKeyCount;

	private SceneMainAddAnimationControl _addAnimationControl;

	public int NewAddKeyCount => _newAddKeyCount;

	public Sprite AddIconSprite => _addIconSprite;

	public virtual void Init(SceneMainAddAnimationControl addAnimationControl)
	{
		_addAnimationControl = addAnimationControl;
		SetInfo();
	}

	public virtual void SetInfo()
	{
	}

	protected virtual void OnCompleted()
	{
	}

	public bool DoAnimation()
	{
		int newAddKeyCount = _newAddKeyCount;
		if (newAddKeyCount < 1)
		{
			OnCompleted();
		}
		else
		{
			FQDev.Time.TimeManager.SetupInvoke(0.1f, () =>
			{
				AddAnimationItem addAnimationItem = _addAnimationControl.GetAddAnimation(_direction);
				Vector3 position = _centerPoint.position;
				addAnimationItem.Play(position, _newAddKeyCount, _addIconSprite, OnCompleted);
				_newAddKeyCount = 0;
			});
		}
		return 0 < newAddKeyCount;
	}
}
