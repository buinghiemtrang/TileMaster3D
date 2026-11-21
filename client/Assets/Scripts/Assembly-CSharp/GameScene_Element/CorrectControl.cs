using System.Collections.Generic;
using GameScene;
using UnityEngine;

namespace GameScene_Element
{
	public class CorrectControl : MonoBehaviour
	{
		[SerializeField]
		private float timeInterval;

		private float _duration;

		private void Update()
		{
			if (GameMgr.I.GameStatus != GameStatus.Playing)
			{
				_duration += Time.deltaTime;
				if (!(_duration < timeInterval))
				{
					_duration = 0f;
					_Correct();
				}
			}
		}

		private void _Correct()
		{
			foreach (KeyValuePair<int, List<Element>> item in ElementsMgr.I.Data.DisplayedElementsId)
			{
				List<Element> value = item.Value;
				for (int i = 0; i < value.Count; i++)
				{
					_CheckPosition(value[i]);
				}
			}
		}

		private void _CheckPosition(Element element)
		{
			if (element.IsLocked)
			{
				return;
			}
			Vector3 position = element.Self.position;
			bool flag = false;
			if (position.x < Config.CorrectLeftBoundary)
			{
				position.x = Config.CreateLeftBoundary;
				flag = true;
			}
			else if (position.x > Config.CorrectRightBoundary)
			{
				position.x = Config.CreateRightBoundary;
				flag = true;
			}
			if (position.z > Config.CorrectForwardBoundary)
			{
				position.z = Config.CreateForwardBoundary;
				flag = true;
			}
			else if (position.z < Config.CorrectBackBoundary)
			{
				position.z = Config.CreateBackBoundary;
				flag = true;
			}
			if (position.y > Config.CorrectUpBoundary)
			{
				position.y = Config.CreateUpBoundary;
			}
			else if (position.y < Config.CorrectDownBoundary)
			{
				position.y = Config.CreateDownBoundary;
			}
			else if (!flag)
			{
				return;
			}
			element.Self.position = position;
		}
	}
}
