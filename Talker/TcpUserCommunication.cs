using System;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Talker
{
	public class TcpUserCommunication : IUserConnection
	{
		private TcpClient client;
		public byte[] readingBytes = new byte[4096];

		public TcpUserCommunication(TcpClient userClient)
		{
			client = userClient;
		}

		public void Write(String clientText)
		{
			if (client.Connected && client.GetStream().CanWrite) {
				byte[] writeText = Encoding.UTF8.GetBytes(clientText);
				client.GetStream().Write(writeText, 0, writeText.Length);
			}
		}

		public NetworkStream Stream
		{
			get {
				return client.GetStream();
			}
		}

		public void Close()
		{
			client.Close();
		}

		public UserConnectionTypes Type {
			get {
				return UserConnectionTypes.Tcp;
			}
		}

		public User User { get; set; }
	}
}

