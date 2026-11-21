namespace BestHTTP.SignalR.Messages
{
	public sealed class DataMessage : IServerMessage
	{
		MessageTypes IServerMessage.Type => MessageTypes.KeepAlive;

		public object Data { get; private set; }

		void IServerMessage.Parse(object data)
		{
		}
	}
}
