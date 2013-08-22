using System;
using SuperWebSocket;
using System.Threading.Tasks;

namespace Talker
{
	public class WebSocketUserCommunication : IUserConnection
	{
		//TODO: IUserCommuncation?
		private WebSocketSession session;
		public WebSocketUserCommunication(WebSocketSession socketSession)
		{
			this.session = socketSession;
		}
	
		public void Write(String clientText)
		{ //TODO: move to bool if fails? cancel and remove connection
			if (session.Connected && !session.InClosing) {
				session.Send(clientText);
			}
		}

		public void Close()
		{
			session.Close(SuperSocket.SocketBase.CloseReason.ClientClosing);
		}

		public string SessionId()
		{
			return session.SessionID;
		}

		public UserConnectionTypes Type {
			get {
				return UserConnectionTypes.WebSocket;
			}
		}

		public User User { get; set; }
	}
}

