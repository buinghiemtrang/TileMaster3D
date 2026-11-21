namespace BestHTTP.SignalR.Messages
{
	public sealed class ProgressMessage : IServerMessage, IHubMessage
	{
		MessageTypes IServerMessage.Type => MessageTypes.KeepAlive;

		public ulong InvocationId { get; private set; }

		public double Progress { get; private set; }

		void IServerMessage.Parse(object data)
		{
		}
	}
}
