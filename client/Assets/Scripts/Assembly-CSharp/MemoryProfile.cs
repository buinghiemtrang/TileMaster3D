using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

public class MemoryProfile : MonoBehaviour
{
	private float _timer;

	private StringBuilder _stringBuilder = new StringBuilder(500);

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Update()
	{
		_timer += Time.deltaTime;
		if (!(_timer <= 0.2f))
		{
			_timer -= 0.2f;
			_stringBuilder.Clear();
			_stringBuilder.AppendLine(string.Format("Total: {0} MB", Profiler.GetTotalAllocatedMemoryLong() >> 20));
			_stringBuilder.AppendLine(string.Format("Reserved: {0} MB", Profiler.GetTotalReservedMemoryLong() >> 20));
		}
	}

	private void OnGUI()
	{
		GUI.TextArea(new Rect(10f, 30f, 150f, 50f), _stringBuilder.ToString());
	}
}
