namespace _FrameWork.Http
{
	public class RepeatHttpInfo : HttpInfo
	{
		public int RepeatCount;

		public float Duration;

		public bool IsRequestEnd;

		public float RepeatRequestInterval;

		public bool IsSuccessRemoveRequest;

		public bool IsAbortRemoveRequest;
	}
}
