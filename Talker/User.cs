using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Generic;

namespace Talker
{
	public class User
	{
		private TcpClient client;
		private string name;

		public User(TcpClient clientConnection, int clientIndex)
		{
			client = clientConnection;
			name = "User " + clientIndex;
			this.Desc = " is a newbie needing a description. ";
			this.TellBuffer = new List<UserCommuncationBuffer>();
		}

		public void Write(string clientText)
		{
			NetworkStream clientStream = client.GetStream();

			if(clientStream.CanWrite) {
				byte[] writeText = Encoding.UTF8.GetBytes(clientText);

				clientStream.Write(writeText, 0, writeText.Length);
			}
		}

		public void WriteLine(string clientText)
		{
			this.Write(clientText + "\n");
		}

		public NetworkStream Stream
		{
			get { return client.GetStream(); }
		}

		public void Quit()
		{
			//close but don't destory the object since there could still be references to the user still.
			client.Close();
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Desc {
			get;
			set;
		}

		public string Email {
			get;
			set;
		}

		public string Gender {
			get;
			set;
		}

		public int TotalLogins {
			get;
			set;
		}

		public short Age {
			get;
			set;
		}

		public DateTime Logon {
			get;
			set;
		}

		public List<UserCommuncationBuffer> TellBuffer {
			get;
			protected set;
		}
	}
}

