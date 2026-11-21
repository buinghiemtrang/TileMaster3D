namespace BestHTTP.SignalRCore.Messages
{
	public struct CloseWithErrorMessage
	{
		public string error;

		public MessageTypes type => MessageTypes.Handshake;
	}
}
