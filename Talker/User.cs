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

		public DateTime Logon {
			get;
			set;
		}
	}
}

