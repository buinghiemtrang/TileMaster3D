namespace FQDev.Logger
{
	public interface ILogAppender
	{
		void Append(LogLevel level, string message, string stackTrace);
	}
}
